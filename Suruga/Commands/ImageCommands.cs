using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using Image = SixLabors.ImageSharp.Image;

#pragma warning disable CS1591
namespace Suruga.Commands;

[Group("profile", "User profile related commands.")]
public sealed class ImageCommands(IHttpClientFactory clientFactory) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly HttpClient _http = clientFactory.CreateClient();

    [SlashCommand("fetch", "Fetches your or somebody else's profile picture.")]
    public async Task FetchProfileAsync([Summary(description: "The user whose profile picture to adjust.")] SocketGuildUser? user = null)
    {
        SocketGuildUser member = user ?? (SocketGuildUser)Context.User;

        Uri url = new(member.GetGuildAvatarUrl(ImageFormat.WebP, size: 2048) ?? member.GetDisplayAvatarUrl(ImageFormat.WebP, size: 2048));
        await using Stream attachmentStream = await _http.GetStreamAsync(url);

        await RespondWithFileAsync(attachmentStream, Path.GetFileName(url.LocalPath));
    }

    [SlashCommand("brightness", "Mutates the brightness of a profile picture.")]
    public async Task BrightnessAsync
    (
        [Summary(description: "The amount by which to adjust the brightness")]
        [MinValue(0), MaxValue(100)]
        double amount,
        
        [Summary(description: "The user whose profile picture to adjust.")]
        SocketGuildUser? user = null
    )
    {
        SocketGuildUser member = user ?? (SocketGuildUser)Context.User;
        KeyValuePair<string, MemoryStream> attachment = await GetProcessedAvatarAsync(member, x => x.Brightness((float)(amount / 100)));

        await using MemoryStream attachmentStream = attachment.Value;
        await RespondWithFileAsync(attachmentStream, attachment.Key);
    }

    [SlashCommand("orient", "Orients a profile picture so that it is suitable for viewing.")]
    public async Task OrientAsync([Summary(description: "The user whose profile picture to adjust.")] SocketGuildUser? user = null)
    {
        SocketGuildUser member = user ?? (SocketGuildUser)Context.User;
        KeyValuePair<string, MemoryStream> attachment = await GetProcessedAvatarAsync(member, x => x.AutoOrient());

        await using MemoryStream attachmentStream = attachment.Value;
        await RespondWithFileAsync(attachmentStream, attachment.Key);
    }

    [SlashCommand("guassian", "Applies a guassian filter on a profile picture.")]
    public async Task GuassianAsync
    (
        [Summary(description: "The type of Gaussian filter to apply")]
        [Choice("blur", "blur")]
        [Choice("sharpen", "sharpen")]
        string filter,
        
        [Summary(description: "The user whose profile picture to adjust.")]
        SocketGuildUser? user = null
    )
    {
        SocketGuildUser member = user ?? (SocketGuildUser)Context.User;
        KeyValuePair<string, MemoryStream> attachment = await GetProcessedAvatarAsync(member, x => _ = filter == "blur" ? x.GaussianBlur() : x.GaussianSharpen()).ConfigureAwait(false);

        await using MemoryStream attachmentStream = attachment.Value;
        await RespondWithFileAsync(attachmentStream, $"{Random.Shared.Next()}.webp");
    }

    [SlashCommand("replace", "Replaces a color from a profile picture with another.")]
    public async Task ChangeColorAsync
    (
        [Summary(description: "The current color in hexadecimal format (e.g. '#RRGGBB').")] string oldColor,
        [Summary(description: "The new color in hexadecimal format (e.g '#RRGGBB').")] string newColor,
        [Summary(description: "The user whose profile picture to adjust.")] SocketGuildUser? user = null
    )
    {
        Vector4 rgbColor = HexToRgbAsVector4(oldColor);
        Vector4 rgbNewColor = HexToRgbAsVector4(newColor);

        SocketGuildUser member = user ?? (SocketGuildUser)Context.User;

        KeyValuePair<string, MemoryStream> attachment = await GetProcessedAvatarAsync(member, ctx => ctx.ProcessPixelRowsAsVector4(row =>
        {
            for (int x = 0; x < row.Length; x++)
            {
                if (row[x] == rgbColor)
                {
                    row[x] = rgbNewColor;
                }
            }
        }));

        await using MemoryStream attachmentStream = attachment.Value;
        await RespondWithFileAsync(attachmentStream, attachment.Key);
        return;

        static Vector4 HexToRgbAsVector4(string input)
        {
            uint color = Convert.ToUInt32(input.TrimStart('#'), 16);

            float r = ((byte)((color >> 16) & 0xFF)) / 255f;
            float g = ((byte)((color >> 8) & 0xFF)) / 255f;
            float b = ((byte)(color & 0xFF)) / 255f;

            return new Vector4(r, g, b, 1f); // Normalize RGB values to range [0, 1]
        }
    }

    [SlashCommand("pixelate", "Pixelates your profile picture.")]
    public async Task PixelateAsync
    (
        [Summary(description: "The size of the pixels.")]
        [MinValue(1)]
        int size,
        
        [Summary(description: "The user whose profile picture to adjust.")]
        SocketGuildUser? user = null
    )
    {
        SocketGuildUser member = user ?? (SocketGuildUser)Context.User;
        KeyValuePair<string, MemoryStream> attachment = await GetProcessedAvatarAsync(member, x => x.Saturate(size));

        await using MemoryStream attachmentStream = attachment.Value;
        await RespondWithFileAsync(attachmentStream, attachment.Key);
    }

    [SlashCommand("saturate", "Saturates your profile picture.")]
    public async Task SaturateAsync
    (
        [Summary(description: "The amount of saturation to apply")]
        [MinValue(0), MaxValue(100)]
        double amount,
        
        [Summary(description: "The user whose profile picture to adjust.")] SocketGuildUser? user = null
    )
    {
        SocketGuildUser member = user ?? (SocketGuildUser)Context.User;
        KeyValuePair<string, MemoryStream> attachment = await GetProcessedAvatarAsync(member, x => x.Saturate((float)(amount / 20f)));

        await using MemoryStream attachmentStream = attachment.Value;
        await RespondWithFileAsync(attachmentStream, attachment.Key);
    }

    private async Task<KeyValuePair<string, MemoryStream>> GetProcessedAvatarAsync(SocketGuildUser user, Action<IImageProcessingContext> ctx)
    {
        string url = GetAvatarUrl();

        await using Stream avatarStream = await _http.GetStreamAsync(url);
        using Image avatar = await Image.LoadAsync(avatarStream);
        using Image clonedAvatar = avatar.Clone(ctx);
        MemoryStream attachmentStream = new();

        await clonedAvatar.SaveAsWebpAsync(attachmentStream);
        return KeyValuePair.Create(GetAvatarFileName(url), attachmentStream);

        string GetAvatarUrl()
            => user.GetGuildAvatarUrl(ImageFormat.WebP, size: 2048) ?? user.GetDisplayAvatarUrl(ImageFormat.WebP, size: 2048);
        
        string GetAvatarFileName(string avatarUrl)
            => Path.GetFileName(new Uri(avatarUrl).LocalPath);
    }
}
