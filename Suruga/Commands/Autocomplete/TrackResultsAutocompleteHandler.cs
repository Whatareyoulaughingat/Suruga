using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using System.Collections.ObjectModel;

namespace Suruga.Commands.Autocomplete;

/// <summary>
/// Handles autocomplete suggestions for track results based on a query.
/// </summary>
public sealed class TrackResultsAutocompleteHandler(IAudioService audio) : AutocompleteHandler
{
    /// <summary>
    /// Generates autocomplete track suggestions asynchronously based on the provided query.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="autocompleteInteraction">The autocomplete interaction.</param>
    /// <param name="parameter">The parameter information.</param>
    /// <param name="services">The service provider.</param>
    /// <returns>An <see cref="AutocompletionResult"/> containing the track search result.</returns>
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync
    (
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services
    )
    {
        string? query = ((SocketAutocompleteInteraction)context.Interaction).Data.Current.Value.ToString();

        if (string.IsNullOrWhiteSpace(query))
        {
            return AutocompletionResult.FromSuccess();
        }

        TrackLoadResult result = await audio.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTubeMusic);

        if (result.IsFailed)
        {
            return AutocompletionResult.FromSuccess();
        }

        ReadOnlyCollection<AutocompleteResult> suggestions = result.Tracks
            .Where(track => !string.IsNullOrWhiteSpace(track.Title) && track.Uri is not null)
            .Select(track => new AutocompleteResult(track.Title, track.Uri!.AbsoluteUri))
            .ToList()
            .AsReadOnly();

        return AutocompletionResult.FromSuccess(suggestions);
    }
}
