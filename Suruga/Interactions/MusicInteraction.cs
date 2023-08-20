using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Suruga.Handlers;
using Suruga.Infrastructure;
using Suruga.Logging;
using System.Collections.Immutable;

namespace Suruga.Interactions;

public sealed class MusicInteraction : ApplicationCommandsModule
{
    private readonly IConfiguration _configuration = Program.Host.Services.GetRequiredService<IConfiguration>();

    private readonly Logger _logger = Program.Host.Services.GetRequiredService<Logger>();

    [SlashCommand("join", "Joins a voice channel.")]
    public async Task<LavalinkGuildPlayer?> JoinAsync(
        InteractionContext ctx,
        [Option("channel", "A specific voice channel to join"), ChannelTypes(ChannelType.Voice | ChannelType.Stage)] DiscordChannel? channel = null)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (channel?.Users.FirstOrDefault(member => member == ctx.Guild.CurrentMember) is not null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I am already in {channel.Mention}."));
            return null;
        }

        if (channel is null && ctx.Member.VoiceState is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You must join a voice or stage channel."));
            return null;
        }

        LavalinkExtension lavalink = ctx.Client.GetLavalink();

        if (!lavalink.ConnectedSessions.Any())
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("An internal issue has occured."));
            _logger.LogError("Failed to retrieve Lavalink sessions!");

            return null;
        }

        LavalinkSession session = lavalink.ConnectedSessions.Values.First();

        LavalinkGuildPlayer player = await session.ConnectAsync(
            channel ?? ctx.Member.VoiceState.Channel,
            !bool.TryParse(_configuration.GetRequiredSection("Bot")["DeafenInVoiceChannel"], out bool shouldDeafen) || shouldDeafen);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Joined {channel?.Mention ?? ctx.Member.VoiceState.Channel.Mention}"));
        return player;
    }

    [SlashCommand("leave", "Leaves the voice channel.")]
    public async Task LeaveAsync(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        LavalinkExtension lavalink = ctx.Client.GetLavalink();
        LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

        if (guildPlayer is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not connected to any voice or stage channel."));
            return;
        }

        string voiceChannelName = guildPlayer.Channel.Mention;

        await guildPlayer.DisconnectAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Left {voiceChannelName}!"));
    }

    [SlashCommand("play", "Play a track.")]
    public async Task PlayAsync(InteractionContext ctx, [Option("query", "The query to search for")] string query)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (ctx.Member.VoiceState is null || ctx.Member.VoiceState.Channel is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must join a voice channel first."));
            return;
        }

        LavalinkExtension lavalink = ctx.Client.GetLavalink();
        LavalinkGuildPlayer? player = lavalink.GetGuildPlayer(ctx.Guild) ?? await JoinAsync(ctx, ctx.Member.VoiceState.Channel);

        if (player is not null)
        {
            LavalinkTrackLoadingResult loadResult = await player.LoadTracksAsync(
                LavalinkSearchType.Youtube | LavalinkSearchType.SoundCloud | LavalinkSearchType.Deezer | LavalinkSearchType.Plain,
                query);

            if (loadResult.LoadType is LavalinkLoadResultType.Empty or LavalinkLoadResultType.Error)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I did not find anything for {query}."));
                return;
            }

            List<LavalinkTrack> tracks = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Track => loadResult.GetResultAs<List<LavalinkTrack>>(),
                LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks,
                LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>(),
                _ => throw new InvalidOperationException("Unexpected load result type.")
            };

            if (loadResult.LoadType is LavalinkLoadResultType.Track)
            {
                // todo: use interaction, create webhook builder, wait for user to choose button, do 'x' action on button, play music. done
                ctx.Client.GetInteraction();
                await ResponseHelper.CreateTrackChoiceListAsync(ctx, tracks.DistinctBy(track => track.Info.Uri).Take(5).ToImmutableList());
                
                await player.PlayAsync(loadResult.GetResultAs<LavalinkTrack>());

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Now playing {query}"));
                return;
            }
            else if (loadResult.LoadType is LavalinkLoadResultType.Playlist or LavalinkLoadResultType.Search)
            {
                foreach (LavalinkTrack track in tracks)
                {
                    player.AddToQueue(new QueueEntry { Track = track }, track);
                }

                await player.PlayAsync(player.QueueEntries[0].Track);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {tracks.Count} tracks to the queue. Now playing {tracks[0].Info.Title}"));
                return;
            }


        }
    }
}
