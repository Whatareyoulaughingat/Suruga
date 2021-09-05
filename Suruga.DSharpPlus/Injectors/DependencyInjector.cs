using DSharpPlus;
using Lavalink4NET;
using Lavalink4NET.Cluster;
using Lavalink4NET.DSharpPlus;
using Lavalink4NET.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Suruga.DSharpPlus.Handlers.Application;
using Suruga.DSharpPlus.Services;

namespace Suruga.DSharpPlus.Injectors
{
    public class DependencyInjector
    {
        public static ServiceProvider Services { get; private protected set; }

        public static void Inject()
        {
            // Setup DI.
            Services = new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(new DiscordConfiguration
                {
                    Token = ConfigurationHandler.Data.Token,
                    Intents = DiscordIntents.AllUnprivileged,
                    LogTimestampFormat = "hh:mm:ss",
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
                            RestUri = "http://127.0.0.1:2333",
                            WebSocketUri = "ws://127.0.0.1:2333",
                            Password = "youshallnotpass",
                        },
                    },

                    StayOnline = true,
                })
                .AddSingleton<InactivityTrackingService>()
                .AddSingleton(new InactivityTrackingOptions
                {
                    DisconnectDelay = TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationHandler.Data.VoiceChannelDisconnectDelay)),
                })

                .AddSingleton<ChannelService>()
                .AddSingleton<MusicService>()
                .BuildServiceProvider();
        }
    }
}
