using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Suruga.Services;

internal sealed class DatabaseMigrationService(DbContext database) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        => await database.Database.MigrateAsync(stoppingToken);
}
