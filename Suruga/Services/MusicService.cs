using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Lavalink4NET.Lyrics;
using Suruga.Injectors;

namespace Suruga.Services;

public class MusicService
{
    public async Task<DiscordMessage> PlayAsync(DiscordClient client, DiscordChannel channel, DiscordMember member, IAudioService audioService, string url)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "Join a voice channel first.");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return await Embeds.CreateErrorEmbed(channel, member, "Specify a valid URL.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id)
            ?? await audioService.JoinAsync<QueuedLavalinkPlayer>(member.Guild.Id, member.VoiceState.Channel.Id, true);

        audioService.TrackEnd += async (sender, trackEndEventArgs) => await OnTrackEndAsync(trackEndEventArgs, channel, member).ConfigureAwait(false);
        audioService.TrackStuck += async (sender, trackStuckEventArgs) => await OnTrackStuckAsync(trackStuckEventArgs, channel, member).ConfigureAwait(false);

        TrackLoadResponsePayload trackLoadResponse = await audioService.LoadTracksAsync(url, SearchMode.YouTube);
        if (trackLoadResponse.LoadType == TrackLoadType.LoadFailed || trackLoadResponse.LoadType == TrackLoadType.NoMatches)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "Failed to load the specified track or there were no matches of it.");
        }

        LavalinkTrack track = trackLoadResponse.Tracks.First();

        if ((player.CurrentTrack != null && player.State == PlayerState.Playing) || player.State == PlayerState.Paused)
        {
            player.Queue.Add(track);
            return await Embeds.CreateEmbed(channel, member, $"{track.Title} has been added to the queue.");
        }

        client.Resumed += async (client, readyEventArgs) => await player.ResumeAsync();

        await player.PlayAsync(track);
        return await Embeds.CreateEmbed(channel, member, $"Now Playing [{track.Title}]({track.Source})");
    }

    public async Task<DiscordMessage> LeaveAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        await player.DisconnectAsync();
        return await Embeds.CreateEmbed(channel, member, $"Left **{member.VoiceState.Channel.Name}**");
    }

    public async Task<DiscordMessage> StopAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        if (player.State == PlayerState.NotPlaying)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "There isn't anything to stop.");
        }

        await player.StopAsync();
        return await Embeds.CreateEmbed(channel, member, "Stopped the playback.");
    }

    public async Task<DiscordMessage> PauseAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        if (player.CurrentTrack == null || player.State == PlayerState.Paused)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "There isn't anything to pause.");
        }

        await player.PauseAsync();
        return await Embeds.CreateEmbed(channel, member, $"Paused [{player.CurrentTrack.Title}]({player.CurrentTrack.Source})");
    }

    public async Task<DiscordMessage> ResumeAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        if (player.CurrentTrack == null || player.State == PlayerState.Playing)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "There is nothing to resume.");
        }

        await player.ResumeAsync();
        return await Embeds.CreateEmbed(channel, member, $"Resumed [{player.CurrentTrack.Title}]({player.CurrentTrack.Source})");
    }

    public async Task<DiscordMessage> UpdateVolumeAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService, float volume)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        if (volume > 10f || volume < 0f)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "The volume must be in a range between 0 and 10.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        await player.SetVolumeAsync(volume, force: true);
        return await Embeds.CreateEmbed(channel, member, $"Updated volume to {volume}");
    }

    public async Task<DiscordMessage> SkipAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService, int skipCount = 1)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            return null;
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        if (!player.Queue.TryDequeue(out LavalinkTrack queueableLavalinkTrack))
        {
            return await Embeds.CreateErrorEmbed(channel, member, "There isn't any track in the queue in order to skip the current one.");
        }

        if (queueableLavalinkTrack is null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, $"The specific track is invalid.");
        }

        if (skipCount > player.Queue.Count)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "The number of tracks to skip is bigger than the actual queue.");
        }

        await player.SkipAsync(skipCount);
        await player.PlayAsync(queueableLavalinkTrack);
        return await Embeds.CreateEmbed(channel, member, "Skipped the track.");
    }

    public async Task<DiscordMessage> ShuffleAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        if (player.Queue.IsEmpty || player.Queue.Count <= 1)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "The queue isn't big enough to shuffle. Have at least 2 tracks in the queue.");
        }

        player.Queue.Shuffle();
        return await Embeds.CreateEmbed(channel, member, "Shuffled the queue.");
    }

    public async Task<DiscordMessage> ListQueueAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        if (!player.Queue.Any() && player.CurrentTrack != null)
        {
            return await Embeds.CreateEmbed(channel, member, $"Currently Playing: [{player.CurrentTrack.Title}]({player.CurrentTrack.Source}).\nNothing else is queued.");
        }
        else
        {
            /*
            Now we know if we have something in the queue worth replying with, so we iterate through all the Tracks in the queue.
            Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
            This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song).
            */

            int trackNum = 2;

            StringBuilder builderDescription = new();
            foreach (LavalinkTrack track in player.Queue)
            {
                builderDescription.Append($"\n{trackNum}: **{track.Title}**.\n");
                trackNum++;

                await Task.CompletedTask.ConfigureAwait(false);
            }

            return await Embeds.CreateEmbed(channel, member, $"Currently playing: [{player.CurrentTrack.Title}]({player.CurrentTrack.Source}).\n{builderDescription}");
        }
    }

    public async Task<DiscordMessage> ReplayAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);
        await player.ReplayAsync();

        return await Embeds.CreateEmbed(channel, member, $"Replaying: [{player.CurrentTrack.Title}]({player.CurrentTrack.Source}).");
    }

    public async Task<DiscordMessage> LoopAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }
        
        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        // Think of the loop command as a toggle rather than having 2 seperate commands for looping and unlooping.
        if (player.IsLooping)
        {
            player.IsLooping = false;
            return await Embeds.CreateEmbed(channel, member, "Started looping.");
        }
        else
        {
            player.IsLooping = true;
            return await Embeds.CreateEmbed(channel, member, "Started unlooping.");
        }
    }

    public async Task<DiscordMessage> NowPlayingAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        return await Embeds.CreateEmbed(channel, member, $"Now Playing: [{player.CurrentTrack.Title}]");
    }

    public async Task<DiscordMessage> ClearQueueAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        player.Queue.Clear();
        return await Embeds.CreateEmbed(channel, member, "Sucessfully cleared the queue.");
    }

    public async Task<DiscordMessage> PostLyricsAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
    {
        if (member.VoiceState == null || member.VoiceState.Channel == null)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
        }

        QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

        using LyricsService lyricsService = new(new LyricsOptions());
        string lyrics = await lyricsService.RequestLyricsAsync(player.CurrentTrack.Author, player.CurrentTrack.Title);

        if (string.IsNullOrWhiteSpace(lyrics))
        {
            return await Embeds.CreateErrorEmbed(channel, member, "Could not find any lyrics for the specified track.");
        }

        return await Embeds.CreateEmbed(channel, member, lyrics);
    }

    public async Task<DiscordMessage> OnTrackEndAsync(TrackEndEventArgs trackEndArgs, DiscordChannel channel, DiscordMember member)
    {
        if (!trackEndArgs.MayStartNext)
        {
            return null;
        }

        if (!(trackEndArgs.Player as QueuedLavalinkPlayer).Queue.TryDequeue(out LavalinkTrack queuedLavalinkTrack))
        {
            return null;
        }

        if (queuedLavalinkTrack is not LavalinkTrack lavalinkTrack)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "Next item in queue is not a track.");
        }

        await trackEndArgs.Player.PlayAsync(lavalinkTrack);
        return await Embeds.CreateEmbed(channel, member, $"Now Playing: [{lavalinkTrack.Title}]({lavalinkTrack.Source}).");
    }

    public async Task<DiscordMessage> OnTrackStuckAsync(TrackStuckEventArgs truckStuckArgs, DiscordChannel channel, DiscordMember member)
    {
        await truckStuckArgs.Player.StopAsync();

        if (!(truckStuckArgs.Player as QueuedLavalinkPlayer).Queue.TryDequeue(out LavalinkTrack queuedLavalinkTrack))
        {
            return null;
        }

        if (queuedLavalinkTrack is not LavalinkTrack lavalinkTrack)
        {
            return await Embeds.CreateErrorEmbed(channel, member, "Next item in queue is not a track.");
        }

        await truckStuckArgs.Player.PlayAsync(lavalinkTrack);
        return await Embeds.CreateEmbed(channel, member, "Playing the next track in the queue because the current one was stuck.");
    }
}
