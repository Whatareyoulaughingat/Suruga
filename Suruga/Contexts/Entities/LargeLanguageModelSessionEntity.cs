using System.ComponentModel.DataAnnotations;

namespace Suruga.Contexts.Entities;

/// <summary>
/// Represents an entity storing Large Language Model session information.
/// </summary>
public sealed class LargeLanguageModelSessionEntity
{
    /// <summary>
    /// Gets the unique identifier of the user associated with the Large Language Model session.
    /// </summary>
    /// <value>The user ID.</value>
    [Key]
    public required ulong UserId { get; init; }

    /// <summary>
    /// Gets or sets the history of the Large Language Model session stored as a compressed UTF-8 byte array.
    /// </summary>
    /// <value>The compressed byte array representing the user's session history.</value>
    [Required]
    public required byte[] History { get; set; }
}
