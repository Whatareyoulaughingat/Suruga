using System.Text;
using Discord;
using Discord.Interactions;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Players;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Tracks;
using Suruga.Commands.Autocomplete;
using Suruga.Contexts;
using Suruga.Contexts.Entities;

#pragma warning disable CS1591
namespace Suruga.Commands;

[Group("music", "Music related commands.")]
public sealed class MusicCommands(IAudioService audio, DatabaseContext database) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly StringBuilder TrackListBuilder = new();
    
    [SlashCommand("play", "Plays a track from the given query.")]
    public async Task PlayAsync
    (
        [Summary(description: "A URL or a name of a song, artist, or album.")]
        [Autocomplete(typeof(TrackResultsAutocompleteHandler))]
        string query
    )
    {
        await DeferAsync();

        QueuedLavalinkPlayer? player = await GetPlayerAsync();

        if (player is null)
        {
            return;
        }

        IReadOnlyList<TrackQueueItem>? queue = await database.RetrieveTrackQueueAsync(Context.Guild.Id);

        if (queue is not null)
        {
            await player.Queue.AddRangeAsync(queue);
        }

        await player.PlayAsync(query);
        await ModifyOriginalResponseAsync(x => x.Content = $"Now playing: [{player.CurrentTrack?.Title}]({player.CurrentTrack?.Uri})");
    }

    [SlashCommand("leave", "Stops the player and leaves from the voice channel.")]
    public async Task LeaveAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        await player.DisconnectAsync();
    }

    [SlashCommand("pause", "Pauses the player.")]
    public async Task PauseAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.State is PlayerState.Paused)
        {
            await RespondAsync("Player is already paused.");
            return;
        }

        await player.PauseAsync();
        await RespondAsync($"Paused [{player.CurrentTrack?.Title}]({player.CurrentTrack?.Uri})");
    }

    [SlashCommand("resume", "Resumes the player.")]
    public async Task ResumeAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.CurrentTrack is null)
        {
            await RespondAsync("There is nothing playing.");
            return;
        }

        await player.ResumeAsync();
        await RespondAsync($"Resumed: [{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})");
    }

    [SlashCommand("stop", "Stops the player.")]
    public async Task StopAsync()
    {
        await DeferAsync();

        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.CurrentTrack is null)
        {
            await RespondAsync("There is nothing playing.");
            return;
        }

        await database.SaveTrackQueueAsync(player.Queue, Context.Guild.Id);

        await player.StopAsync();
        await ModifyOriginalResponseAsync(x => x.Content = "Stopped playing.");
    }

    [SlashCommand("skip", "Skips the current track.")]
    public async Task SkipAsync([Summary(description: "The number of tracks to skip")] int count = 1)
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.CurrentTrack is null)
        {
            await RespondAsync("Nothing playing!");
            return;
        }

        if (player.Queue.IsEmpty)
        {
            await RespondAsync("There is no track to skip.");
            return;
        }

        LavalinkTrack track = player.CurrentTrack!;

        await player.SkipAsync(count);
        await RespondAsync($"Skipped [{track.Title}]({track.Uri})");
    }

    [SlashCommand("list", "Lists every queued track.")]
    public async Task ListAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.Queue.Count == 1)
        {
            await RespondAsync($"Now Playing: [{player.CurrentTrack?.Title}]({player.CurrentTrack?.Uri}).");
        }
        else
        {
            for (int i = 2; i < player.Queue.Count; i++)
            {
                LavalinkTrack track = player.Queue[i].Track!;
                TrackListBuilder.AppendLine($"{i}: [{track.Title}]({track.Uri})");
            }

            await RespondAsync
            (
                embed: new EmbedBuilder()
                    .WithDescription($"Now Playing: [{player.CurrentTrack?.Title}]({player.CurrentTrack?.Uri})" + Environment.NewLine + TrackListBuilder.ToString())
                    .Build()
            );
            
            TrackListBuilder.Clear();
        }
    }

    [SlashCommand("volume", "Sets the volume of the player.")]
    public async Task SetVolumeAsync
    (
        [Summary(description: "The volume of the player ranging from 0 to 100.")]
        [MinValue(0)]
        [MaxValue(100)]
        int volume
    )
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        await player.SetVolumeAsync(volume / 100f);
        await RespondAsync($"Set volume to {volume}%.");
    }

    [SlashCommand("shuffle", "Shuffles the queue.")]
    public async Task ShuffleAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.Queue.Count < 1)
        {
            await RespondAsync("You only have a single track in the queue.");
            return;
        }

        await player.Queue.ShuffleAsync();
        await RespondAsync("Queue shuffled.");
    }

    [SlashCommand("clear", "Clears the queue.")]
    public async Task ClearQueueAsync()
    {
        QueuedLavalinkPlayer? player = await GetPlayerAsync(connectToVoiceChannel: false);

        if (player is null)
        {
            return;
        }

        if (player.Queue.Count < 0)
        {
            await RespondAsync("No queue exists in order to clear.");
        }

        await database.ClearAsync<TrackQueueEntity>(x => x.GuildId == Context.Guild.Id);

        await player.Queue.ClearAsync();
        await RespondAsync("Queue cleared.");
    }

    private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = true)
    {
        PlayerResult<QueuedLavalinkPlayer> result = await audio.Players.RetrieveAsync
        (
            Context,
            PlayerFactory.Queued,
            new PlayerRetrieveOptions(connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None)
        );

        if (result.IsSuccess)
        {
            return result.Player;
        }

        await ModifyOriginalResponseAsync(x => x.Content = result.Status switch
        {
            PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
            PlayerRetrieveStatus.BotNotConnected => "I am currently not connected to a voice channel.",
            _ => "Unknown error.",
        });
        
        return null;
    }
}
