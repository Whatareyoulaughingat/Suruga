using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Suruga.Extensions;
using Suruga.Handlers;

namespace Suruga.Services
{
    public class MusicService
    {
        private const int DefaultVolume = 50;
        private bool defaultVolumeChanged = false;

        public MusicService()
            => Queue = new LavalinkExtensions<LavalinkTrack>();

        private LavalinkExtensions<LavalinkTrack> Queue { get; set; }

        public async Task<DiscordMessage> PlayAsync(DiscordClient client, DiscordChannel channel, DiscordMember member, string url)
        {
            // Connecting to a voice channel.
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            LavalinkExtension lavalink = client.GetLavalink();
            LavalinkNodeConnection lavalinkNode = lavalink.ConnectedNodes.Values.First();
            await lavalinkNode.ConnectAsync(member.VoiceState.Channel);

            // Playing a URL in a voice channel.
            LavalinkGuildConnection connection = lavalinkNode.GetGuildConnection(member.VoiceState.Guild);
            LavalinkLoadResult loadResult = await lavalinkNode.Rest.GetTracksAsync(url, LavalinkSearchType.Youtube | LavalinkSearchType.SoundCloud);

            // Hook an event for playing tracks one after another (assuming there is a queue). This event was supposed to be put in HookEvents() (SurugaClient.cs file) alongside with some other events but it's not possible.
            connection.PlaybackFinished += async (connection, trackEndedArgs) => await TrackFinishedReason(trackEndedArgs, channel, member);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "Failed to search or load for the specifed track!");
            }

            LavalinkTrack lavalinkTrack = loadResult.Tracks.First();

            if (connection.CurrentState.CurrentTrack != null)
            {
                Queue.Enqueue(lavalinkTrack);
                return await EmbedHandler.CreateEmbed(channel, member, $"[{lavalinkTrack.Title}]({lavalinkTrack.Uri.AbsoluteUri}) has been added to the queue.");
            }

            if (defaultVolumeChanged != false)
            {
                await connection.SetVolumeAsync(DefaultVolume);
            }

            await connection.PlayAsync(lavalinkTrack);
            return await EmbedHandler.CreateEmbed(channel, member, $"Now Playing: [{connection.CurrentState.CurrentTrack.Title}]({connection.CurrentState.CurrentTrack.Uri.AbsoluteUri})");
        }

        public async Task<DiscordMessage> LeaveAsync(DiscordClient client, DiscordChannel channel, DiscordMember member)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            LavalinkExtension lavalink = client.GetLavalink();
            LavalinkNodeConnection lavalinkNode = lavalink.ConnectedNodes.Values.First();
            LavalinkGuildConnection connection = lavalinkNode.GetGuildConnection(member.VoiceState.Guild);

            string voiceChannel = connection.Channel.Name;

            await connection.DisconnectAsync();
            return await EmbedHandler.CreateEmbed(channel, member, $"Left **{voiceChannel}**");
        }

        public async Task StopAsync(DiscordClient client, DiscordChannel channel, DiscordMember member)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
                return;
            }

            LavalinkExtension lavalink = client.GetLavalink();
            LavalinkNodeConnection node = lavalink.ConnectedNodes.Values.First();
            LavalinkGuildConnection connection = node.GetGuildConnection(member.VoiceState.Guild);

            await connection.StopAsync();
        }

        public async Task<DiscordMessage> PauseAsync(DiscordClient client, DiscordChannel channel, DiscordMember member)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            LavalinkExtension lavalink = client.GetLavalink();
            LavalinkNodeConnection node = lavalink.ConnectedNodes.Values.First();
            LavalinkGuildConnection connection = node.GetGuildConnection(member.VoiceState.Guild);

            if (connection.CurrentState.CurrentTrack == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "There is nothing to pause.");
            }

            await connection.PauseAsync();
            return await EmbedHandler.CreateEmbed(channel, member, $"Paused [{connection.CurrentState.CurrentTrack.Title}]({connection.CurrentState.CurrentTrack.Uri.AbsoluteUri})");
        }

        public async Task<DiscordMessage> ResumeAsync(DiscordClient client, DiscordChannel channel, DiscordMember member)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            LavalinkExtension lavalink = client.GetLavalink();
            LavalinkNodeConnection node = lavalink.ConnectedNodes.Values.First();
            LavalinkGuildConnection connection = node.GetGuildConnection(member.VoiceState.Guild);

            if (connection.CurrentState.CurrentTrack == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "There is nothing to resume.");
            }

            await connection.ResumeAsync();
            return await EmbedHandler.CreateEmbed(channel, member, $"Resumed [{connection.CurrentState.CurrentTrack.Title}]({connection.CurrentState.CurrentTrack.Uri.AbsoluteUri})");
        }

        public async Task<DiscordMessage> UpdateVolumeAsync(DiscordClient client, DiscordChannel channel, DiscordMember member, int volume)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            if (volume > 100 || volume < 0)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "Volume must be between 0 to 100.");
            }

            LavalinkExtension lavalink = client.GetLavalink();
            LavalinkNodeConnection node = lavalink.ConnectedNodes.Values.First();
            LavalinkGuildConnection connection = node.GetGuildConnection(member.VoiceState.Guild);

            // If the input volume is not equal to the default volume, then the default volume has changed, otherwise it hasn't.
            if (volume != DefaultVolume)
            {
                defaultVolumeChanged = true;
            }
            else
            {
                defaultVolumeChanged = true;
            }

            await connection.SetVolumeAsync(volume);
            return await EmbedHandler.CreateEmbed(channel, member, $"Updated volume to {volume}");
        }

        public async Task<LavalinkTrack> SkipAsync(DiscordClient client, DiscordChannel channel, DiscordMember member, TimeSpan? delay = default)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
                return null;
            }

            LavalinkExtension lavalink = client.GetLavalink();
            LavalinkNodeConnection node = lavalink.ConnectedNodes.Values.First();
            LavalinkGuildConnection connection = node.GetGuildConnection(member.VoiceState.Guild);

            if (!Queue.Dequeue(out LavalinkTrack queueableLavalinkTrack))
            {
                await EmbedHandler.CreateErrorEmbed(channel, member, "There isn't any track to play in order to skip the current track.");
                return null;
            }

            if (queueableLavalinkTrack is not LavalinkTrack lavalinkTrack)
            {
                await EmbedHandler.CreateErrorEmbed(channel, member, $"Couldn't cast **{queueableLavalinkTrack.GetType()}**. Is the specific track valid?");
                return null;
            }

            await Task.Delay(delay ?? TimeSpan.Zero)
                .ContinueWith(_ => StopAsync(client, channel, member))
                .ContinueWith(_ => PlayAsync(client, channel, member, queueableLavalinkTrack.Uri.AbsoluteUri));

            return lavalinkTrack;
        }

        public async Task<DiscordMessage> ShuffleAsync(DiscordChannel channel, DiscordMember member)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            if (Queue == null | Queue.Count() <= 1)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "There isn't a queue to shuffle.");
            }

            Queue.Shuffle();
            return await EmbedHandler.CreateEmbed(channel, member, "Shuffled the playlist.");
        }

        public async Task<DiscordMessage> ListQueueAsync(DiscordClient client, DiscordChannel channel, DiscordMember member)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must first join a voice channel.");
            }

            LavalinkExtension lavalink = client.GetLavalink();
            LavalinkNodeConnection node = lavalink.ConnectedNodes.Values.First();
            LavalinkGuildConnection connection = node.GetGuildConnection(member.VoiceState.Guild);

            if (!Queue.Any() && connection.CurrentState.CurrentTrack != null)
            {
                return await EmbedHandler.CreateEmbed(channel, member, $"Currently Playing: [{connection.CurrentState.CurrentTrack.Title}]({connection.CurrentState.CurrentTrack.Uri.AbsoluteUri}).\nNothing else is queued.");
            }
            else
            {
                /*
                Now we know if we have something in the queue worth replying with, so we iterate through all the Tracks in the queue.
                Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.
                */

                int trackNum = 2;

                StringBuilder builderDescription = new();
                foreach (LavalinkTrack track in Queue)
                {
                    builderDescription.Append($"\n{trackNum}: **{track.Title}**.\n");
                    trackNum++;

                    await Task.CompletedTask;
                }

                return await EmbedHandler.CreateEmbed(channel, member, $"Currently playing: [{connection.CurrentState.CurrentTrack.Title}]({connection.CurrentState.CurrentTrack.Uri.AbsoluteUri}).\n{builderDescription}");
            }
        }

        public async Task<DiscordMessage> TrackFinishedReason(TrackFinishEventArgs trackEndReason, DiscordChannel channel, DiscordMember member)
        {
            if (!trackEndReason.Reason.MayStartNext())
            {
                return null;
            }

            if (!Queue.Dequeue(out LavalinkTrack queueableLavalinkTrack))
            {
                return null;
            }

            if (queueableLavalinkTrack is not LavalinkTrack lavalinkTrack)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "Next item in queue is not a track!");
            }

            await trackEndReason.Player.PlayAsync(lavalinkTrack);
            return await EmbedHandler.CreateEmbed(channel, member, $"Now Playing: [{lavalinkTrack.Title}]({lavalinkTrack.Uri.AbsoluteUri}).");
        }
    }
}