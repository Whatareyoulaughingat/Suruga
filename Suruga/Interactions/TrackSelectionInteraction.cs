using Lavalink4NET.Tracks;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Interactivity;
using Remora.Results;
using Suruga.Intermediary;

namespace Suruga.Interactions;

public sealed class TrackSelectionInteraction
(
    TrackSelectedMediator mediator,
    IFeedbackService feedbackService,
    IInteractionCommandContext ctx,
    IDiscordRestInteractionAPI interactionApi,
    IDiscordRestChannelAPI channelApi
) : InteractionGroup
{
    [Button("track-selection")]
    public async ValueTask<Result> OnTrackSelectionClicked(string state)
    {
        TrackSelectedMessage? notification = mediator.Get();

        if (notification is null)
        {
            return Result.FromSuccess();
        }

        LavalinkTrack selectedTrack = notification.Tracks[int.Parse(state)];

        await interactionApi.EditOriginalInteractionResponseAsync
        (
            applicationID: ctx.Interaction.ApplicationID,
            token: ctx.Interaction.Token,
            content: $"Enqueued [{selectedTrack.Title}]({selectedTrack?.Uri})",
            components: null,
            ct: CancellationToken
        );

        await notification.Player.PlayAsync(selectedTrack!, cancellationToken: CancellationToken);
        return (Result)await feedbackService.SendContextualSuccessAsync($"Now playing: [{selectedTrack.Title}]({selectedTrack?.Uri})", ct: CancellationToken);
    }

    [Button("cancel-track-selection")]
    public async ValueTask<Result> OnCancelTrackSelectionClicked()
    {
        TrackSelectedMessage? notification = mediator.Get();

        if (notification is null)
        {
            return Result.FromSuccess();
        }

        await interactionApi.EditOriginalInteractionResponseAsync(ctx.Interaction.ApplicationID, ctx.Interaction.Token, components: null, ct: CancellationToken);

        Result<IChannel> voiceChannelResult = await channelApi.GetChannelAsync(DiscordSnowflake.New(notification.Player.VoiceChannelId), CancellationToken);

        if (!voiceChannelResult.IsDefined(out IChannel? voiceChannel))
        {
            return (Result)await feedbackService.SendContextualErrorAsync("Failed to find voice channel.");
        }

        await notification.Player.DisconnectAsync(CancellationToken);
        return (Result)await feedbackService.SendContextualSuccessAsync($"Left {voiceChannel?.Name.OrDefault()}", ct: CancellationToken);
    }
}
