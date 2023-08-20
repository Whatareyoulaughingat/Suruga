using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Suruga.Helpers;
using Suruga.Logging;
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
using EmbedType = Suruga.Helpers.EmbedType;

namespace Suruga.Services;

public sealed class MusicService
{
    private ConcurrentBag<IDisposable> _disposableEvents;

    private readonly IConfiguration _configuration;

    private readonly LavaNode _lavaNode;

    private readonly ProgramLogger _programLogger;

    public MusicService(IConfiguration configuration, LavaNode lavaNode, ProgramLogger programLogger)
    {
        _configuration = configuration;
        _lavaNode = lavaNode;
        _programLogger = programLogger;
    }

    internal async Task LeaveAsync(IGuildUser user, ISocketMessageChannel channel)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Left {player.VoiceChannel.Name}");

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
            _programLogger.LogError("Failed to dispose a player and its events.", ex);
        }
    }

    internal async Task PlayAsync(IGuildUser user, ISocketMessageChannel channel, string query, IVoiceChannel voiceChannel = null)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "Provide a URL");
            return;
        }

        if (!Uri.IsWellFormedUriString(query, UriKind.Absolute))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "The URL your provided is not valid.");
            return;
        }

        if (!_lavaNode.HasPlayer(user.Guild))
        {
            IDisposable trackEndEvent = Observable.FromEventPattern<TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack>>(
                handler => _lavaNode.OnTrackEnd += (args) => OnTrackEndedAsync(user, channel, args),
                handler => _lavaNode.OnTrackEnd -= (args) => OnTrackEndedAsync(user, channel, args))
            .Subscribe();

            IDisposable trackStuckEvent = Observable.FromEventPattern<TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack>>(
                handler => _lavaNode.OnTrackStuck += (args) => OnTrackStuckAsync(user, channel, args),
                handler => _lavaNode.OnTrackStuck -= (args) => OnTrackStuckAsync(user, channel, args))
            .Subscribe();

            IDisposable trackExceptionEvent = Observable.FromEventPattern<TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack>>(
                handler => _lavaNode.OnTrackException += (args) => OnTrackExceptionAsync(user, channel, args),
                handler => _lavaNode.OnTrackException -= (args) => OnTrackExceptionAsync(user, channel, args))
            .Subscribe();

            _disposableEvents = new() { trackEndEvent, trackStuckEvent, trackExceptionEvent };
        }

        try
        {
            if (voiceChannel is null)
            {
                _ = !bool.TryParse(_configuration["DeafenOnVoiceChannel"], out bool shouldDeafen)
                    ? await user.VoiceChannel.ConnectAsync(selfDeaf: true)
                    : await user.VoiceChannel.ConnectAsync(shouldDeafen);

                await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Joined {user.VoiceChannel.Name}");
            }
            else
            {
                _ = !bool.TryParse(_configuration["DeafenOnVoiceChannel"], out bool shouldDeafen)
                    ? await voiceChannel.ConnectAsync(selfDeaf: true)
                    : await voiceChannel.ConnectAsync(shouldDeafen);

                await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Joined {voiceChannel.Name}");
            }
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, $"An internal error has occured.");
            _programLogger.LogError("A player has failed to join a voice channel.", ex);

            return;
        }

        // todo: check if a player is created when calling discord.net's connectasync.
        // otherwise use lavanodes joinasync to join a voice channel and getting the player.
        _lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player);

        SearchResponse response = await _lavaNode.SearchAsync(SearchType.YouTube | SearchType.SoundCloud | SearchType.Direct, query);

        if (response.Status is SearchStatus.LoadFailed)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "Failed to load songs!");
            return;
        }

        if (response.Status is SearchStatus.NoMatches)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, $"No matches were found for {query}");
            return;
        }

        try
        {
            if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
            {
                player.Vueue.Enqueue(response.Tracks);
                await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Enqueued {response.Tracks.Count} songs");
            }
            else
            {
                LavaTrack track = response.Tracks.First();

                await player.PlayAsync(track);
                await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Now Playing [{track.Title}]({track.Url})");
            }
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, $"An internal error has occured.");
            _programLogger.LogError("Failed to play a song, a playlist or a queued song", ex);
        }
    }

    internal async Task StopAsync(IGuildUser user, ISocketMessageChannel channel)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        if (player.PlayerState is PlayerState.Stopped or PlayerState.None)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "A song must be playing or be paused in order to stop!");
            return;
        }

        try
        {
            await player.StopAsync();
            await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Stopped Playing {player.Track.Title}");
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured");
            _programLogger.LogError("Failed to stop a player.", ex);
        }
    }

    internal async Task SkipAsync(IGuildUser user, ISocketMessageChannel channel)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        if (player.PlayerState is PlayerState.Stopped or PlayerState.None)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "A song must be playing or be paused in order to skip!");
            return;
        }

        if (player.Vueue.Count <= 1)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "There isn't a queue in order to skip!");
            return;
        }

        try
        {
            (LavaTrack Skipped, _) = await player.SkipAsync();
            await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Skipped {Skipped.Title}");
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to skip a song.", ex);
        }
    }

    internal async Task PauseAsync(IGuildUser user, ISocketMessageChannel channel)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        if (player.PlayerState is PlayerState.Paused or PlayerState.Stopped or PlayerState.None)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "A song must be playing in order to pause!");
            return;
        }

        try
        {
            await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Paused {player.Track.Title}");
            await player.PauseAsync();
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to pause a song.", ex);
        }
    }

    internal async Task ResumeAsync(IGuildUser user, ISocketMessageChannel channel)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        if (player.PlayerState is PlayerState.Playing or PlayerState.Stopped or PlayerState.None)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "A song must be paused in order to resume!");
            return;
        }

        try
        {
            await player.ResumeAsync();
            await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Resumed {player.Track.Title}");
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to resume a song.", ex);
        }
    }

    internal async Task SetVolumeAsync(IGuildUser user, ISocketMessageChannel channel, int volume)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        if (volume > 200 || volume < 0)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "The range of the volume must be between 0 and 200.");
            return;
        }

        try
        {
            await player.SetVolumeAsync(volume);
            await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Increased the volume to {volume}%");
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to set the volume of a player.", ex);
        }
    }

    internal async Task ShuffleAsync(IGuildUser user, ISocketMessageChannel channel)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        if (player.Vueue.Count <= 1)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "There aren't any songs to shuffle in the queue!");
            return;
        }

        try
        {
            player.Vueue.Shuffle();
            await EmbedHelper.Create(user, channel, EmbedType.Successful, "Shuffled the queue!");
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to shuffle a queue's songs in a player.", ex);
        }
    }

    internal async Task FetchLyricsAsync(IGuildUser user, ISocketMessageChannel channel)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        if (player.PlayerState is PlayerState.Stopped or PlayerState.None)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, $"A song must be playing or be paused in order to get its lyrics!");
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
                await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "Failed to fetch lyrics!");
                _programLogger.LogError("Failed to fetch the lyrics of a song.", ex);

                return;
            }
        }

        if (string.IsNullOrWhiteSpace(lyrics))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, $"No lyrics were found for {player.Track.Title}");
            return;
        }

        try
        {
            StringBuilder lyricsBuilder = new();
            foreach (string line in lyrics.Split(Environment.NewLine))
            {
                lyricsBuilder.AppendLine(line);
            }

            await EmbedHelper.Create(user, channel, EmbedType.Successful, $"```{lyricsBuilder}```");
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to embed a song's lyrics.", ex);
        }
    }

    internal async Task FetchArtworkAsync(IGuildUser user, ISocketMessageChannel channel)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        if (player.PlayerState is PlayerState.Stopped or PlayerState.None)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "A song must be playing or be paused in order to get its artwork!");
            return;
        }

        try
        {
            string artworkUrl = await player.Track.FetchArtworkAsync();

            if (string.IsNullOrWhiteSpace(artworkUrl))
            {
                await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, $"Failed to find an artwork of {player.Track.Title}!");
                return;
            }

            await EmbedHelper.Create(user, channel, EmbedType.Successful, imageUrl: artworkUrl);
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to get a song's artwork.", ex);
        }
    }

    internal async Task MixChannelsAsync(IGuildUser user, ISocketMessageChannel channel, double rightToRight, double leftToLeft, double rightToLeft, double leftToRight)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
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
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to apply a channel mixing filter in a player.", ex);
        }
    }

    internal async Task DistortAsync(IGuildUser user, ISocketMessageChannel channel, int offset, int tanOffset, int cosOffset, int scale, int tanScale, int cosScale)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
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
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to apply a distortion filter in a player.", ex);
        }
    }

    internal async Task KarokeAsync(IGuildUser user, ISocketMessageChannel channel, double filterWidth, double band, double monoLevel, double level)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
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
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to apply a karoke filter in a player.", ex);
        }
    }

    internal async Task LowPassAsync(IGuildUser user, ISocketMessageChannel channel, double smoothing)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new LowPassFilter { Smoothing = smoothing });
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to apply a low-pass filter in a player.", ex);
        }
    }

    internal async Task PanAsync(IGuildUser user, ISocketMessageChannel channel, double hz)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
            return;
        }

        try
        {
            await player.ApplyFilterAsync(new RotationFilter { Hertz = hz });
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to apply an audio-pan filter in a player.", ex);
        }
    }

    internal async Task TimescaleAsync(IGuildUser user, ISocketMessageChannel channel, double pitch, double rate, double speed)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
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
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to apply a timescale filter in a player.", ex);
        }
    }

    internal async Task OscillateVolumeAsync(IGuildUser user, ISocketMessageChannel channel, double depth, double frequency)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
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
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to apply a tremolo filter in a player.", ex);
        }
    }

    internal async Task OscillatePitchAsync(IGuildUser user, ISocketMessageChannel channel, double frequency, double depth)
    {
        if (user?.VoiceChannel is null)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "You must join a voice channel first!");
            return;
        }

        if (!_lavaNode.TryGetPlayer(user.Guild, out LavaPlayer<LavaTrack> player))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "I'm not in a voice channel!");
            return;
        }

        if (player is null)
        {
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
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to apply a vibrato filter in a player.", ex);
        }
    }

    private async Task OnTrackEndedAsync(IGuildUser user, ISocketMessageChannel channel, TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackEnded)
    {
        if (trackEnded.Reason is TrackEndReason.Stopped)
        {
            return;
        }

        if (!trackEnded.Player.Vueue.TryDequeue(out LavaTrack queuedSong))
        {
            await EmbedHelper.Create(user, channel, EmbedType.Successful, "Queue finished!");
            return;
        }

        if (queuedSong is not LavaTrack track)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "Next item in queue is not a track!");
            return;
        }

        try
        {
            await trackEnded.Player.PlayAsync(track);
            await EmbedHelper.Create(user, channel, EmbedType.Successful, $"Now playing: {track.Title}");
        }
        catch (Exception ex)
        {
            await EmbedHelper.Create(user, channel, EmbedType.Unsuccessful, "An internal error has occured.");
            _programLogger.LogError("Failed to play the next queued song in a player.", ex);
        }
    }

    private async Task OnTrackStuckAsync(IGuildUser user, ISocketMessageChannel channel, TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackStuck)
    {
        trackStuck.Player.Vueue.Enqueue(trackStuck.Track);

        await EmbedHelper.Create(
            user,
            channel,
            EmbedType.Successful,
            $"{trackStuck.Track.Title} has been re-added to the queue after getting stuck",
            overridableColor: 0xFFEF00);
    }

    private async Task OnTrackExceptionAsync(IGuildUser user, ISocketMessageChannel channel, TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackException)
    {
        trackException.Player.Vueue.Enqueue(trackException.Track);

        await EmbedHelper.Create(
            user,
            channel,
            EmbedType.Successful,
            $"{trackException.Track.Title} has been re-added to queue because of an internal issue.",
            overridableColor: 0xFFEF00);
    }
}
