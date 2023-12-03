using System.Collections.Concurrent;

namespace Suruga.Intermediary;

public sealed class TrackSelectionNotificationMediator
{
    private ConcurrentQueue<TrackSelectionNotification> Notifications { get; set; } = new();

    internal void Add(TrackSelectionNotification notification)
        => Notifications.Enqueue(notification);

    internal TrackSelectionNotification? Get()
    {
        if (!Notifications.TryDequeue(out TrackSelectionNotification? notification))
        {
            return null;
        }

        return notification;
    }
}
