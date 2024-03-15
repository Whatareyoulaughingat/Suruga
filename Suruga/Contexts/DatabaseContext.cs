using Lavalink4NET.Players.Queued;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel.ChatCompletion;
using Suruga.Contexts.Entities;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Suruga.Contexts;

/// <summary>
/// Represents a database context for handling Large Language Model sessions and music track queues.
/// </summary>
/// <remarks>
/// This class provides methods for retrieving, saving and clearing chat history and track queues asynchronously.
/// </remarks>
public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the <see cref="DbSet{AiSessionEntity}"/> of Large Language Model session entities.
    /// </summary>
    public DbSet<LargeLanguageModelSessionEntity> Sessions { get; init; } = null!;

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TrackQueueEntity}"/> of music track queue entities.
    /// </summary>
    public DbSet<TrackQueueEntity> TrackQueues { get; init; } = null!;

    private static readonly JsonSerializerOptions Options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    /// <summary>
    /// Retrieves a chat history asynchronously for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The chat history associated with the user, or null if not found.</returns>
    internal async ValueTask<ChatHistory?> RetrieveHistoryAsync(ulong userId)
    {
        LargeLanguageModelSessionEntity? session = await Sessions.FirstOrDefaultAsync(x => x.UserId == userId);

        if (session is null)
        {
            return null;
        }

        await using MemoryStream historyStream = await DecompressAsync(session.History);
        return await JsonSerializer.DeserializeAsync<ChatHistory>(historyStream);
    }

    /// <summary>
    /// Saves a chat history asynchronously for the specified user.
    /// </summary>
    /// <param name="history">The chat history to save.</param>
    /// <param name="userId">The ID of the user.</param>
    internal async ValueTask SaveHistoryAsync(ChatHistory history, ulong userId)
    {
        byte[] data = await CompressAsync(JsonSerializer.SerializeToUtf8Bytes(history, Options));
        LargeLanguageModelSessionEntity? session = await Sessions.FirstOrDefaultAsync(x => x.UserId == userId);

        if (session is not null)
        {
            session.History = data;
        }
        else
        {
            Add(new LargeLanguageModelSessionEntity
            {
                UserId = userId,
                History = data
            });
        }

        await SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a music track queue asynchronously for the specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <returns>The track queue associated with the guild, or null if not found.</returns>
    internal async ValueTask<IReadOnlyList<TrackQueueItem>?> RetrieveTrackQueueAsync(ulong guildId)
    {
        TrackQueueEntity? queue = await TrackQueues.FirstOrDefaultAsync(x => x.GuildId == guildId);

        if (queue is null)
        {
            return null;
        }

        await using MemoryStream identifierStream = await DecompressAsync(queue.Identifier);
        using StreamReader identiferReaderStream = new(identifierStream);

        string identifiers = await identiferReaderStream.ReadToEndAsync();

        return identifiers
            .Split(Environment.NewLine)
            .Select(identifier => new TrackQueueItem(identifier))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Saves a music track queue asynchronously for the specified guild.
    /// </summary>
    /// <param name="trackQueue">The track queue to save.</param>
    /// <param name="guildId">The ID of the guild.</param>
    internal async ValueTask SaveTrackQueueAsync(ITrackQueue trackQueue, ulong guildId)
    {
        if (!trackQueue.HasHistory)
        {
            return;
        }

        TrackQueueEntity? queue = await TrackQueues.FirstOrDefaultAsync(x => x.GuildId == guildId);

        string mergedIdentifiers = string.Join(Environment.NewLine, trackQueue.History.Select(track => track.Identifier));
        byte[] data = await CompressAsync(Encoding.UTF8.GetBytes(mergedIdentifiers));

        if (queue is not null)
        {
            queue.Identifier = data;
        }
        else
        {
            Add(new TrackQueueEntity
            {
                GuildId = guildId,
                Identifier = data
            });
        }

        await SaveChangesAsync();
    }

    /// <summary>
    /// Clears an entity from the database asynchronously based on the specified predicate.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <returns>True if the entity was found and removed; otherwise, false.</returns>
    internal async ValueTask<bool> ClearAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        DbSet<TEntity> set = Set<TEntity>();
        TEntity? entity = await set.FirstOrDefaultAsync(predicate);

        if (entity is null)
        {
            return false;
        }

        set.Remove(entity);
        await SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Decompresses the specified compressed data asynchronously.
    /// </summary>
    /// <remarks>
    /// Make sure to dispose the returned <see cref="MemoryStream"/>.
    /// </remarks>
    /// <param name="compressedData">The compressed data to decompress.</param>
    /// <returns>A <see cref="MemoryStream"/> containing the decompressed data.</returns>
    private static async ValueTask<MemoryStream> DecompressAsync(byte[] compressedData)
    {
        await using MemoryStream compressedDataStream = new(compressedData);
        await using BrotliStream decompressionStream = new(compressedDataStream, CompressionMode.Decompress);
        MemoryStream decompressedDataStream = new();

        await decompressionStream.CopyToAsync(decompressedDataStream);
        decompressedDataStream.Position = 0;

        return decompressedDataStream;
    }
    
    /// <summary>
    /// Compresses the specified uncompressed data asynchronously.
    /// </summary>
    /// <param name="uncompressedData">The uncompressed data to compress.</param>
    /// <returns>A <see cref="T:byte[]"/> containing the compressed data.</returns>
    private static async ValueTask<byte[]> CompressAsync(byte[] uncompressedData)
    {
        await using MemoryStream compressedDataStream = new();
        await using BrotliStream compressionStream = new(compressedDataStream, CompressionMode.Compress);

        await compressionStream.WriteAsync(uncompressedData);
        await compressionStream.FlushAsync();

        return compressedDataStream.ToArray();
    }

    /// <summary>
    /// Configures the database options using SQLite.
    /// </summary>
    /// <param name="options">The options builder for configuring the database.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=ai_sessions.db");
}
