using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;
using System.Collections.Immutable;

namespace Suruga.Intermediary;

internal sealed record TrackSelectionNotification(IQueuedLavalinkPlayer Player, PlayerState State, ImmutableArray<LavalinkTrack> Tracks);
