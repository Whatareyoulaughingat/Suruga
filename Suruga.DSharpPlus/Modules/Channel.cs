using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Suruga.DSharpPlus.Services;

namespace Suruga.DSharpPlus.Modules;

public class Channel : BaseCommandModule
{
    private readonly ChannelService channelService;

    public Channel(ChannelService _channelService)
        => channelService = _channelService;

    [Command("create_textchannel")]
    [Description("Creates a text channel with the specified arguments.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task CreateTextChannelCommand(CommandContext commandContext, string name, bool nsfw = false, DiscordChannel parentChannel = null)
        => await channelService.CreateTextChannelAsync(commandContext.Guild, commandContext.Channel, commandContext.Member, name, nsfw, parentChannel);

    [Command("delete_textchannel")]
    [Description("Deleted a text channel.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
    public async Task DeleteTextChannelCommand(CommandContext commandContext, DiscordChannel channel)
        => await channelService.DeleteTextChannelAsync(commandContext.Channel, commandContext.Member, channel);

}
