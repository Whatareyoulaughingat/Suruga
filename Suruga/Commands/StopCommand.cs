using Discord;
using Discord.WebSocket;
using Suruga.Services;
using System.Threading.Tasks;

namespace Suruga.Commands;

internal sealed class StopCommand : ICommandInfrastructure
{
    public async Task CreateAsync(DiscordSocketClient client) => await client
        .CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
            .WithName("stop")
            .WithDescription("Stops a song.")
            .Build());

    public async Task HandleAsync(MusicService music, SocketSlashCommand command)
        => await music.StopAsync(command.User as SocketGuildUser, command.Channel);
}
