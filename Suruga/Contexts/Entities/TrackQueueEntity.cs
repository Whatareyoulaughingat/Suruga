using System.ComponentModel.DataAnnotations;

namespace Suruga.Contexts.Entities;

/// <summary>
/// Represents an entity storing information about music track queues.
/// </summary>
public sealed class TrackQueueEntity
{
    /// <summary>
    /// Gets the unique identifier of the guild associated with the music track queue.
    /// </summary>
    /// <value>The unique identifier of the guild.</value>
    [Key]
    public required ulong GuildId { get; init; }

    /// <summary>
    /// Gets or sets the track identifiers of the music track queue stored as a compressed UTF-8 byte array.
    /// </summary>
    /// <value>The compressed byte array representing the track queue identifiers.</value>
    [Required]
    public required byte[] Identifier { get; set; }
}
