using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Enums;

namespace Suruga.Interactions;

public class PingInteraction : ApplicationCommandsModule
{
    [SlashCommand("ping", "Pings the bot.")]
    public async Task PingAsync(InteractionContext ctx)
        => await ctx.CreateResponseAsync(InteractionResponseType.Pong);
}
