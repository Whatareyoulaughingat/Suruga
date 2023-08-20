using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Suruga.DNet.Handlers;
using Suruga.DNet.Logging;
using Suruga.DNet.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Node;

namespace Suruga.DNet;

internal sealed class Client
{
    private readonly IConfiguration _configuration = Program.Host.Services.GetRequiredService<IConfiguration>();

    private readonly Logger _logger = Program.Host.Services.GetRequiredService<Logger>();

    private readonly DiscordSocketClient _client = Program.Host.Services.GetRequiredService<DiscordSocketClient>();

    private readonly InteractionHandler _interactionHandler = Program.Host.Services.GetRequiredService<InteractionHandler>();

    private readonly InteractionService _interactionService = Program.Host.Services.GetRequiredService<InteractionService>();

    private readonly LavaNode _lavaNode = Program.Host.Services.GetRequiredService<LavaNode>();

    internal async Task RunAsync()
    {
        await _interactionHandler.InitializeAsync();

        _interactionService.Log += OnLog;
        _client.Log += OnLog;
        _client.Ready += OnReadyAsync;

        try
        {
            await _client.LoginAsync(TokenType.Bot, _configuration["Token"]);
            await _client.StartAsync();
        }
        catch (Exception ex)
        {
            _interactionService.Log -= OnLog;
            _client.Log -= OnLog;
            _client.Ready -= OnReadyAsync;

            await _client.StopAsync();
            await _client.LogoutAsync();
            await _client.DisposeAsync();

            _logger.LogCritical("Failed to start.", ex);
        }

        await Task.Delay(Timeout.Infinite);
    }

    private async Task OnReadyAsync()
    {
        if (Enum.TryParse(_configuration["Status"], out UserStatus status))
        {
            await _client.SetStatusAsync(status);
        }

        if (Enum.TryParse(_configuration["ActivityType"], out ActivityType activity))
        {
            await _client.SetActivityAsync(new Game(
                name: _configuration["ActivityName"],
                type: activity,
                flags: ActivityProperties.None,
                details: _configuration["ActivityDescription"]));
        }

#if DEBUG
        await _interactionService.RegisterCommandsToGuildAsync(1122190575232352306);
#else
        await _interactionService.RegisterCommandsGloballyAsync();
#endif
        await _lavaNode.ConnectAsync();
    }

    private Task OnLog(LogMessage message)
    {
        _logger.Log(message.Severity.ToLogLevel(), message.Message, message.Exception);
        return Task.CompletedTask;
    }
}
