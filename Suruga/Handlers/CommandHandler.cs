using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Suruga.Handlers;

internal sealed class CommandHandler
{
    private readonly IConfiguration _configuration;

    private readonly DiscordSocketClient _client;

    private readonly CommandService _commands;

    public CommandHandler(
        IConfiguration configuration,
        DiscordSocketClient client,
        CommandService commands)
    {
        _configuration = configuration;
        _client = client;
        _commands = commands;
    }

    internal async Task InitializeAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), Program.Host.Services);
    }

    private async Task HandleCommandAsync(SocketMessage message)
    {
        // Don't process the command if it was a system message.
        if (message is not SocketUserMessage userMessage)
        {
            return;
        }

        // Create a number to track where the prefix ends and the command begins.
        int argPos = 0;

        if (!userMessage.HasCharPrefix(_configuration["CommandPrefix"].First(), ref argPos) || userMessage.Author.IsBot)
        {
            return;
        }

        SocketCommandContext context = new(_client, userMessage);
        await _commands.ExecuteAsync(context, argPos, Program.Host.Services);
    }
}
