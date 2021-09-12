using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Suruga.Handlers.Application;
using Suruga.Injectors;

namespace Suruga.Client;

public class SurugaClient
{
    private readonly DiscordShardedClient discordClient;
    private readonly InactivityTrackingService inactivityTracking;
    private readonly IAudioService audioService;

    public SurugaClient()
    {
        DependencyInjector.Inject();

        // Get required services from DI.
        discordClient = DependencyInjector.Services.GetRequiredService<DiscordShardedClient>();
        inactivityTracking = DependencyInjector.Services.GetRequiredService<InactivityTrackingService>();
        audioService = DependencyInjector.Services.GetRequiredService<IAudioService>();
    }

    /// <summary>
    /// Starts asynchronously this bot.
    /// </summary>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    public async Task RunAsync()
    {
        await InitializeCommandHandlerAsync().ConfigureAwait(false);
        InitializeEvents();

        await discordClient.StartAsync();
        await Task.Delay(-1).ConfigureAwait(false);
    }

    /// <summary>
    /// Initializes the bot's events.
    /// </summary>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    private void InitializeEvents()
    {
        discordClient.Ready += async (client, readyEventArgs) =>
        {
            await Task.Factory.StartNew(async () =>
            {
                await discordClient.UpdateStatusAsync(new DiscordActivity(ConfigurationHandler.Data.Activity, Enum.Parse<ActivityType>(ConfigurationHandler.Data.ActivityType)));
                await audioService.InitializeAsync();

                inactivityTracking.BeginTracking();
            });
        };
    }

    /// <summary>
    /// Initializes the command handler.
    /// </summary>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    private async Task InitializeCommandHandlerAsync()
    {
        IReadOnlyDictionary<int, CommandsNextExtension> commandsNext = await discordClient.UseCommandsNextAsync(new CommandsNextConfiguration
        {
            StringPrefixes = ConfigurationHandler.Data.CommandPrefixes.Values,
            IgnoreExtraArguments = true,
            Services = DependencyInjector.Services,
        });

        foreach (CommandsNextExtension commands in commandsNext.Values)
        {
            commands.RegisterCommands(Assembly.GetExecutingAssembly());
        }
    }
}
