using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Suruga.GlobalData;

namespace Suruga.Handlers.Application;

public class DatabaseHandler : DbContext
{
    public DatabaseHandler()
        => Database.Migrate();

    /// <summary>
    /// Adds a new entity unless it already exists.
    /// </summary>
    /// <typeparam name="TEntity">The entity.</typeparam>
    /// <param name="predicate">The predicate for checking if the entity exists or not.</param>
    /// <param name="entityValue">The collection of the entity.</param>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    public async Task AddEntityAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Func<TEntity> entityValue) where TEntity : class
    {
        TEntity entity = await Set<TEntity>().FirstOrDefaultAsync(predicate);

        if (entity != null)
        {
            return;
        }

        TEntity newEntity = entityValue();

        await AddAsync(newEntity);
        await SaveChangesAsync();
    }

    public async Task AddEntityAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await AddAsync(entity);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Gets or adds an entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity.</typeparam>
    /// <param name="predicate">The predicate for checking if the entity exists already or it's new.</param>
    /// <param name="entityValue">The new values for the entity if it doesn't exist.</param>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    public async Task<TEntity> GetOrAddEntityAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Func<TEntity> entityValue) where TEntity : class
    {
        TEntity entity = await Set<TEntity>().FirstOrDefaultAsync(predicate);

        if (entity != null)
        {
            return entity;
        }

        TEntity newEntity = entityValue();
        await AddAsync(newEntity);

        return newEntity;
    }

    /// <summary>
    /// Gets a specific entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="predicateEntityExists">The predicate if the entity exists.</param>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    public async Task<TEntity> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>> predicateEntityExists) where TEntity : class
    {
        TEntity entity = await Set<TEntity>().FirstOrDefaultAsync(predicateEntityExists);
        return entity ?? throw new Exception("Could not get entity because it does not exist.");
    }

    /// <summary>
    /// Removes a specific entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="entity">The entity itself.</param>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    public async Task RemoveEntityAsync<TEntity>(TEntity entity) where TEntity : class
    {
        Remove(entity);
        await SaveChangesAsync();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={Paths.Base}\\LiveWallpapers.db;");
}
