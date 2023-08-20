using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink.Entities;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Suruga.Handlers;

internal sealed class ResponseHelper
{
    private static readonly DiscordWebhookBuilder s_webhookBuilder = new();

    internal static async Task CreateTrackChoiceListAsync(BaseContext ctx, ImmutableList<LavalinkTrack> tracks)
    {
        DiscordEmbedBuilder embed = new();

        for (int i = 0; i < tracks.Count; i++)
        {
            embed.WithDescription("Please select a provider in order to play the specified track.");

            StringBuilder stringBuilder = new();

            string domain = tracks[i].Info.Uri.Host.StartsWith("www.") ? tracks[i].Info.Uri.Host[4..] : tracks[i].Info.Uri.Host;
            domain = tracks[i].Info.Uri.Host.StartsWith("https://www.") ? tracks[i].Info.Uri.Host[12..] : domain;
            domain = domain.Split('.')[0];

            stringBuilder.AppendLine($"{i}. {char.ToUpper(domain[0])} {tracks[i].Info.Title} ({tracks[i].Info.Length})");

            embed.WithDescription(stringBuilder.ToString());
        }

        s_webhookBuilder.AddComponents(new DiscordActionRowComponent(GenerateButtons(tracks.Count)));
        await ctx.EditResponseAsync(s_webhookBuilder);
       
        s_webhookBuilder.Clear();
    }

    private static IEnumerable<DiscordButtonComponent> GenerateButtons(int amount)
    {
        for (int i = 1; i < amount; i++)
        {
            yield return new DiscordButtonComponent(ButtonStyle.Primary, emoji: new($"U+003{amount}"));
        }
    }
}
