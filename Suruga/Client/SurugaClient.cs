using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Cluster;
using Lavalink4NET.DSharpPlus;
using Lavalink4NET.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Suruga.Handlers.Application;
using Suruga.Handlers.Discord;
using Suruga.Services;

namespace Suruga.Client
{
    public class SurugaClient
    {
        private readonly ServiceProvider serviceProvider;

        private readonly DiscordShardedClient discordClient;
        private readonly InactivityTrackingService inactivityTracking;
        private readonly IAudioService audioService;

        public SurugaClient()
        {
            // Build the DI.
            serviceProvider = new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(new DiscordConfiguration
                {
                    Token = ConfigurationHandler.Data.Token,
                    Intents = DiscordIntents.GuildMessages | DiscordIntents.GuildVoiceStates | DiscordIntents.Guilds,
                    LogTimestampFormat = "hh:mm:ss",
                    MinimumLogLevel = LogLevel.Debug,
                }))

                .AddSingleton<InactivityTrackingService>()
                .AddSingleton<IAudioService, LavalinkCluster>()
                .AddSingleton<IDiscordClientWrapper, DiscordShardedClientWrapper>()
                .AddSingleton(new LavalinkClusterOptions
                {
                    Nodes = new[]
                    {
                            new LavalinkNodeOptions
                            {
                                RestUri = "http://localhost:2333",
                                WebSocketUri = "ws://localhost:2333",
                                Password = "youshallnotpass",
                            },

                            new LavalinkNodeOptions
                            {
                                RestUri = "http://localhost:2333",
                                WebSocketUri = "ws://localhost:2333",
                                Password = "youshallnotpass",
                            },

                            new LavalinkNodeOptions
                            {
                                RestUri = "http://localhost:2333",
                                WebSocketUri = "ws://localhost:2333",
                                Password = "youshallnotpass",
                            },

                            new LavalinkNodeOptions
                            {
                                RestUri = "http://localhost:2333",
                                WebSocketUri = "ws://localhost:2333",
                                Password = "youshallnotpass",
                            },
                    },

                    StayOnline = true,
                })
                .AddSingleton(new InactivityTrackingOptions
                {
                    DisconnectDelay = TimeSpan.FromMinutes(5.00),
                })

                .AddSingleton<MusicService>()
                .BuildServiceProvider();

            // Get required services from DI.
            discordClient = serviceProvider.GetRequiredService<DiscordShardedClient>();
            inactivityTracking = serviceProvider.GetRequiredService<InactivityTrackingService>();
            audioService = serviceProvider.GetRequiredService<IAudioService>();
        }

        /// <summary>
        /// Starts asynchronously this bot.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task RunAsync()
        {
            await InitializeCommandHandlerAsync().ConfigureAwait(false);
            await InitializeEventsAsync().ConfigureAwait(false);

            await discordClient.StartAsync();
            await Task.Delay(-1).ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the bot's events.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        private async Task InitializeEventsAsync()
        {
            discordClient.Ready += async (_, _) =>
            {
                await Task.Factory.StartNew(async () =>
                {
                    await discordClient.UpdateStatusAsync(new DiscordActivity(ConfigurationHandler.Data.Activity, ConfigurationHandler.Data.ActivityType));
                    await audioService.InitializeAsync();

                    inactivityTracking.BeginTracking();
                });
            };

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the default command handler.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        private async Task InitializeCommandHandlerAsync()
        {
            IReadOnlyDictionary<int, CommandsNextExtension> commandsNext = await discordClient.UseCommandsNextAsync(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { ConfigurationHandler.Data.CommandPrefix },
                IgnoreExtraArguments = true,
                Services = serviceProvider,
                CaseSensitive = true,
            });

            foreach (CommandsNextExtension commands in commandsNext.Values)
            {
                commands.RegisterCommands(Assembly.GetExecutingAssembly());
            }
        }
    }
}