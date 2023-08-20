using Discord.WebSocket;
using Suruga.DNet.Handlers;
using Suruga.DNet.Logging;
using System;
using System.Threading.Tasks;

namespace Suruga.DNet.Services;

public sealed class PingInteractionService
{
    private readonly Logger _logger;

    private readonly EmbedHandler _embed;

    public PingInteractionService(Logger programLogger, EmbedHandler embed)
    {
        _logger = programLogger;
        _embed = embed;
    }

    internal async Task PingAsync(SocketInteraction interaction)
    {
        try
        {
            await _embed.CreateSuccessfulResponse(interaction, description: "Pong!");
        }
        catch (Exception ex)
        {
            await _embed.CreateUnsuccessfulResponse(interaction, description: "An internal error has occured.");
            _logger.LogError("Failed to return ping response after a user request.", ex);
        }
    }
}
