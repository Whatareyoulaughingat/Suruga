using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Cluster;
using Lavalink4NET.DSharpPlus;
using Lavalink4NET.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Suruga.Handlers;
using Suruga.Modules;
using Suruga.Services;

namespace Suruga.Client;

public class SurugaClient
{
    public static ServiceProvider Services { get; private set; }

    private readonly DiscordClient discordClient;
    private readonly InactivityTrackingService inactivityTracking;
    private readonly IAudioService audioService;

    public SurugaClient()
    {
        // Setup DI.
        Services = new ServiceCollection()
            // DSharpPlus singletons.
            .AddSingleton(new DiscordShardedClient(new DiscordConfiguration
            {
                Token = ConfigurationHandler.Data.Token,
                Intents = DiscordIntents.GuildMessages | DiscordIntents.GuildVoiceStates | DiscordIntents.Guilds,
                LogTimestampFormat = "hh:mm:ss",
            }))

            // Lavalink4NET singletons.
            .AddSingleton<IAudioService, LavalinkNode>()
            .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
            .AddSingleton(new LavalinkNodeOptions
            {
                AllowResuming = true,
                Decompression = true,
                DisconnectOnStop = false,
                Password = "youshallnotpass",
                RestUri = "http://localhost:2333",
                WebSocketUri = "ws://localhost:2333",
            })
            .AddSingleton<InactivityTrackingService>()
            .AddSingleton(new InactivityTrackingOptions
            {
                DisconnectDelay = TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationHandler.Data.DisconnectFromVCAfterMinutes)),
                PollInterval = TimeSpan.FromSeconds(30.0),
                TrackInactivity = true,
            })

            // Modules (commands) singletons.
            .AddSingleton<MusicService>()
            .BuildServiceProvider();

        // Get required services from DI.
        discordClient = Services.GetRequiredService<DiscordClient>();
        inactivityTracking = Services.GetRequiredService<InactivityTrackingService>();
        audioService = Services.GetRequiredService<IAudioService>();
    }

    public async Task RunAsync()
    {
        InitializeCommandHandler();
        InitializeEvents();

        await discordClient.ConnectAsync();
        await Task.Delay(-1).ConfigureAwait(false);
    }

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

    private void InitializeCommandHandler()
    {
        CommandsNextExtension commandsNext = discordClient.UseCommandsNext(new CommandsNextConfiguration
        {
            StringPrefixes = ConfigurationHandler.Data.CommandPrefixes,
            IgnoreExtraArguments = true,
            Services = Services,
        });

        commandsNext.RegisterCommands<Music>();
    }
}
