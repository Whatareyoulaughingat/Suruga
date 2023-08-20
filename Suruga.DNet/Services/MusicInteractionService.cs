using Discord;
using Discord.WebSocket;
using Suruga.DNet.Handlers;
using Suruga.DNet.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Player.Filters;
using Victoria.Responses.Search;

namespace Suruga.DNet.Services;

public sealed class MusicInteractionService
{
    private ConcurrentBag<IDisposable> _disposableEvents;

    private readonly Logger _logger;

    private readonly EmbedHandler _embed;

    private readonly LavaNode _lavaNode;

    public MusicInteractionService(Logger programLogger, EmbedHandler embed, LavaNode lavaNode)
    {
        _logger = programLogger;
        _embed = embed;
        _lavaNode = lavaNode;
    }

    internal async Task LeaveAsync(SocketInteraction interaction)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, description: "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, description: "I'm not in a voice channel!");
            return;
        }

        await _embed.CreateSuccessfulResponse(interaction, description: $"Left {player.VoiceChannel.Name}");

        try
        {
            foreach (IDisposable registeredEvent in _disposableEvents)
            {
                registeredEvent?.Dispose();
            }

            await player.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to dispose a player and its events.", ex);
        }
    }

    internal async Task PlayAsync(SocketInteraction interaction, string query)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "Provide a URL.");
            return;
        }

        /*
        if (!Uri.IsWellFormedUriString(query, UriKind.Absolute))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "The URL your provided is not valid.");
            return;
        }
        */

        if (!_lavaNode.HasPlayer((interaction.User as SocketGuildUser).Guild))
        {
            IDisposable trackEndEvent = Observable.FromEventPattern<TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack>>(
                handler => _lavaNode.OnTrackEnd += (args) => OnTrackEndedAsync(interaction, args),
                handler => _lavaNode.OnTrackEnd -= (args) => OnTrackEndedAsync(interaction, args))
            .Subscribe();

            IDisposable trackStuckEvent = Observable.FromEventPattern<TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack>>(
                handler => _lavaNode.OnTrackStuck += (args) => OnTrackStuckAsync(interaction, args),
                handler => _lavaNode.OnTrackStuck -= (args) => OnTrackStuckAsync(interaction, args))
            .Subscribe();

            IDisposable trackExceptionEvent = Observable.FromEventPattern<TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack>>(
                handler => _lavaNode.OnTrackException += (args) => OnTrackExceptionAsync(interaction, args),
                handler => _lavaNode.OnTrackException -= (args) => OnTrackExceptionAsync(interaction, args))
            .Subscribe();

            _disposableEvents = new() { trackEndEvent, trackStuckEvent, trackExceptionEvent };
        }

        LavaPlayer<LavaTrack> player;

        try
        {
            player = await _lavaNode.JoinAsync(interaction.User.MutualGuilds.First(x => x.Id == interaction.GuildId).GetUser(interaction.User.Id).VoiceChannel);
            await _embed.CreateUnsuccessfulResponse(interaction, $"Joined {player.VoiceChannel.Name}");
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("A player has failed to join a voice channel.", ex);

            return;
        }

        SearchResponse response = await _lavaNode.SearchAsync(SearchType.YouTube | SearchType.SoundCloud | SearchType.Direct, query);

        if (response.Status is SearchStatus.LoadFailed)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "Failed to load songs!");
            return;
        }

        if (response.Status is SearchStatus.NoMatches)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, $"No matches were found for {query}");
            return;
        }

        try
        {
            if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
            {
                player.Vueue.Enqueue(response.Tracks);
                await _embed.CreateUnsuccessfulResponse(interaction, $"Enqueued {response.Tracks.Count} songs");
            }
            else
            {
                LavaTrack track = response.Tracks.First();

                await player.PlayAsync(track);
                await _embed.CreateSuccessfulResponse(interaction, $"Now Playing [{track.Title}]({track.Url})");
            }
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to play a song, a playlist or a queued song", ex);
        }
    }

    internal async Task StopAsync(SocketInteraction interaction)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        if (player.PlayerState is PlayerState.Stopped or PlayerState.None)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "A song must be playing or be paused in order to stop!");
            return;
        }

        try
        {
            await player.StopAsync();
            await _embed.CreateSuccessfulResponse(interaction, $"Stopped Playing {player.Track.Title}");
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to stop a player.", ex);
        }
    }

    internal async Task SkipAsync(SocketInteraction interaction)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        if (player.PlayerState is PlayerState.Stopped or PlayerState.None)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "A song must be playing or be paused in order to skip!");
            return;
        }

        if (player.Vueue.Count <= 1)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "There isn't a queue in order to skip!");
            return;
        }

        try
        {
            (LavaTrack Skipped, _) = await player.SkipAsync();
            await _embed.CreateSuccessfulResponse(interaction, $"Skipped {Skipped.Title}");
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to skip a song.", ex);
        }
    }

    internal async Task PauseAsync(SocketInteraction interaction)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        if (player.PlayerState is PlayerState.Paused or PlayerState.Stopped or PlayerState.None)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "A song must be playing in order to pause!");
            return;
        }

        try
        {
            await _embed.CreateSuccessfulResponse(interaction, $"Paused {player.Track.Title}");
            await player.PauseAsync();
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to pause a song.", ex);
        }
    }

    internal async Task ResumeAsync(SocketInteraction interaction)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        if (player.PlayerState is PlayerState.Playing or PlayerState.Stopped or PlayerState.None)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "A song must be paused in order to resume!");
            return;
        }

        try
        {
            await player.ResumeAsync();
            await _embed.CreateSuccessfulResponse(interaction, $"Resumed {player.Track.Title}");
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to resume a song.", ex);
        }
    }

    internal async Task SetVolumeAsync(SocketInteraction interaction, int volume)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        if (volume > 200 || volume < 0)
        {
            await _embed.CreateSuccessfulResponse(interaction, "The range of the volume must be between 0 and 200.");
            return;
        }

        try
        {
            await player.SetVolumeAsync(volume);
            await _embed.CreateSuccessfulResponse(interaction, $"Increased the volume to {volume}%");
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to set the volume of a player.", ex);
        }
    }

    internal async Task ShuffleAsync(SocketInteraction interaction)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        if (player.Vueue.Count <= 1)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "There aren't any songs to shuffle in the queue!");
            return;
        }

        try
        {
            player.Vueue.Shuffle();
            await _embed.CreateSuccessfulResponse(interaction, "Shuffled the queue!");
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to shuffle a queue's songs in a player.", ex);
        }
    }

    internal async Task FetchLyricsAsync(SocketInteraction interaction)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        if (player.PlayerState is PlayerState.Stopped or PlayerState.None)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, $"A song must be playing or be paused in order to get its lyrics!");
            return;
        }

        string lyrics = string.Empty;

        for (int i = 0; i < 2; i++)
        {
            try
            {
                lyrics = await player.Track.FetchLyricsFromGeniusAsync();

                if (!string.IsNullOrWhiteSpace(lyrics))
                {
                    break;
                }
                else
                {
                    lyrics = await player.Track.FetchLyricsFromOvhAsync();
                }
            }
            catch (Exception ex)
            {
                await _embed.CreateUnsuccessfulResponse(interaction, "Failed to fetch lyrics!");
                _logger.LogError("Failed to fetch the lyrics of a song.", ex);

                return;
            }
        }

        if (string.IsNullOrWhiteSpace(lyrics))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, $"No lyrics were found for {player.Track.Title}");
            return;
        }

        try
        {
            StringBuilder lyricsBuilder = new();
            foreach (string line in lyrics.Split(Environment.NewLine))
            {
                lyricsBuilder.AppendLine(line);
            }

            await _embed.CreateSuccessfulResponse(interaction, $"```{lyricsBuilder}```");
        }
        catch (Exception ex)
        {
            await _embed.CreateSuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to embed a song's lyrics.", ex);
        }
    }

    internal async Task FetchArtworkAsync(SocketInteraction interaction)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        if (player.PlayerState is PlayerState.Stopped or PlayerState.None)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "A song must be playing or be paused in order to get its artwork!");
            return;
        }

        try
        {
            string artworkUrl = await player.Track.FetchArtworkAsync();

            if (string.IsNullOrWhiteSpace(artworkUrl))
            {
                await _embed.CreateUnsuccessfulResponse(interaction, $"Failed to find an artwork of {player.Track.Title}!");
                return;
            }

            await _embed.CreateSuccessfulResponse(interaction, imageUrl: artworkUrl);
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to get a song's artwork.", ex);
        }
    }

    internal async Task MixChannelsAsync(
        SocketInteraction interaction,
        double rightToRight,
        double leftToLeft,
        double rightToLeft,
        double leftToRight)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new ChannelMixFilter
            {
                RightToRight = rightToRight,
                LeftToLeft = leftToLeft,
                RightToLeft = rightToLeft,
                LeftToRight = leftToRight,
            });
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to apply a channel mixing filter in a player.", ex);
        }
    }

    internal async Task DistortAsync(
        SocketInteraction interaction,
        int offset,
        int tanOffset,
        int cosOffset,
        int scale,
        int tanScale,
        int cosScale)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new DistortionFilter
            {
                Offset = offset,
                TanOffset = tanOffset,
                CosOffset = cosOffset,
                Scale = scale,
                TanScale = tanScale,
                CosScale = cosScale,
            });
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to apply a distortion filter in a player.", ex);
        }
    }

    internal async Task KarokeAsync(
        SocketInteraction interaction,
        double filterWidth,
        double band,
        double monoLevel,
        double level)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new KarokeFilter
            {
                FilterWidth = filterWidth,
                FilterBand = band,
                MonoLevel = monoLevel,
                Level = level,
            });
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to apply a karoke filter in a player.", ex);
        }
    }

    internal async Task LowPassAsync(SocketInteraction interaction, double smoothing)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new LowPassFilter { Smoothing = smoothing });
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to apply a low-pass filter in a player.", ex);
        }
    }

    internal async Task PanAsync(SocketInteraction interaction, double hz)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new RotationFilter { Hertz = hz });
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to apply an audio-pan filter in a player.", ex);
        }
    }

    internal async Task TimescaleAsync(SocketInteraction interaction, double pitch, double rate, double speed)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new TimescaleFilter
            {
                Pitch = pitch,
                Rate = rate,
                Speed = speed,
            });
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to apply a timescale filter in a player.", ex);
        }
    }

    internal async Task OscillateVolumeAsync(SocketInteraction interaction, double depth, double frequency)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new TremoloFilter
            {
                Depth = depth,
                Frequency = frequency,
            });
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to apply a tremolo filter in a player.", ex);
        }
    }

    internal async Task OscillatePitchAsync(SocketInteraction interaction, double frequency, double depth)
    {
        if (interaction.User as IVoiceState is null)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer((interaction.User as SocketGuildUser).Guild, out LavaPlayer<LavaTrack> player))
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "I'm not in a voice channel!");
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new VibratoFilter
            {
                Frequency = frequency,
                Depth = depth,
            });
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to apply a vibrato filter in a player.", ex);
        }
    }

    private async Task OnTrackEndedAsync(SocketInteraction interaction, TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackEnded)
    {
        if (trackEnded.Reason is TrackEndReason.Stopped)
        {
            return;
        }

        if (!trackEnded.Player.Vueue.TryDequeue(out LavaTrack queuedSong))
        {
            await _embed.CreateSuccessfulResponse(interaction, "Queue finished!");
            return;
        }

        if (queuedSong is not LavaTrack track)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "Next item in queue is not a track!");
            return;
        }

        try
        {
            await trackEnded.Player.PlayAsync(track);
            await _embed.CreateSuccessfulResponse(interaction, $"Now playing: {track.Title}");
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, "An internal error has occured.");
            _logger.LogError("Failed to play the next queued song in a player.", ex);
        }
    }

    private async Task OnTrackStuckAsync(SocketInteraction interaction, TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackStuck)
    {
        trackStuck.Player.Vueue.Enqueue(trackStuck.Track);

        await _embed.CreateSuccessfulResponse(
            interaction,
            $"{trackStuck.Track.Title} has been re-added to the queue after getting stuck.",
            customColor: 0xFFEF00);
    }

    private async Task OnTrackExceptionAsync(SocketInteraction interaction, TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackException)
    {
        trackException.Player.Vueue.Enqueue(trackException.Track);

        await _embed.CreateSuccessfulResponse(
            interaction,
            $"{trackException.Track.Title} has been re-added to queue because of an internal issue.",
            customColor: 0xFFEF00);
    }
}
