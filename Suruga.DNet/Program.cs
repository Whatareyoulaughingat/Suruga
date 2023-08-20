using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Suruga.DNet.Handlers;
using Suruga.DNet.Logging;
using Suruga.DNet.Services;
using System;
using System.IO;
using Victoria;

namespace Suruga.DNet;

internal sealed class Program
{
    internal static IHost Host { get; private set; }

    private static void Main()
    {
        Console.Title = "Suruga";

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                Directory.CreateDirectory(PersistentStoragePaths.LavalinkDirectory);

                if (!File.Exists(PersistentStoragePaths.LavalinkYmlFile))
                {
                    File.WriteAllText(PersistentStoragePaths.LavalinkYmlFile,
                        """
                        server: # REST and WS server
                          port: 2333
                          address: 0.0.0.0
                        plugins:
                        #  name: # Name of the plugin
                        #    some_key: some_value # Some key-value pair for the plugin
                        #    another_key: another_value
                        lavalink:
                          plugins:
                        #    - dependency: "group:artifact:version"
                        #      repository: "repository"
                          pluginsDir: "./plugins"
                          server:
                            password: "youshallnotpass"
                            sources:
                              youtube: true
                              bandcamp: true
                              soundcloud: true
                              twitch: true
                              vimeo: true
                              http: true
                              local: false
                            filters: # All filters are enabled by default
                              volume: true
                              equalizer: true
                              karaoke: true
                              timescale: true
                              tremolo: true
                              vibrato: true
                              distortion: true
                              rotation: true
                              channelMix: true
                              lowPass: true
                            bufferDurationMs: 400 # The duration of the NAS buffer. Higher values fare better against longer GC pauses. Duration <= 0 to disable JDA-NAS. Minimum of 40ms, lower values may introduce pauses.
                            frameBufferDurationMs: 5000 # How many milliseconds of audio to keep buffered
                            opusEncodingQuality: 10 # Opus encoder quality. Valid values range from 0 to 10, where 10 is best quality but is the most expensive on the CPU.
                            resamplingQuality: LOW # Quality of resampling operations. Valid values are LOW, MEDIUM and HIGH, where HIGH uses the most CPU.
                            trackStuckThresholdMs: 10000 # The threshold for how long a track can be stuck. A track is stuck if does not return any audio data.
                            useSeekGhosting: true # Seek ghosting is the effect where whilst a seek is in progress, the audio buffer is read from until empty, or until seek is ready.
                            youtubePlaylistLoadLimit: 6 # Number of pages at 100 each
                            playerUpdateInterval: 5 # How frequently to send player updates to clients, in seconds
                            youtubeSearchEnabled: true
                            soundcloudSearchEnabled: true
                            gc-warnings: true
                            #ratelimit:
                              #ipBlocks: ["1.0.0.0/8", "..."] # list of ip blocks
                              #excludedIps: ["...", "..."] # ips which should be explicit excluded from usage by lavalink
                              #strategy: "RotateOnBan" # RotateOnBan | LoadBalance | NanoSwitch | RotatingNanoSwitch
                              #searchTriggersFail: true # Whether a search 429 should trigger marking the ip as failing
                              #retryLimit: -1 # -1 = use default lavaplayer value | 0 = infinity | >0 = retry will happen this numbers times
                            #youtubeConfig: # Required for avoiding all age restrictions by YouTube, some restricted videos still can be played without.
                              #email: "" # Email of Google account
                              #password: "" # Password of Google account
                            #httpConfig: # Useful for blocking bad-actors from ip-grabbing your music node and attacking it, this way only the http proxy will be attacked
                              #proxyHost: "localhost" # Hostname of the proxy, (ip or domain)
                              #proxyPort: 3128 # Proxy port, 3128 is the default for squidProxy
                              #proxyUser: "" # Optional user for basic authentication fields, leave blank if you don't use basic auth
                              #proxyPassword: "" # Password for basic authentication

                        metrics:
                          prometheus:
                            enabled: false
                            endpoint: /metrics

                        sentry:
                          dsn: ""
                          environment: ""
                        #  tags:
                        #    some_key: some_value
                        #    another_key: another_value

                        logging:
                          file:
                            path: ./logs/

                          level:
                            root: INFO
                            lavalink: INFO

                          request:
                            enabled: true
                            includeClientInfo: true
                            includeHeaders: false
                            includeQueryString: true
                            includePayload: true
                            maxPayloadLength: 10000


                          logback:
                            rollingpolicy:
                              max-file-size: 1GB
                              max-history: 30
                        """);

                    Console.WriteLine($"Lavalink's application.yml file has been generated at {PersistentStoragePaths.LavalinkYmlFile}");
                }

                if (!File.Exists(PersistentStoragePaths.ConfigurationFile))
                {
                    File.WriteAllText(PersistentStoragePaths.ConfigurationFile,
                        """
                        # 'Token' is necessary for the bot to run. Get it from 'https://discord.com/developers/docs/getting-started'
                        # 'DeafenWhenInVoiceChannel' has values 'True' and 'False'. Can also be left empty.
                        [Bot]
                        Token=
                        DeafenWhenInVoiceChannel=True

                        # 'Status' supports the following values: Online, Offline, DoNotDisturb, Idle, Invisible, Streaming.
                        [Status]
                        Type=Online

                        # 'Status' supports the following values: Playing, ListeningTo, Streaming, Competing, Watching, Custom.
                        # 'Name' is a description of your desired status. Can also be left empty.
                        [Activity]
                        Type=
                        Name=

                        # 'Volume' ranges from 0-2000. This sets the Lavalink server's internal volume, not the voice channel's.
                        # 'Password' must be the same as in the application.yml file.
                        # 'Endpoint' and 'ProxyEndpoint' must be the same as in the application.yml file and have the same form (Address:Port).
                        # 'ProxyEndpoint' must be left empty if you are not using a proxy.
                        [Lavalink]
                        Volume=70
                        Password=youshallnotpass
                        Endpoint=127.0.0.1:2333
                        ProxyEndpoint=
                        """);

                    Console.WriteLine($"A configuration file has been generated at {PersistentStoragePaths.BaseDirectory}." +
                                      "Enter a valid bot token before re-launching.");

                    Environment.Exit(0);
                }

                config.SetBasePath(PersistentStoragePaths.BaseDirectory);
                config.AddIniFile(PersistentStoragePaths.ConfigurationFile);
            })
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsoleFormatter<SimpleColoredConsoleFormatter, ConsoleFormatterOptions>();
                loggingBuilder.AddConsole(options => options.FormatterName = "simple-colored-console");
#if DEBUG
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
#else
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
#endif
            })
            .ConfigureServices((ctx, services) =>
            {
                services
                .AddSingleton(ctx.Configuration)
                .AddSingleton<Logger>()
                .AddSingleton<EmbedHandler>()
                .AddSingleton<InteractionHandler>()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.GuildVoiceStates,
#if DEBUG
                    LogLevel = LogSeverity.Debug,
#else
                    LogLevel = LogSeverity.Info,
#endif
                }))
                .AddSingleton((services) =>
                {
                    DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
                    return new InteractionService(client, new InteractionServiceConfig
                    {
                        DefaultRunMode = RunMode.Async,
#if DEBUG
                        LogLevel = LogSeverity.Debug,
#else
                        LogLevel = LogSeverity.Info,
#endif
                    });
                })
                .AddSingleton<MusicInteractionService>()
                .AddSingleton<PingInteractionService>()
                .AddLavaNode();
            })
            .UseConsoleLifetime()
            .Build();

        Host.Start();
        new Client().RunAsync().GetAwaiter().GetResult();
    }
}
