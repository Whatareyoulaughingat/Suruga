using Discord.Interactions;
using Suruga.DNet.Services;
using System.Threading.Tasks;

namespace Suruga.DNet.Modules;

public class PingInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly PingInteractionService _ping;

    public PingInteractionModule(PingInteractionService ping)
        => _ping = ping;

    [SlashCommand("ping", "Checks if the bot is online.")]
    public async Task PingAsync()
        => await _ping.PingAsync(Context.Interaction);
}
