using Discord.Interactions;
using Suruga.Commands;

namespace Suruga.Options;

/// <summary>
/// Stores the file path of the specified large language model in order to determine if <see cref="LargeLanguageModelCommands"/>
/// should be added to the <see cref="InteractionService"/>.
/// </summary>
public sealed record LargeLanguageModelCommandsOptions
{
    /// <summary>
    /// The file path of the model.
    /// </summary>
    public required FileInfo? Model { get; set; }

    /// <summary>
    /// Instructions to be passed to the model. Can contain various characteristics such as personality, likes/dislikes, speech patterns, etc.
    /// </summary>
    public string? Instructions { get; set; }
}
