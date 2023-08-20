using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Suruga.Client;
using Suruga.Global;
using Suruga.Handlers;
using Suruga.Logging;
using Suruga.Services;
using System;
using System.IO;
using Victoria;
using HostProvider = Microsoft.Extensions.Hosting.Host;

namespace Suruga;

internal sealed class Program
{
    internal static IHost Host { get; private set; }

    private static void Main()
    {
        Console.Title = "Suruga";

        Host = HostProvider
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                Directory.CreateDirectory(Paths.BinariesDirectory);
                Directory.CreateDirectory(Paths.LogsDirectory);

                if (!File.Exists(Paths.ConfigurationFile))
                {
                    File.WriteAllText(Paths.ConfigurationFile,
                        """
                        Token=
                        CommandPrefix=?
                        Status=Online
                        ActivityName=
                        ActivityDescription=
                        ActivityStatus=
                        HexColorOnSuccessfulCommand=
                        HexColorOnUnssuccessfulCommand=
                        LavalinkHostname=localhost
                        LavalinkPort=2333
                        DeafenOnVoiceChannel=True
                        """);

                    Console.WriteLine($"A configuration file has been generated in {Paths.ConfigurationFile}. Fill in every value and run the bot again.");
                    Environment.Exit(0);
                }

                config.AddIniFile(Paths.ConfigurationFile, optional: false, reloadOnChange: false);
            })
            .ConfigureServices((ctx, services) =>
            {
                services
                .AddSingleton(ctx.Configuration)
                .AddSingleton<ConsoleFormatter, SimpleColoredConsoleFormatter>()
                .AddLogging(builder =>
                {
#if DEBUG
                    builder.SetMinimumLevel(LogLevel.Debug);
#else
                    builder.SetMinimumLevel(LogLevel.Information);
#endif
                    builder.AddConsole(x => x.FormatterName = "simple-colored-console");
                })
                .AddSingleton<ProgramLogger>()
                .AddSingleton<ClientLogger>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<InteractionHandler>()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.GuildVoiceStates | GatewayIntents.MessageContent | GatewayIntents.GuildMessages,
                    LogLevel = LogSeverity.Debug,
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Debug,
                    DefaultRunMode = Discord.Commands.RunMode.Async,
                    CaseSensitiveCommands = false,
                }))
                .AddSingleton((services) =>
                {
                    DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
                    return new InteractionService(client, new InteractionServiceConfig
                    {
                        DefaultRunMode = Discord.Interactions.RunMode.Async,
                        LogLevel = LogSeverity.Debug,
                        UseCompiledLambda = true,
                    });
                })
                .AddSingleton<MusicService>()
                .AddLavaNode();
            })
            .UseConsoleLifetime()
            .Build();

        Host.Start();
        new SurugaClient().RunAsync().GetAwaiter().GetResult();
    }
}
