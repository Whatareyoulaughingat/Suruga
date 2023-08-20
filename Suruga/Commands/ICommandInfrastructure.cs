using Discord.WebSocket;
using Suruga.Services;
using System.Threading.Tasks;

namespace Suruga.Commands;

internal interface ICommandInfrastructure
{
    public Task CreateAsync(DiscordSocketClient client);

    public Task HandleAsync(MusicService music, SocketSlashCommand command);
}
