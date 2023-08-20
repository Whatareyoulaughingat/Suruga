using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;

namespace Suruga.Infrastructure;

internal sealed class QueueEntry : IQueueEntry
{
    public required LavalinkTrack Track { get; set; }

    public Task AfterPlayingAsync(LavalinkGuildPlayer player)
    {
        player.RemoveFromQueue(this);
        return Task.CompletedTask;
    }

    public async Task<bool> BeforePlayingAsync(LavalinkGuildPlayer player)
    {
        await player.Channel.SendMessageAsync($"Playing {Track.Info.Title}");
        return true;
    }
}
