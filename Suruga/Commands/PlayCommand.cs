using Discord;
using Discord.WebSocket;
using Suruga.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suruga.Commands;

internal sealed class PlayCommand : ICommandInfrastructure
{
    public async Task CreateAsync(DiscordSocketClient client) => await client
        .CreateGlobalApplicationCommandAsync(new SlashCommandBuilder()
            .WithName("play")
            .WithDescription("Plays a song or playlist.")
            .AddOption(
                name: "url",
                type: ApplicationCommandOptionType.Channel,
                description: "A link or query that points to a song or playlist",
                isRequired: true)
            .AddOption(
                name: "voice-channel",
                type: ApplicationCommandOptionType.Channel,
                description: "A specific voice channel.",
                isRequired: false,
                channelTypes: new List<ChannelType>() { ChannelType.Voice })
            .Build());

    public async Task HandleAsync(MusicService music, SocketSlashCommand command)
        => await music.PlayAsync(command.User as SocketGuildUser, command.Channel, command.Data.Options.First().Value.ToString());
}
