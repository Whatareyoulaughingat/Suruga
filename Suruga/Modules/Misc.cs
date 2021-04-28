using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Suruga.Services;

namespace Suruga.Modules
{
    public class Misc : BaseCommandModule
    {
        private readonly MiscService miscService;

        public Misc(MiscService miscservice)
            => miscService = miscservice;

        [Command("ping")]
        public async Task PingCommand(CommandContext commandContext)
            => await miscService.PingAsync(commandContext.Client, commandContext.Channel, commandContext.Member);
    }
}
