using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Suruga.DNet.Handlers;

public sealed class EmbedHandler
{
    private readonly IConfiguration _configuration;

    public EmbedHandler(IConfiguration configuration)
        => _configuration = configuration;

    internal async Task CreateSuccessfulResponse(
        SocketGuildUser user,
        SocketTextChannel channel,
        string description = null,
        string title = null,
        string imageUrl = null,
        uint? customColor = null)
        => await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithColor(customColor ?? GetSuccessfulResponseColorOrDefault())
                .WithTitle(title ?? string.Empty)
                .WithDescription(description ?? string.Empty)
                .WithImageUrl(imageUrl ?? string.Empty)
                .WithCurrentTimestamp()
                .WithFooter(user.DisplayName, user.GetDisplayAvatarUrl())
                .Build());

    internal async Task CreateSuccessfulResponse(
        SocketInteraction interaction,
        string description = null,
        string title = null,
        string imageUrl = null,
        uint? customColor = null)
        => await interaction.RespondAsync(embed: new EmbedBuilder()
                .WithColor(customColor ?? GetSuccessfulResponseColorOrDefault())
                .WithTitle(title ?? string.Empty)
                .WithDescription(description ?? string.Empty)
                .WithImageUrl(imageUrl ?? string.Empty)
                .WithCurrentTimestamp()
                .WithFooter(interaction.User.Username, interaction.User.GetAvatarUrl())
                .Build());

    internal async Task CreateUnsuccessfulResponse(
        SocketGuildUser user,
        SocketTextChannel channel,
        string description = null,
        string title = null)
        => await channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithColor(GetUnsuccessfulResponseColorOrDefault())
                .WithTitle(title ?? string.Empty)
                .WithDescription(description ?? string.Empty)
                .WithCurrentTimestamp()
                .WithFooter(user.DisplayName, user.GetDisplayAvatarUrl())
                .Build());

    internal async Task CreateUnsuccessfulResponse(
        SocketInteraction interaction,
        string description = null,
        string title = null)
        => await interaction.RespondAsync(embed: new EmbedBuilder()
            .WithColor(GetUnsuccessfulResponseColorOrDefault())
            .WithTitle(title ?? string.Empty)
            .WithDescription(description ?? string.Empty)
            .WithCurrentTimestamp()
            .WithFooter(interaction.User.Username, interaction.User.GetAvatarUrl())
            .Build());

    private uint GetSuccessfulResponseColorOrDefault()
        => uint.TryParse(_configuration["HexColorOnSuccessfulCommand"], out uint successfulEmbedHexColor)
            ? successfulEmbedHexColor
            : 0x3CB043;

    private uint GetUnsuccessfulResponseColorOrDefault()
        => uint.TryParse(_configuration["HexColorOnUnsuccessfulCommand"], out uint unsuccessfulEmbedHexColor)
            ? unsuccessfulEmbedHexColor
            : 0xD0312D;
}
