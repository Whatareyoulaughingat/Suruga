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
using Microsoft.Extensions.DependencyInjection;
using Suruga.Handlers;
using Suruga.Services;

namespace Suruga.Client
{
    public class SurugaClient
    {
        private readonly ServiceProvider serviceProvider;

        private readonly DiscordShardedClient discordClient;
        private readonly IAudioService audioService;

        public SurugaClient()
        {
            try
            {
                // Build the DI.
                serviceProvider = new ServiceCollection()
                    .AddSingleton(new DiscordShardedClient(new DiscordConfiguration
                    {
                        Token = ConfigurationHandler.Configuration.Token,
                        Intents = DiscordIntents.GuildMessages | DiscordIntents.GuildVoiceStates | DiscordIntents.Guilds,
                    }))

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
                        },

                        LoadBalacingStrategy = LoadBalancingStrategies.ScoreStrategy,
                        StayOnline = true,
                    })

                    .AddSingleton<ChannelService>()
                    .AddSingleton<HentaiService>()
                    .AddSingleton<MiscService>()
                    .AddSingleton<MusicService>()
                    .BuildServiceProvider();

                // Get required services from DI.
                discordClient = serviceProvider.GetRequiredService<DiscordShardedClient>();
                audioService = serviceProvider.GetRequiredService<IAudioService>();
            }
            catch
            {
                throw new Exception();
            }
            finally
            {
                bool disposed = false;
                if (disposed == true)
                {
                    serviceProvider.Dispose();
                }
            }
        }

        /// <summary>
        /// Starts asynchronously this bot.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task RunAsync()
        {
            await InitializeEventsAsync();
            await InitializeCommandHandlerAsync();

            await discordClient.StartAsync();
            await Task.Delay(-1);
        }

        /// <summary>
        /// Initializes the bot's events.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        private Task InitializeEventsAsync()
        {
            discordClient.Ready += async (discordShardedClient, args) =>
            {
                using Task onReadyEventInitialization = await Task.Factory.StartNew(async () =>
                {
                    await discordClient.UpdateStatusAsync(new DiscordActivity(ConfigurationHandler.Configuration.Activity, ConfigurationHandler.Configuration.ActivityType));
                    await audioService.InitializeAsync();
                });
            };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes the default command handler.
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        /// </summary>
        private async Task InitializeCommandHandlerAsync()
        {
            IReadOnlyDictionary<int, CommandsNextExtension> commandsNext = await discordClient.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { ConfigurationHandler.Configuration.CommandPrefix },
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