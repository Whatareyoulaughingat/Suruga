using Lavalink4NET.Artwork;
using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking.Extensions;
using Lavalink4NET.Lyrics;
using Lavalink4NET.Remora.Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using Remora.Discord.Interactivity.Extensions;
using Suruga.Commands;
using Suruga.Interactions;
using Suruga.Intermediary;
using System.Collections.Immutable;

namespace Suruga;

internal sealed class Program
{
    private static async Task Main(
        string token,
        UserStatus? status,
        string? activityName,
        ActivityType? activityType,
        string lavalinkHttpAddress = "http://localhost:2333",
        string lavalinkWsAddress = "ws://localhost:2333",
        string lavalinkPassword = "youshallnotpass")
    {
        Console.Title = "Suruga";

        IHost host = Host.CreateDefaultBuilder()
            .UseConsoleLifetime()
            .AddDiscordService(_ => token)
            .ConfigureServices(services =>
            {
                services.AddSingleton<TrackSelectionNotificationMediator>();

                services
                .AddSingleton<SlashService>()
                .AddDiscordCommands(enableSlash: true)
                .AddCommandTree().WithCommandGroup<AudioCommands>().Finish()
                .AddInteractivity()
                .AddInteractionGroup<TrackSelectionInteraction>();

                services
                .AddLavalink()
                .AddInactivityTracking()
                .AddSingleton<ILyricsService, LyricsService>()
                .AddSingleton<IArtworkService, ArtworkService>()
                .AddMemoryCache();

                services
                .Configure<DiscordGatewayClientOptions>(options =>
                {
                    options.Intents |= GatewayIntents.GuildVoiceStates | GatewayIntents.MessageContents;
                    options.Presence = new UpdatePresence
                    (
                        Status: status ?? UserStatus.Online,
                        IsAFK: false,
                        Since: null,
                        Activities: ImmutableArray.Create(new Activity(activityName ?? string.Empty, activityType.GetValueOrDefault()))
                    );
                })
                .ConfigureLavalink(options =>
                {
                    string lavalinkWsFullAddress = lavalinkWsAddress.EndsWith('/')
                        ? lavalinkWsAddress + "v4/websocket"
                        : lavalinkWsAddress + "/v4/websocket";

                    options.BaseAddress = new(lavalinkHttpAddress);
                    options.WebSocketUri = new(lavalinkWsFullAddress);
                    options.Passphrase = lavalinkPassword;
                })
                .ConfigureInactivityTracking(options => options.DefaultTimeout = TimeSpan.FromMinutes(2.00));
            })
            .ConfigureLogging(logging =>
            {
                logging
                .AddConsole()
                .AddFilter("System.Net.Http.HttpClient.*.LogicalHandler", LogLevel.Warning)
                .AddFilter("System.Net.Http.HttpClient.*.ClientHandler", LogLevel.Warning)
#if DEBUG
                .SetMinimumLevel(LogLevel.Debug);
#else
                .SetMinimumLevel(LogLevel.Information);
#endif
            })
            .Build();

        SlashService slashService = host.Services.GetRequiredService<SlashService>();
#if DEBUG
        await slashService.UpdateSlashCommandsAsync(DiscordSnowflake.New(1159852457548058744));
#else
        await slashService.UpdateSlashCommandsAsync();
#endif
        await host.RunAsync();
        await host.WaitForShutdownAsync();
    }
}
