using Discord;
using Discord.WebSocket;
using Suruga.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suruga.Commands;

internal sealed class LeaveCommand : ICommandInfrastructure
{
    public async Task CreateAsync(DiscordSocketClient client)
    {
        await client.CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
            .WithName("leave")
            .WithDescription("Leaves the voice channel you are currently in.")
            .AddOption(
                name: "voice-channel",
                type: ApplicationCommandOptionType.Channel,
                description: "A specific voice channel.",
                isRequired: false,
                channelTypes: new List<ChannelType>() { ChannelType.Voice })
            .Build());
    }

    public async Task HandleAsync(MusicService music, SocketSlashCommand command)
        => await music.LeaveAsync(command.User as SocketGuildUser, command.Channel);
}
