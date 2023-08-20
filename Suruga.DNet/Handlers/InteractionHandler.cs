using Discord.Interactions;
using Discord.WebSocket;
using Suruga.DNet.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Suruga.DNet.Handlers;

public sealed class InteractionHandler
{
    private readonly DiscordSocketClient _client;

    private readonly InteractionService _commands;

    private readonly Logger _logger;

    public InteractionHandler(DiscordSocketClient client, InteractionService interactionService, Logger logger)
    {
        _client = client;
        _commands = interactionService;
        _logger = logger;
    }

    internal async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), Program.Host.Services);
        _client.InteractionCreated += HandleAsync;
    }

    private async Task HandleAsync(SocketInteraction interaction)
    {
        try
        {
            SocketInteractionContext ctx = new(_client, interaction);
            await _commands.ExecuteCommandAsync(ctx, Program.Host.Services);
        }
        catch (Exception ex)
        {
            /*
            if (interaction.Type is InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
            */

            _logger.LogError($"An interaction has failed. ({ex.Message})", ex);
        }
    }
}
