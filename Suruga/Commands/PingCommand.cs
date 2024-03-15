using Discord.Interactions;

#pragma warning disable CS1591
namespace Suruga.Commands;

public sealed class PingCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Pings the bot.")]
    public async Task PingAsync()
        => await RespondAsync("Pong!");
}
