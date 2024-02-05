using Lavalink4NET.Tracks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Interactivity;
using System.Text;

namespace Suruga.Handlers;

internal sealed class TrackSelectionEmbedHelper
{
    internal static StringBuilder EmbedTrackBuilder { get; } = new("Select a track by clicking one of the buttons below." + Environment.NewLine);

    internal static List<ButtonComponent> TrackSelectionButtons { get; } = [];

    internal static void GenerateButtons(IReadOnlyList<LavalinkTrack> tracks)
    {
        for (int i = 1; i <= tracks.Count; i++)
        {
            EmbedTrackBuilder.AppendLine($"{i}. [{tracks[i].Title}]({tracks[i]?.Uri}) ({tracks[i]?.Duration})");

            TrackSelectionButtons.Add
            (
                new ButtonComponent
                (
                    Style: ButtonComponentStyle.Primary,
                    Label: i.ToString(),
                    CustomID: CustomIDHelpers.CreateButtonIDWithState($"track-selection", i.ToString())
                )
            );
        }

        TrackSelectionButtons.Add
        (
            new ButtonComponent
            (
                Style: ButtonComponentStyle.Danger,
                Label: "Cancel",
                CustomID: CustomIDHelpers.CreateButtonID("cancel-track-selection")
            )
        );
    }

    internal static void Clear()
    {
        EmbedTrackBuilder.Clear();
        TrackSelectionButtons.Clear();
    }
}
