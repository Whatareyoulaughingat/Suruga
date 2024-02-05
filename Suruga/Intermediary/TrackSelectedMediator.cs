using System.Collections.Concurrent;

namespace Suruga.Intermediary;

public sealed class TrackSelectedMediator
{
    private ConcurrentQueue<TrackSelectedMessage> Notifications { get; set; } = new();

    internal void Add(TrackSelectedMessage notification)
        => Notifications.Enqueue(notification);

    internal TrackSelectedMessage? Get()
    {
        if (!Notifications.TryDequeue(out TrackSelectedMessage? notification))
        {
            return null;
        }

        return notification;
    }
}
