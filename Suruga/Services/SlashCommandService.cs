using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Suruga.Commands;
using Suruga.Options;

namespace Suruga.Services;

internal sealed class SlashCommandService
(
    IOptions<LargeLanguageModelCommandsOptions> llmOptions,
    IOptions<LavalinkConnectionServiceOptions> lavalinkOptions,
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    ILogger<DiscordClientService> logger
) : DiscordClientService(client, logger)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (llmOptions.Value.Model is not null)
        {
            await interactions.AddModuleAsync<LargeLanguageModelCommands>(services);
        }
        
        if (lavalinkOptions.Value.EnableMusicCommands)
        {
            await interactions.AddModuleAsync<MusicCommands>(services);
        }

        await interactions.AddModuleAsync<GuildCommands>(services);
        await interactions.AddModuleAsync<ImageCommands>(services);
        await interactions.AddModuleAsync<PingCommand>(services);

        Client.InteractionCreated += OnInteractionCreated;

        await Client.WaitForReadyAsync(stoppingToken);
        await interactions.RegisterCommandsGloballyAsync();

        /*
#if DEBUG
        await interactions.RegisterCommandsToGuildAsync(guildId: 1159852457548058744);
#else
        await interactions.RegisterCommandsGloballyAsync();
#endif
*/
    }

    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        SocketInteractionContext context = new(Client, interaction);
        IResult result = await interactions.ExecuteCommandAsync(context, services);

        if (!result.IsSuccess)
        {
            Logger.LogError("Failed to execute a command with error: {Reason}", result.ErrorReason);
        }
    }
}
