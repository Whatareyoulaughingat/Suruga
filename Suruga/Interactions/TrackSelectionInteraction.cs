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
    TrackSelectionNotificationMediator mediator,
    FeedbackService feedbackService,
    IInteractionCommandContext ctx,
    IDiscordRestInteractionAPI interactionApi,
    IDiscordRestChannelAPI channelApi
) : InteractionGroup
{
    private readonly TrackSelectionNotificationMediator mediator = mediator;

    private readonly FeedbackService feedbackService = feedbackService;

    private readonly IInteractionCommandContext ctx = ctx;

    private readonly IDiscordRestInteractionAPI interactionApi = interactionApi;

    private readonly IDiscordRestChannelAPI channelApi = channelApi;

    [Button("track-selection")]
    public async ValueTask<Result> OnTrackSelectionClicked(string state)
    {
        TrackSelectionNotification? notification = mediator.Get();

        if (notification is not TrackSelectionNotification validNotification)
        {
            return Result.FromSuccess();
        }

        LavalinkTrack selectedTrack = validNotification.Tracks[int.Parse(state)];

        await interactionApi.EditOriginalInteractionResponseAsync
        (
            applicationID: ctx.Interaction.ApplicationID,
            token: ctx.Interaction.Token,
            content: $"Enqueued [{selectedTrack.Title}]({selectedTrack?.Uri})",
            components: null,
            ct: CancellationToken
        );

        await validNotification.Player.PlayAsync(selectedTrack, cancellationToken: CancellationToken);
        return (Result)await feedbackService.SendContextualSuccessAsync($"Now playing: [{selectedTrack.Title}]({selectedTrack?.Uri})", ct: CancellationToken);
    }

    [Button("cancel-track-selection")]
    public async ValueTask<Result> OnCancelTrackSelectionClicked()
    {
        TrackSelectionNotification? notification = mediator.Get();

        if (notification is not TrackSelectionNotification validNotification)
        {
            return Result.FromSuccess();
        }

        await interactionApi.EditOriginalInteractionResponseAsync(ctx.Interaction.ApplicationID, ctx.Interaction.Token, components: null, ct: CancellationToken);

        Result<IChannel> voiceChannelResult = await channelApi.GetChannelAsync(DiscordSnowflake.New(validNotification.Player.VoiceChannelId), CancellationToken);

        if (!voiceChannelResult.IsDefined(out IChannel? voiceChannel))
        {
            return (Result)await feedbackService.SendContextualErrorAsync("Failed to find voice channel.");
        }

        await validNotification.Player.DisconnectAsync(CancellationToken);
        return (Result)await feedbackService.SendContextualSuccessAsync($"Left {voiceChannel?.Name.OrDefault()}", ct: CancellationToken);
    }
}
