using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Suruga.Handlers;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace Suruga.Services
{
    public class MusicService
    {
        private readonly LavaNode lavaNode;

        public MusicService(LavaNode lavanode)
            => lavaNode = lavanode;

        public async Task<Embed> JoinAsync(SocketGuild guild, IVoiceState voiceState, SocketTextChannel textChannel, SocketUserMessage message)
        {
            if (lavaNode.HasPlayer(guild))
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateError("I'm already connected to a voice channel!");
                }
            }

            if (voiceState.VoiceChannel is null)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateError("You must be connected to a voice channel!");
                }
            }

            try
            {
                await lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);

                using (message.Channel.EnterTypingState())
                {
                    return await EmbedHandler.CreateMusic($"Joined {voiceState.VoiceChannel.Name}.");
                }
            }
            catch (Exception ax)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateException(ax);
                }
            }
        }

        public async Task<Embed> PlayAsync(SocketGuildUser user, SocketGuild guild, string query, SocketUserMessage message)
        {
            if (user.VoiceChannel == null)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateError("You Must First Join a Voice Channel.");
                }
            }

            if (!lavaNode.HasPlayer(guild))
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateError("I'm not connected to a voice channel.");
                }
            }

            if (query is null)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateError("Please specify a URL");
                }
            }

            try
            {
                var player = lavaNode.GetPlayer(guild);

                LavaTrack track;
                var search = await lavaNode.SearchAsync(query);

                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError($"I wasn't able to find anything for {query}.");
                    }
                }

                track = search.Tracks.FirstOrDefault();

                if (search.LoadStatus == LoadStatus.LoadFailed)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError($"An error has occured with the player. Here's the error details:\n{search.Exception.Message}");
                    }
                }

                if ((player.Track != null && player.PlayerState == PlayerState.Playing) || player.PlayerState == PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);

                    await LogService.LogInformationAsync("Music", $"{track.Title} has been added to the music queue");

                    using (message.Channel.EnterTypingState())
                    {
                        return await EmbedHandler.CreateMusic($"{track.Title} has been added to queue");
                    }
                }

                await player.PlayAsync(track);

                await LogService.LogInformationAsync("Music", $"Bot Now Playing: {track.Title}\nUrl: {track.Url}");

                using (message.Channel.EnterTypingState())
                {
                    return await EmbedHandler.CreateMusic($"🎵  Now Playing: {track.Title}\nUrl: {track.Url}");
                }
            }
            catch (Exception bx)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateException(bx);
                }
            }
        }

        public async Task<Embed> LeaveAsync(SocketGuild guild, SocketUserMessage message)
        {
            try
            {
                var player = lavaNode.GetPlayer(guild);

                if (player.PlayerState == PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                await lavaNode.LeaveAsync(player.VoiceChannel);
                await lavaNode.DisposeAsync();

                await LogService.LogInformationAsync("Music", $"Bot has left.");

                using (message.Channel.EnterTypingState())
                {
                    return await EmbedHandler.CreateMusic("I've left. Thank you for playing music");
                }
            }
            catch (KeyNotFoundException)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateError("I'm not connected to a voice channel");
                }
            }
        }

        public async Task<Embed> ListAsync(SocketGuild guild, SocketUserMessage message)
        {
            try
            {
                var builderDescription = new StringBuilder();

                var player = lavaNode.GetPlayer(guild);

                if (player == null)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError($"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}help for info on how to use the bot.");
                    }
                }

                if (player.PlayerState is PlayerState.Playing)
                {
                    if (player.Queue.Count < 1 && player.Track != null)
                    {
                        using (message.Channel.EnterTypingState())
                        {
                            return await EmbedHandler.CreateMusic($"Now Playing: {player.Track.Title}, Nothing Else Is Queued.");
                        }
                    }
                    else
                    {
                        /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
                         *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                            This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
                        var trackNum = 2;

                        foreach (LavaTrack track in player.Queue)
                        {
                            builderDescription.Append($"{trackNum}: [{track.Title}]({track.Url}) - {track.Id}\n");
                            trackNum++;

                            await Task.CompletedTask;
                        }

                        using (message.Channel.EnterTypingState())
                        {
                            return await EmbedHandler.CreateMusic($"Now Playing: [{player.Track.Title}]({player.Track.Url}) \n{builderDescription}");
                        }
                    }
                }
                else
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError($"The player doesn't seem to be playing anything right now. If this is an error, please contact Waylaa.");
                    }
                }
            }
            catch (Exception dx)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateException(dx);
                }
            }
        }

        /*This is ran when a user uses the command Skip
          Task Returns an Embed which is used in the command call. */
        public async Task<Embed> SkipTrackAsync(SocketGuild guild, SocketUserMessage message)
        {
            try
            {
                var player = lavaNode.GetPlayer(guild);

                if (player == null)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError($"I couldn't aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}help for info on how to use the bot.");
                    }
                }

                /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip.
                     User is expected to use the Stop command if they're only wanting to skip the current song. */
                if (player.Queue.Count < 1)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError($"Unable To skip a track as there is only One or No songs currently playing." + $"\n\nDid you mean {GlobalData.Config.DefaultPrefix}stop?");
                    }
                }
                else
                {
                    /* Save the current song for use after we skip it. */
                    var currentTrack = player.Track;
                    /* Skip the current song. */
                    await player.SkipAsync();
                    await LogService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");

                    using (message.Channel.EnterTypingState())
                    {
                        return await EmbedHandler.CreateMusic($"I have successfully skiped {currentTrack.Title}");
                    }
                }
            }
            catch (Exception ex)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateException(ex);
                }
            }
        }

        /*This is ran when a user uses the command Stop
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> StopAsync(SocketGuild guild, SocketUserMessage message)
        {
            try
            {
                var player = lavaNode.GetPlayer(guild);

                if (player == null)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError($"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}help for info on how to use the bot.");
                    }
                }

                /* Check if the player exists, if it does, check if it is playing.
                     If it is playing, we can stop.*/
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                await LogService.LogInformationAsync("Music", $"Bot has stopped playback.");

                using (message.Channel.EnterTypingState())
                {
                    return await EmbedHandler.CreateMusic("I Have stopped playback & the queue has been cleared.");
                }
            }
            catch (Exception fx)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateException(fx);
                }
            }
        }

        public async Task<Embed> SetVolumeAsync(SocketGuild guild, ushort volume, SocketUserMessage message)
        {
            if (volume > 1000 || volume < 0)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateError("Volume must be between 0 and 1000.");
                }
            }

            try
            {
                var player = lavaNode.GetPlayer(guild);

                await player.UpdateVolumeAsync(volume);

                await LogService.LogInformationAsync("Music", $"Bot Volume set to: {volume}");

                using (message.Channel.EnterTypingState())
                {
                    return await EmbedHandler.CreateMusic($"Volume has been set to {volume}.");
                }
            }
            catch (Exception gx)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateException(gx);
                }
            }
        }

        public async Task<Embed> PauseAsync(SocketGuild guild, SocketUserMessage message)
        {
            try
            {
                var player = lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Stopped)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError("There is nothting to pause");
                    }
                }

                if (player.PlayerState == PlayerState.Paused)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError("I'm already paused!");
                    }
                }

                await player.PauseAsync();

                using (message.Channel.EnterTypingState())
                {
                    return await EmbedHandler.CreateMusic($"Paused **{player.Track.Title}");
                }
            }
            catch (Exception hx)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateException(hx);
                }
            }
        }

        public async Task<Embed> ResumeAsync(SocketGuild guild, SocketUserMessage message)
        {
            try
            {
                var player = lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Playing)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError("There's no need to resume. I'm not paused!");
                    }
                }

                if (player.PlayerState is PlayerState.Paused)
                {
                    await player.ResumeAsync();
                }

                using (message.Channel.EnterTypingState())
                {
                    return await EmbedHandler.CreateMusic($"Resumed: {player.Track.Title}");
                }
            }
            catch (Exception ix)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateException(ix);
                }
            }
        }

        public async Task<Embed> TrackEnded(TrackEndedEventArgs trackEndedArgs)
        {
            IUserMessage message = null;

            if (!trackEndedArgs.Reason.ShouldPlayNext())
            {
                return null;
            }

            if (!trackEndedArgs.Player.Queue.TryDequeue(out var queueable))
            {
                /*
                using (message.Channel.EnterTypingState())
                {
                    return await EmbedHandler.CreateMusicEmbed("Playback finished!");
                }
                */
                return null;
            }

            if (!(queueable is LavaTrack track))
            {
                using (message.Channel.EnterTypingState()) // error caused here
                {
                    return EmbedHandler.CreateError("Next item in queue is not a track.");
                }
            }

            await trackEndedArgs.Player.PlayAsync(track);

            using (message.Channel.EnterTypingState())
            {
                return await EmbedHandler.CreateMusic($"Now Playing, [{track.Title}]({track.Url})");
            }
        }

        public async Task<Embed> ShuffleAsync(SocketGuild guild, SocketUserMessage message)
        {
            try
            {
                var player = lavaNode.GetPlayer(guild);

                if (player?.Queue is null || player.Queue.Count <= 1)
                {
                    using (message.Channel.EnterTypingState())
                    {
                        return EmbedHandler.CreateError("There is nothing to shuffle!");
                    }
                }

                await Task.Run(() => player.Queue.Shuffle());

                using (message.Channel.EnterTypingState())
                {
                    return await EmbedHandler.CreateMusic("Playlist shuffled!");
                }
            }
            catch (Exception jx)
            {
                using (message.Channel.EnterTypingState())
                {
                    return EmbedHandler.CreateException(jx);
                }
            }
        }
    }
}
