using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Suruga.Options;

namespace Suruga.Services;

internal sealed class LavalinkConnectionService
(
    IOptions<LavalinkConnectionServiceOptions> options,
    DiscordSocketClient client,
    IAudioService audio,
    ILogger<DiscordClientService> logger
) : DiscordClientService(client, logger)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);

        if (!options.Value.EnableMusicCommands)
        {
            return;
        }

        await audio.StartAsync(stoppingToken);
    }
}

