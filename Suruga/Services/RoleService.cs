using System.Threading.Tasks;
using DSharpPlus.Entities;
using Suruga.Handlers.Discord;

namespace Suruga.Services
{
    public class RoleService
    {
        public async Task<DiscordMessage> CreateRoleAsync(DiscordChannel channel, DiscordMember member, DiscordGuild guild, string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "You must specify a name or at least a whitespace character.");
            }

            await guild.CreateRoleAsync(roleName);
            return await EmbedHandler.CreateEmbed(channel, member, $"Successfully created **{roleName}**");
        }
    }
}
