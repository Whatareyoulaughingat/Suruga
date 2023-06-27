using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Suruga.Handlers;
using Suruga.Logging;
using System;
using System.Threading.Tasks;
using Victoria.Node;

namespace Suruga.Client;

internal sealed class SurugaClient
{
    private IConfiguration _configuration;

    private ProgramLogger _programLogger;

    private ClientLogger _clientLogger;

    private DiscordSocketClient _client;

    private CommandHandler _commandHandler;

    private InteractionHandler _interactionHandler;

    private LavaNode _lavaNode;

    internal async Task RunAsync()
    {
        _configuration = Program.Host.Services.GetRequiredService<IConfiguration>();
        _programLogger = Program.Host.Services.GetRequiredService<ProgramLogger>();
        _clientLogger = Program.Host.Services.GetRequiredService<ClientLogger>();
        _client = Program.Host.Services.GetRequiredService<DiscordSocketClient>();
        _commandHandler = Program.Host.Services.GetRequiredService<CommandHandler>();
        _interactionHandler = Program.Host.Services.GetRequiredService<InteractionHandler>();
        _lavaNode = Program.Host.Services.GetRequiredService<LavaNode>();

        _client.Log += OnLog;
        _client.Ready += OnReadyAsync;

        try
        {
            await _client.LoginAsync(TokenType.Bot, _configuration["Token"]);
            await _client.StartAsync();
        }
        catch (Exception ex)
        {
            _programLogger.LogCritical("Failed to start.", ex);
        }

        await Task.Delay(-1);
    }

    private async Task OnReadyAsync()
    {
        await _commandHandler.InitializeAsync();
        await _interactionHandler.InitializeAsync();

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

        await _lavaNode.ConnectAsync();
    }

    private Task OnLog(LogMessage message)
    {
        _clientLogger.Log(_clientLogger.ToLogLevel(message.Severity), message.Message, message.Exception);
        return Task.CompletedTask;
    }
}
