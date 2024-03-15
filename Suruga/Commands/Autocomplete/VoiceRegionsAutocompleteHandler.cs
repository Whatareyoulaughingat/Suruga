using System.Collections.ObjectModel;
using Discord;
using Discord.Interactions;

namespace Suruga.Commands.Autocomplete;

/// <summary>
/// Handles autocomplete suggestions for voice regions.
/// </summary>
public sealed class VoiceRegionsAutocompleteHandler : AutocompleteHandler
{
    /// <summary>
    /// Generates autocomplete voice region suggestions asynchronously.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="autocompleteInteraction">The autocomplete interaction.</param>
    /// <param name="parameter">The parameter information.</param>
    /// <param name="services">The service provider.</param>
    /// <returns>An <see cref="AutocompletionResult"/> containing the generated voice region suggestions.</returns>
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync
    (
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services
    )
    {
        IReadOnlyCollection<IVoiceRegion> regions = await context.Client.GetVoiceRegionsAsync();

        ReadOnlyCollection<AutocompleteResult> suggestions = regions
            .Select(x => new AutocompleteResult(x.IsOptimal ? $"{x.Name} - (Most optimal for your region)" : x.Name, x.Id))
            .ToList()
            .AsReadOnly();
        
        return AutocompletionResult.FromSuccess(suggestions);
    }
}