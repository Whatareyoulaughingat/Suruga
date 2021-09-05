using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Suruga.Handlers.Discord;

namespace Suruga.Services
{
    public class MusicService
    {
        public async Task<DiscordMessage> PlayAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService, string url)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "Join a voice channel first.");
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "Specify a valid URL.");
            }

            QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id)
                ?? await audioService.JoinAsync<QueuedLavalinkPlayer>(member.Guild.Id, member.VoiceState.Channel.Id, true);

            audioService.TrackEnd += async (sender, trackEndEventArgs) => await OnTrackEndAsync(trackEndEventArgs, channel, member).ConfigureAwait(false);
            audioService.TrackStuck += async (sender, trackStuckEventArgs) => await OnTrackStuckAsync(trackStuckEventArgs, channel, member).ConfigureAwait(false);

            TrackLoadResponsePayload trackLoadResponse = await audioService.LoadTracksAsync(url, SearchMode.YouTube);
            if (trackLoadResponse.LoadType == TrackLoadType.LoadFailed || trackLoadResponse.LoadType == TrackLoadType.NoMatches)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "Failed to load the specified track or there were no matches of it.");
            }

            LavalinkTrack track = trackLoadResponse.Tracks.First();

            if ((player.CurrentTrack != null && player.State == PlayerState.Playing) || player.State == PlayerState.Paused)
            {
                player.Queue.Add(track);
                return await EmbedHandler.CreateEmbed(channel, member, $"{track.Title} has been added to the queue.", null);
            }

            await player.PlayAsync(track);
            return await EmbedHandler.CreateEmbed(channel, member, $"Now Playing [{track.Title}]({track.Source})", null);
        }

        public async Task<DiscordMessage> LeaveAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

            await player.DisconnectAsync();
            return await EmbedHandler.CreateEmbed(channel, member, $"Left **{member.VoiceState.Channel.Name}**", null);
        }

        public async Task<DiscordMessage> StopAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

            if (player.State == PlayerState.NotPlaying)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "There isn't anything to stop.");
            }

            await player.StopAsync();
            return await EmbedHandler.CreateEmbed(channel, member, "Stopped the playback.", null);
        }

        public async Task<DiscordMessage> PauseAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
        {
            if (member.VoiceState == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

            if (player.CurrentTrack == null || player.State == PlayerState.Paused)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "There isn't anything to pause.");
            }

            await player.PauseAsync();
            return await EmbedHandler.CreateEmbed(channel, member, $"Paused [{player.CurrentTrack.Title}]({player.CurrentTrack.Source})", null);
        }

        public async Task<DiscordMessage> ResumeAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

            if (player.CurrentTrack == null || player.State == PlayerState.Playing)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "There is nothing to resume.");
            }

            await player.ResumeAsync();
            return await EmbedHandler.CreateEmbed(channel, member, $"Resumed [{player.CurrentTrack.Title}]({player.CurrentTrack.Source})", null);
        }

        public async Task<DiscordMessage> UpdateVolumeAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService, float volume)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            if (volume > 10f || volume < 0f)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The volume must be in a range between 0 and 10.");
            }

            QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

            await player.SetVolumeAsync(volume, force: true);
            return await EmbedHandler.CreateEmbed(channel, member, $"Updated volume to {volume}", null);
        }

        public async Task<DiscordMessage> SkipAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService, int skipCount = 1)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
                return null;
            }

            QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

            if (!player.Queue.TryDequeue(out LavalinkTrack queueableLavalinkTrack))
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "There isn't any track in the queue in order to skip the current one.");
            }

            if (queueableLavalinkTrack is null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, $"The specific track is invalid.");
            }

            if (skipCount > player.Queue.Count)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The number of tracks to skip is bigger than the actual queue.");
            }

            await player.SkipAsync(skipCount);
            await player.PlayAsync(queueableLavalinkTrack);
            return await EmbedHandler.CreateEmbed(channel, member, "Skipped the track.", null);
        }

        public async Task<DiscordMessage> ShuffleAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

            if (player.Queue.IsEmpty || player.Queue.Count <= 1)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The queue isn't big enough to shuffle. Have at least 2 tracks in the queue.");
            }

            player.Queue.Shuffle();
            return await EmbedHandler.CreateEmbed(channel, member, "Shuffled the queue.", null);
        }

        public async Task<DiscordMessage> ListQueueAsync(DiscordChannel channel, DiscordMember member, IAudioService audioService)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            QueuedLavalinkPlayer player = audioService.GetPlayer<QueuedLavalinkPlayer>(member.Guild.Id);

            if (!player.Queue.Any() && player.CurrentTrack != null)
            {
                return await EmbedHandler.CreateEmbed(channel, member, $"Currently Playing: [{player.CurrentTrack.Title}]({player.CurrentTrack.Source}).\nNothing else is queued.", null);
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

                return await EmbedHandler.CreateEmbed(channel, member, $"Currently playing: [{player.CurrentTrack.Title}]({player.CurrentTrack.Source}).\n{builderDescription}", null);
            }
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
                return await EmbedHandler.CreateErrorEmbed(channel, member, "Next item in queue is not a track.");
            }

            await trackEndArgs.Player.PlayAsync(lavalinkTrack);
            return await EmbedHandler.CreateEmbed(channel, member, $"Now Playing: [{lavalinkTrack.Title}]({lavalinkTrack.Source}).", null);
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
                return await EmbedHandler.CreateErrorEmbed(channel, member, "Next item in queue is not a track.");
            }

            await truckStuckArgs.Player.PlayAsync(lavalinkTrack);
            return await EmbedHandler.CreateEmbed(channel, member, "Playing the next track in the queue because the current one was stuck.", null);
        }
    }
}
