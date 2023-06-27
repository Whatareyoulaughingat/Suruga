using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Suruga.Handlers;

internal sealed class InteractionHandler
{
    private ImmutableDictionary<string, RestGlobalCommand> _interactions;

    private readonly DiscordSocketClient _client;

    private readonly InteractionService _interactionService;

    public InteractionHandler(DiscordSocketClient client, InteractionService interactionService)
    {
        _client = client;
        _interactionService = interactionService;
    }

    internal async Task InitializeAsync()
    {
        // _client.SlashCommandExecuted += OnSlashCommandExecuted;

        IEnumerable<ModuleInfo> modules = await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), Program.Host.Services);
        IReadOnlyCollection<RestGlobalCommand> interactions = await _interactionService.AddModulesGloballyAsync(modules: modules.ToArray());

        _interactions = interactions.ToImmutableDictionary(key => key.Name);
    }

    /*
    private async Task OnSlashCommandExecuted(SocketSlashCommand command)
    {
        switch (command.CommandName)
        {
            case "leave":
                await 
        }
    }
    */
}
