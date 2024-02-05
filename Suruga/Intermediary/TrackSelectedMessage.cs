using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;
using System.Collections.ObjectModel;

namespace Suruga.Intermediary;

internal sealed record TrackSelectedMessage(IQueuedLavalinkPlayer Player, PlayerState State, ReadOnlyCollection<LavalinkTrack> Tracks);
