using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Suruga.Services;

namespace Suruga.Modules
{
    [Group("channel")]
    public class Channel : BaseCommandModule
    {
        private readonly ChannelService channelService;

        public Channel(ChannelService channelservice)
            => channelService = channelservice;

        [Command("create_textchannel")]
        [Description("Creates a text channel with the specified name.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task CreateTextChannelCommand(CommandContext commandContext, [RemainingText] string channelName)
            => await channelService.CreateTextChannelAsync(commandContext.Channel, commandContext.Member, commandContext.Guild, channelName);

        [Command("create_voicechannel")]
        [Description("Creates a voice channel with the speicifed name.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task CreateVoiceChannelCommand(CommandContext commandContext, [RemainingText] string channelName)
            => await channelService.CreateVoiceChannelAsync(commandContext.Channel, commandContext.Member, commandContext.Guild, channelName);

        [Command("create_categorychannel")]
        [Description("Creates a category channel with the specified name.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task CreateCategoryChannelCommand(CommandContext commandContext, [RemainingText] string channelName)
            => await channelService.CreateCategoryChannelAsync(commandContext.Channel, commandContext.Member, commandContext.Guild, channelName);

        [Command("bitrate")]
        [Description("Changes the bitrate of the specified voice channel.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task ChangeVoiceChannelBitrateCommand(CommandContext commandContext, int? bitrate, [RemainingText] DiscordChannel voiceChannel)
            => await channelService.ChangeVoiceChannelBitrateAsync(commandContext.Channel, commandContext.Member, bitrate, voiceChannel);

        [Command("rename")]
        [Description("Renames the specified channel.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task RenameChannelCommand(CommandContext commandContext, DiscordChannel channel, [RemainingText] string newName)
            => await channelService.RenameChannelAsync(commandContext.Channel, commandContext.Member, channel, newName);

        [Command("togglensfw")]
        [Description("Marks or unmarks the channel as NSFW.")]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task ToggleTextChannelAsNsfwCommand(CommandContext commandContext, DiscordChannel textChannel)
            => await channelService.ToggleTextChannelAsNsfwAsync(commandContext.Channel, commandContext.Member, textChannel);

        [Command("setparent")]
        [Description("Sets a channel inside a category.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task SetChildChannelToParentCommand(CommandContext commandContext, DiscordChannel childChannel, DiscordChannel parentChannel)
            => await channelService.SetChildChannelToParentAsync(commandContext.Channel, commandContext.Member, childChannel, parentChannel);

        [Command("slowmode")]
        [Description("Sets slowmode limit on the specified channel.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task SetTextChannelUserRateLimitCommand(CommandContext commandContext, DiscordChannel textChannel, int? userRateLimit)
            => await channelService.SetTextChannelUserRateLimitAsync(commandContext.Channel, commandContext.Member, textChannel, userRateLimit);

        [Command("setposition")]
        [Description("Sets a new position for the specified channel.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task SetChannelPositionCommand(CommandContext commandContext, int? position, [RemainingText] DiscordChannel channel)
            => await channelService.SetChannelPositionAsync(commandContext.Channel, commandContext.Member, position, channel);

        [Command("topic")]
        [Description("Sets a new topic for the specified channel.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageChannels | Permissions.SendMessages)]
        public async Task SetChannelTopicCommand(CommandContext commandContext, DiscordChannel textChannel, [RemainingText] string topic)
            => await channelService.SetChannelTopicAsync(commandContext.Channel, commandContext.Member, textChannel, topic);

        [Command("userlimit")]
        [Description("Sets a user limit for the specified voice channel.")]
        [RequirePermissions(Permissions.AccessChannels | Permissions.ManageMessages | Permissions.SendMessages)]
        public async Task SetVoiceChannelUserLimitCommand(CommandContext commandContext, int userLimit, [RemainingText] DiscordChannel voiceChannel)
            => await channelService.SetVoiceChannelUserLimitAsync(commandContext.Channel, commandContext.Member, voiceChannel, userLimit);
    }
}
