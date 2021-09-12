using DSharpPlus.Entities;
using Suruga.Handlers.Application;

namespace Suruga.Services
{
    public class CommandPrefixService
    {
        public async Task ChangePrefix(DiscordGuild guild, string prefix)
        {
            ConfigurationHandler.Data.CommandPrefixes.Add(guild.Id, prefix);
        }
    }
}
