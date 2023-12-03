using Lavalink4NET;
using Lavalink4NET.Artwork;
using Lavalink4NET.Clients;
using Lavalink4NET.Lyrics;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Remora.Discord;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Results;
using Suruga.Handlers;
using Suruga.Intermediary;
using System.Collections.Immutable;
using System.ComponentModel;

namespace Suruga.Commands;

public sealed class AudioCommands
(
    ICommandContext ctx,
    IDiscordRestChannelAPI channelApi,
    IFeedbackService feedback,
    IAudioService audio,
    IArtworkService artworkService,
    ILyricsService lyricsService,
    TrackSelectionNotificationMediator mediator,
    ILogger<AudioCommands> logger
) : CommandGroup
{
    [Command("play")]
    [Description("Plays a track based on a given query or URL.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> PlayAsync([Description("Keywords to search for")] [DiscordTypeHint(TypeHint.String)] string query)
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: true, [PlayerPrecondition.Playing, PlayerPrecondition.NotPlaying]);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        TrackLoadResult tracksResult = await audio.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTube, cancellationToken: CancellationToken);

        if (!tracksResult.HasMatches)
        {
            return (Result)await feedback.SendContextualErrorAsync("No tracks were found.", ct: CancellationToken);
        }

        if (tracksResult.Track is LavalinkTrack singleTrack && tracksResult.Tracks.Length == 1)
        {
            if (player.State is PlayerState.Playing)
            {
                await player.Queue.AddAsync(new TrackQueueItem(new(tracksResult.Track)), CancellationToken);
                return (Result)await feedback.SendContextualInfoAsync($"Enqueued [{tracksResult.Track.Title}]({tracksResult.Track?.Uri})", ct: CancellationToken);
            }

            await player.PlayAsync(singleTrack, cancellationToken: CancellationToken);
            return (Result)await feedback.SendContextualSuccessAsync($"Now playing: [{singleTrack.Title}]({singleTrack?.Uri})", ct: CancellationToken);
        }

        if (tracksResult.IsPlaylist)
        {
            foreach (LavalinkTrack track in tracksResult.Tracks)
            {
                await player.Queue.AddAsync(new TrackQueueItem(new(track)), CancellationToken);
            }

            return (Result)await feedback.SendContextualInfoAsync($"Enqueued {tracksResult.Tracks.Length} tracks.", ct: CancellationToken);
        }

        TrackSelectionEmbedHelper.GenerateButtons(tracksResult.Tracks);

        Result<IReadOnlyList<IMessage>> messageResult = await feedback.SendContextualInfoAsync
        (
            TrackSelectionEmbedHelper.EmbedTrackBuilder.ToString(),
            options: new FeedbackMessageOptions(MessageComponents: new[] { new ActionRowComponent(TrackSelectionEmbedHelper.TrackSelectionButtons) }),
            ct: CancellationToken
        );

        mediator.Add(new TrackSelectionNotification(player, player.State, tracksResult.Tracks));
        return Result.FromSuccess();
    }

    [Command("leave")]
    [Description("Leaves from the voice channel.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> LeaveAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync();

        if (player is null)
        {
            return Result.FromSuccess();
        }

        Result<IChannel> voiceChannelResult = await channelApi.GetChannelAsync(DiscordSnowflake.New(player.VoiceChannelId), CancellationToken);

        if (!voiceChannelResult.IsDefined(out IChannel? vc))
        {
            return (Result)await feedback.SendContextualErrorAsync("Failed to find voice channel.");
        }

        await player.DisconnectAsync();
        await player.DisposeAsync();

        return (Result)await feedback.SendContextualSuccessAsync($"Left {vc?.Name.OrDefault()}", ct: CancellationToken);
    }

    [Command("resume")]
    [Description("Resumes the current stopped track.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> ResumeAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: PlayerPrecondition.Paused);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        await player.ResumeAsync(CancellationToken);
        return (Result)await feedback.SendContextualSuccessAsync($"Resumed [{player.CurrentTrack.Title}]({player.CurrentTrack?.Uri}).", ct: CancellationToken);
    }

    [Command("pause")]
    [Description("Pauses the current playing track.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> PauseAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: [PlayerPrecondition.NotPaused]);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        await player.PauseAsync(CancellationToken);
        return (Result)await feedback.SendContextualSuccessAsync($"Paused [{player.CurrentTrack.Title}]({player.CurrentTrack?.Uri}).", ct: CancellationToken);
    }

    [Command("skip")]
    [Description("Skips the current playing track and moves on to the next one in the queue.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> SkipAsync([MinValue(1)][DiscordTypeHint(TypeHint.Integer)] int numberOfTracks = 1)
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: [PlayerPrecondition.Playing, PlayerPrecondition.QueueNotEmpty]);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        await player.SkipAsync(numberOfTracks, CancellationToken);
        return (Result)await feedback.SendContextualSuccessAsync($"Skipped [{player.CurrentTrack.Title}]({player.CurrentTrack?.Uri}).", ct: CancellationToken);
    }

    [Command("stop")]
    [Description("Stops playing and removes the queue.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> StopAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: [PlayerPrecondition.Playing]);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        LavalinkTrack currentTrack = player.CurrentTrack;

        await player.StopAsync(CancellationToken);
        return (Result)await feedback.SendContextualSuccessAsync($"Stopped [{currentTrack?.Title}]({currentTrack?.Uri}).", ct: CancellationToken);
    }

    [Command("shuffle")]
    [Description("Shuffles the queue.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> ShuffleAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: PlayerPrecondition.QueueNotEmpty);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        await player.Queue.ShuffleAsync(CancellationToken);
        return (Result)await feedback.SendContextualSuccessAsync($"Queue shuffled.", ct: CancellationToken);
    }

    [Command("loop")]
    [Description("Loops the current track.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> LoopAsync(bool loopQueue = false)
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: PlayerPrecondition.QueueNotEmpty);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        _ = loopQueue == false ? player.RepeatMode = TrackRepeatMode.Track : TrackRepeatMode.Queue;
        return (Result)await feedback.SendContextualSuccessAsync($"Enabled looping.", ct: CancellationToken);
    }

    [Command("volume")]
    [Description("Sets the volume of the player.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> SetVolumeAsync([MinValue(0)] [MaxValue(100)] [DiscordTypeHint(TypeHint.Integer)] int value)
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync();

        if (player is null)
        {
            return Result.FromSuccess();
        }

        float newVolume = value / 100f;

        await player.SetVolumeAsync(newVolume, CancellationToken);
        return (Result)await feedback.SendContextualSuccessAsync($"Volume changed to {value}%.", ct: CancellationToken);
    }

    [Command("currentvolume")]
    [Description("Displays the current volume.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> ShowVolumeAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync();

        if (player is null)
        {
            return Result.FromSuccess();
        }

        return (Result)await feedback.SendContextualSuccessAsync($"The volume of the player is currently at {player.Volume * 100f}%.", ct: CancellationToken);
    }

    [Command("position")]
    [Description("Shows the latest position of the playing track.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> ShowPositionAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: PlayerPrecondition.Playing);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        return (Result)await feedback.SendContextualSuccessAsync($"Position: {player.Position?.Position.Hours} / ({player.CurrentTrack?.Duration})", ct: CancellationToken);
    }

    [Command("thumbnail")]
    [Description("Fetches the thumbnail of the current track.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> FetchThumbnailAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: PlayerPrecondition.Playing);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        if (player.CurrentTrack is not LavalinkTrack track)
        {
            return (Result)await feedback.SendContextualErrorAsync("Current track in queue is invalid.", ct: CancellationToken);
        }

        Uri? uri = await artworkService.ResolveAsync(track, CancellationToken);

        if (uri is not Uri artworkUri)
        {
            return (Result)await feedback.SendContextualErrorAsync($"Could not find thumbnail for [{track.Title}]({track?.Uri}).", ct: CancellationToken);
        }

        return (Result)await feedback
            .SendContextualEmbedAsync(new EmbedBuilder()
            .WithImageUrl(artworkUri.ToString())
            .Build()
            .Entity);
    }

    [Command("lyrics")]
    [Description("Fetches the lyrics of the current track.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> FetchLyricsAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: PlayerPrecondition.Playing);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        string? nullableLyrics = await lyricsService.GetLyricsAsync(player.CurrentTrack, CancellationToken);

        if (nullableLyrics is not string lyrics)
        {
            return (Result)await feedback.SendContextualErrorAsync($"Could not find a lyrics source for [{player.CurrentTrack.Title}]({player.CurrentTrack?.Uri}).", ct: CancellationToken);
        }

        return (Result)await feedback.SendContextualSuccessAsync(lyrics, ct: CancellationToken);
    }

    [Command("pitch")]
    [Description("Increases or decreases the pitch of the current track.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> ChangePitchAsync([MinValue(25)] [MaxValue(200)] [DiscordTypeHint(TypeHint.Integer)] int value)
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: PlayerPrecondition.Playing);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        float newPitch = value / 100f;

        player.Filters.Timescale = new(Pitch: newPitch);
        await player.Filters.CommitAsync(CancellationToken);

        return (Result)await feedback.SendContextualSuccessAsync($"Changed the speed of the track to {value}%", ct: CancellationToken);
    }

    [Command("speed")]
    [Description("Increases or decreases the speed of the current track.")]
    [RequireContext(ChannelType.GuildText)]
    public async ValueTask<Result> ChangeSpeedAsync([MinValue(25)] [MaxValue(200)] [DiscordTypeHint(TypeHint.Integer)] int value)
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(preconditions: PlayerPrecondition.Playing);

        if (player is null)
        {
            return Result.FromSuccess();
        }

        float newSpeed = value / 100f;

        player.Filters.Timescale = new(newSpeed);
        await player.Filters.CommitAsync(CancellationToken);

        return (Result)await feedback.SendContextualSuccessAsync($"Changed the speed of the track to {value}%", ct: CancellationToken);
    }

    private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = false, params IPlayerPrecondition[] preconditions)
    {
        async ValueTask<Result> SendPreconditionErrorMessage(PlayerResult<QueuedLavalinkPlayer> result)
        {
            string message = result.Status switch
            {
                PlayerRetrieveStatus.UserNotInVoiceChannel => "You must be in a voice channel.",
                PlayerRetrieveStatus.BotNotConnected => "I am not connected to any voice channel.",
                PlayerRetrieveStatus.VoiceChannelMismatch => "You must be in the same voice channel as the bot.",

                PlayerRetrieveStatus.PreconditionFailed when result.Precondition == PlayerPrecondition.Playing => "I am currently playing a track.",
                PlayerRetrieveStatus.PreconditionFailed when result.Precondition == PlayerPrecondition.NotPlaying => "I must be playing a track first.",
                PlayerRetrieveStatus.PreconditionFailed when result.Precondition == PlayerPrecondition.Paused => "I am not paused.",
                PlayerRetrieveStatus.PreconditionFailed when result.Precondition == PlayerPrecondition.NotPaused => "I am already paused.",
                PlayerRetrieveStatus.PreconditionFailed when result.Precondition == PlayerPrecondition.QueueEmpty => "The queue is empty.",
                _ => $"An unknown player status has been retrieved. If this is a bug, you can create an issue report [here](https://github.com/waylaa/Suruga).",
            };

            return (Result)await feedback.SendContextualErrorAsync(message, ct: CancellationToken);
        }

        PlayerResult<QueuedLavalinkPlayer> playerResult = await audio.Players.RetrieveAsync
        (
            ctx,
            PlayerFactory.Queued,
            new PlayerRetrieveOptions
            (
                connectToVoiceChannel == false ? PlayerChannelBehavior.None : PlayerChannelBehavior.Join,
                MemberVoiceStateBehavior.AlwaysRequired,
                ImmutableArray.Create(preconditions)
            ),
            CancellationToken
        );

        if (!playerResult.IsSuccess)
        {
            await SendPreconditionErrorMessage(playerResult);
            return null;
        }

        return playerResult.Player;
    }
}
