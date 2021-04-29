using Lavalink4NET.Player;

namespace Suruga.Extensions
{
    public static class TrackEndEventArgsExtensions
    {
        public static QueuedLavalinkPlayer ToQueuedLavalinkPlayer(this LavalinkPlayer player)
        {
            return _ = player as QueuedLavalinkPlayer;
        }
    }
}
