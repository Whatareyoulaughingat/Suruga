using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using Suruga.Handlers;
using Suruga.Modules;
using Suruga.Services;

namespace Suruga.Client
{
    public class SurugaClient
    {
        private readonly ServiceProvider serviceProvider;

        private readonly DiscordShardedClient discordShardedClient;
        private readonly LavalinkConfiguration lavalinkConfiguration;

        public SurugaClient()
        {
            // Build the DI.
            serviceProvider = new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(new DiscordConfiguration
                {
                    Token = ConfigurationHandler.Configuration.Token,
                    Intents = DiscordIntents.GuildMessages | DiscordIntents.GuildVoiceStates | DiscordIntents.Guilds,
                }))

                .AddSingleton(new LavalinkConfiguration())

                .AddSingleton<MusicService>()
                .BuildServiceProvider();

            // Get required services from DI.
            discordShardedClient = serviceProvider.GetRequiredService<DiscordShardedClient>();
            lavalinkConfiguration = serviceProvider.GetRequiredService<LavalinkConfiguration>();
        }

        /// <summary>
        /// Starts asynchronously this bot.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task RunAsync()
        {
            await InitializeEvents();
            await InitializeCommandHandler();

            await discordShardedClient.StartAsync();
            await Task.Delay(-1);
        }

        /// <summary>
        /// Initializes the bot's events.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        private async Task InitializeEvents()
        {
            // DSharpPlus events.
            discordShardedClient.Ready += async (discordShardedClient, args) =>
            {
                await Task.Factory.StartNew(async () =>
                {
                    await discordShardedClient.UpdateStatusAsync(new DiscordActivity(ConfigurationHandler.Configuration.Activity, ConfigurationHandler.Configuration.ActivityType));

                    // Initialize Lavalink and connect to it.
                    LavalinkExtension lavalinkExtension = discordShardedClient.UseLavalink();
                    await lavalinkExtension.ConnectAsync(lavalinkConfiguration);
                });
            };

            await Task.CompletedTask;
        }

        /// <summary>
        /// Initializes the default command handler.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        private async Task InitializeCommandHandler()
        {
            IReadOnlyDictionary<int, CommandsNextExtension> commandsNextExtension = await discordShardedClient.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { ConfigurationHandler.Configuration.CommandPrefix },
                IgnoreExtraArguments = true,
                Services = serviceProvider,
            });

            foreach (KeyValuePair<int, CommandsNextExtension> item in commandsNextExtension)
            {
                item.Value.RegisterCommands<Music>();
            }
        }
    }
}