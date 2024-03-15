using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Suruga.Commands.Autocomplete;

#pragma warning disable CS1591
namespace Suruga.Commands;

[Group("server", "Server related commands.")]
public sealed class GuildCommands : InteractionModuleBase<SocketInteractionContext>
{
    [Group("channels", "Channel related commands.")]
    [RequireBotPermission(ChannelPermission.ManageChannels)]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    public sealed class Channels : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("text", "Creates a text channel.")]
        public async Task CreateTextChannelAsync
        (
            string name,
            string? topic = null,
            bool isNsfw = false,
            int slowmode = 0,
            int threadsSlowmode = 0,
            SocketCategoryChannel? relativePosition = null,
            int position = 0
        )
        {
            await Context.Guild.CreateTextChannelAsync(name, channel =>
            {
                channel.Topic = topic;
                channel.IsNsfw = isNsfw;
                channel.SlowModeInterval = slowmode;
                channel.DefaultSlowModeInterval = threadsSlowmode;
                channel.Position = position;
                channel.CategoryId = relativePosition?.Id;
            });

            await RespondAsync($"Created **{name}**");
        }

        [SlashCommand("voice", "Creates a voice channel.")]
        public async Task CreateVoiceChanelAsync
        (
            string name,
            int? maxUsersAllowed = null,
            bool isNsfw = false,
            int slowmode = 0,
            int threadsSlowmode = 0,
            [MinValue(8000), MaxValue(384000)] int bitrate = 64000,
            VideoQualityMode videoQuality = VideoQualityMode.Auto,
            [Autocomplete(typeof(VoiceRegionsAutocompleteHandler))]
            string? region = null,
            SocketCategoryChannel? relativePosition = null,
            int position = 0
        )
        {
            bitrate = Context.Guild.PremiumSubscriptionCount switch
            {
                0 when bitrate > 64000 => 64000,
                >= 2 and < 7 when bitrate > 128000 => 128000,
                >= 7 and < 14 when bitrate > 256000 => 256000,
                >= 14 when bitrate > 384000 => 384000,
                _ => (int)(Math.Round((double)bitrate / 1000) *
                           1000) // Divide bitrate by 1000 and round to nearest integer.
            };

            await Context.Guild.CreateVoiceChannelAsync(name, channel =>
            {
                channel.IsNsfw = isNsfw;
                channel.SlowModeInterval = slowmode;
                channel.DefaultSlowModeInterval = threadsSlowmode;
                channel.Bitrate = bitrate;
                channel.VideoQualityMode = videoQuality;
                channel.UserLimit = maxUsersAllowed;
                channel.RTCRegion = region;
                channel.CategoryId = relativePosition?.Id;
                channel.Position = position;
            });

            await RespondAsync($"Created **{name}**");
        }

        [SlashCommand("category", "Creates a category.")]
        public async Task CreateCategoryAsync(string name, int position = 0)
        {
            await Context.Guild.CreateCategoryChannelAsync("name", channel => channel.Position = position);
            await RespondAsync($"Created **{name}**");
        }

        [SlashCommand("delete", "Deletes a channel.")]
        public async Task DeleteChannelAsync(SocketGuildChannel channel, string? reason = null)
        {
            await channel.DeleteAsync(new RequestOptions { AuditLogReason = reason });
            await RespondAsync($"Deleted **{channel.Name}**.");
        }

        [SlashCommand("nsfw", "Marks or unmarks a text or voice channel as NSFW.")]
        public async Task MarkOrUnmarkTextChannelAsNsfwAsync([ChannelTypes(ChannelType.Text, ChannelType.Voice)] SocketGuildChannel channel)
        {
            SocketTextChannel specifiedChannel = (SocketTextChannel)channel;

            await specifiedChannel.ModifyAsync(x => x.IsNsfw = !specifiedChannel.IsNsfw);
            string nsfwState = specifiedChannel.IsNsfw ? "Marked" : "Unmarked";

            await RespondAsync($"{nsfwState} **{channel.Name}** as an NSFW channel.");
        }
    }

    [Group("moderation", "Moderation related commands.")]
    public sealed class Moderation : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("kick", "Kicks a member.")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUserAsync(SocketGuildUser user, string? reason = null)
        {
            if (user.Id == Context.User.Id || user.Id == Context.Guild.OwnerId)
            {
                return;
            }

            if (((SocketGuildUser)Context.User).Hierarchy - user.Hierarchy <= 0)
            {
                await RespondAsync($"I am unable to ban {user.Nickname} as they hold a higher role.");
                return;
            }

            await user.KickAsync(reason);
            await RespondAsync($"Kicked {user.Nickname}");
        }

        [SlashCommand("ban", "Bans a member.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync(SocketGuildUser user, [MinValue(0), MaxValue(7)] int pruneDays = 0, string? reason = null)
        {
            if (user.Id == Context.User.Id || user.Id == Context.Guild.OwnerId)
            {
                return;
            }
            
            if (((SocketGuildUser)Context.User).Hierarchy - user.Hierarchy <= 0)
            {
                await RespondAsync($"I am unable to ban {user.Nickname} as they hold a higher role.");
                return;
            }

            await user.BanAsync(pruneDays, reason);
            await RespondAsync($"Banned {user.Nickname}");
        }
    }
}
