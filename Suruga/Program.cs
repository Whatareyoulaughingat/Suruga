using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Suruga.Contexts;
using Suruga.Options;
using Suruga.Services;

namespace Suruga;

internal sealed class Program
{
    private static async Task Main
    (
        string token,
        FileInfo? llmModel = null,
        string? llmInstructions = null,
        bool enableMusicCommands = false,
        string lavalinkRestHostname = "http://localhost:2333",
        string lavalinkWebsocketHostname = "ws://localhost:2333/v4/websocket",
        string lavalinkPassword = "youshallnotpass"
    )
    {
        Console.Title = "Suruga";
            
        await Host
        .CreateDefaultBuilder()
        .ConfigureServices(services =>
        {
            services.AddLogging(x => x.AddConsole());

            services.AddOptions<LargeLanguageModelCommandsOptions>().Configure(x =>
            {
                x.Model = llmModel;
                x.Instructions = Path.Exists(llmInstructions) ? File.ReadAllText(llmInstructions) : llmInstructions;
            });
            
            services.AddOptions<LavalinkConnectionServiceOptions>().Configure(x => x.EnableMusicCommands = enableMusicCommands);

            services
            .AddDbContext<DbContext, DatabaseContext>(x => x.UseSqlite("Data Source=ai_sessions.db"))
            .AddMemoryCache()
            .AddHttpClient();

            services
            .AddLavalink()
            .ConfigureLavalink(x =>
            { 
                x.BaseAddress = new Uri(lavalinkRestHostname);
                x.WebSocketUri = new Uri(lavalinkWebsocketHostname);
                x.Passphrase = lavalinkPassword;
            })
            .AddHostedService<LavalinkConnectionService>();

            services.AddDiscordHost((configuration, _) =>
            {
                configuration.SocketConfig = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildBans | GatewayIntents.GuildVoiceStates,
                };

                configuration.Token = token;
            });

            services.AddInteractionService((configuration, _) =>
            {
                configuration.LogLevel = LogSeverity.Info;
                configuration.DefaultRunMode = RunMode.Async;
            });

            services
            .AddHostedService<DatabaseMigrationService>()
            .AddHostedService<SlashCommandService>();
        })
        .RunConsoleAsync();
    }
}
