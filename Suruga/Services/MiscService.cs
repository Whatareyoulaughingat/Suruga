using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Suruga.Handlers;

namespace Suruga.Services
{
    public class MiscService
    {
        public async Task<DiscordMessage> PingAsync(DiscordClient client, DiscordChannel channel, DiscordMember member)
        {
            return await EmbedHandler.CreateEmbed(channel, member, $"Latency: {client.Ping}");
        }
    }
}
