using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Suruga.Handlers.Discord;

namespace Suruga.Services
{
    public class ChannelService
    {
        public async Task<DiscordMessage> CreateTextChannelAsync(DiscordChannel channel, DiscordMember member, DiscordGuild guild, string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "A text channel must always have a name.");
            }

            await guild.CreateTextChannelAsync(channelName);
            return await EmbedHandler.CreateEmbed(channel, member, $"**{channelName}** has been created successfully.");
        }

        public async Task<DiscordMessage> CreateVoiceChannelAsync(DiscordChannel channel, DiscordMember member, DiscordGuild guild, string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "A voice channel must always have a name.");
            }

            await guild.CreateVoiceChannelAsync(channelName);
            return await EmbedHandler.CreateEmbed(channel, member, $"**{channelName}** has been created successfully.");
        }

        public async Task<DiscordMessage> CreateCategoryChannelAsync(DiscordChannel channel, DiscordMember member, DiscordGuild guild, string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "A category channel must always have a name.");
            }

            await guild.CreateChannelCategoryAsync(channelName);
            return await EmbedHandler.CreateEmbed(channel, member, $"**{channelName}** has been created successfully.");
        }

        public async Task<DiscordMessage> ChangeVoiceChannelBitrateAsync(DiscordChannel channel, DiscordMember member, int? bitrate, DiscordChannel voiceChannel)
        {
            if (voiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified voice channel doesn't exist.");
            }

            if (voiceChannel.Type != ChannelType.Voice)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified channel isn't a voice channel.");
            }

            if (bitrate > 96000 || bitrate < 8000 || bitrate == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The bitrate value must be between 8000 and 96000.");
            }

            await voiceChannel.ModifyAsync(x => x.Bitrate = bitrate);
            return await EmbedHandler.CreateEmbed(channel, member, $"Changed the bitrate of {voiceChannel.Name} successfully.");
        }

        public async Task<DiscordMessage> RenameChannelAsync(DiscordChannel channel, DiscordMember member, DiscordChannel toBeRenamedChannel, string newChannelName)
        {
            if (toBeRenamedChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified channel doesn't exist.");
            }

            if (string.IsNullOrWhiteSpace(newChannelName))
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "A new name for a channel cannot be empty or contain whitespace characters.");
            }

            await toBeRenamedChannel.ModifyAsync(x => x.Name = newChannelName);
            return await EmbedHandler.CreateEmbed(channel, member, $"Renamed {toBeRenamedChannel.Name} successfully.");
        }

        public async Task<DiscordMessage> ToggleTextChannelAsNsfwAsync(DiscordChannel channel, DiscordMember member, DiscordChannel textChannel)
        {
            if (textChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified text channel doesn't exist.");
            }

            if (textChannel.Type != ChannelType.Text)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified channel isn't a text channel.");
            }

            if (textChannel.IsNSFW == false)
            {
                await textChannel.ModifyAsync(x => x.Nsfw = true);
                return await EmbedHandler.CreateEmbed(channel, member, $"**{textChannel.Name}** has been marked as NSFW successfully.");
            }
            else
            {
                await textChannel.ModifyAsync(x => x.Nsfw = false);
                return await EmbedHandler.CreateEmbed(channel, member, $"**{channel.Name}** has been unmarked as NSFW successfully.");
            }
        }

        public async Task<DiscordMessage> SetChildChannelToParentAsync(DiscordChannel channel, DiscordMember member, DiscordChannel childChannel, DiscordChannel parentChannel)
        {
            if (childChannel == null || parentChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The child channel (The channel that is going to be positioned inside the parent channel) or the parent channel is null");
            }

            if (parentChannel.Type != ChannelType.Category)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The parent channel must be a category channel.");
            }

            await childChannel.ModifyAsync(x => x.Parent = parentChannel);
            return await EmbedHandler.CreateEmbed(channel, member, $"**{childChannel.Name}** has been positioned inside **{parentChannel.Name}** successfully.");
        }

        public async Task<DiscordMessage> SetTextChannelUserRateLimitAsync(DiscordChannel channel, DiscordMember member, DiscordChannel textChannel, int? userRateLimit)
        {
            if (textChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified channel doesn't exist.");
            }

            if (userRateLimit > 0 || userRateLimit < 21600 || userRateLimit == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The slowmode value must be between 0 and 21600 seconds.");
            }

            await textChannel.ModifyAsync(x => x.PerUserRateLimit = userRateLimit);
            return await EmbedHandler.CreateEmbed(channel, member, $"Set **{textChannel.Name}'s** slowmode to {userRateLimit}");
        }

        public async Task<DiscordMessage> SetChannelPositionAsync(DiscordChannel channel, DiscordMember member, int? position, DiscordChannel toBePositionedChannel)
        {
            if (toBePositionedChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified channel doesn't exist.");
            }

            /*
            if (position < 0)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The position of a channel cannot be less than 0.");
            }
            */

            await toBePositionedChannel.ModifyAsync(x => x.Position = position);
            return await EmbedHandler.CreateEmbed(channel, member, $"Changed the position of **{toBePositionedChannel.Name}** successfully.");
        }

        public async Task<DiscordMessage> SetChannelTopicAsync(DiscordChannel channel, DiscordMember member, DiscordChannel textChannel, string topic)
        {
            if (textChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified text channel doesn't exist.");
            }

            if (textChannel.Type != ChannelType.Text)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified channel isn't a text channel.");
            }

            if (string.IsNullOrWhiteSpace(topic))
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "A topic cannot be empty or contain whitespace characters.");
            }

            await textChannel.ModifyAsync(x => x.Topic = topic);
            return await EmbedHandler.CreateEmbed(channel, member, $"Changed the topic of **{textChannel.Name}** successfully.");
        }

        public async Task<DiscordMessage> SetVoiceChannelUserLimitAsync(DiscordChannel channel, DiscordMember member, DiscordChannel voiceChannel, int userLimit)
        {
            if (voiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified voice channel doesn't exist.");
            }

            if (voiceChannel.Type != ChannelType.Voice)
            {
                return await EmbedHandler.CreateErrorEmbed(channel, member, "The specified channel isn't a voice channel.");
            }

            await voiceChannel.ModifyAsync(x => x.Userlimit = userLimit);
            return await EmbedHandler.CreateEmbed(channel, member, $"Changed the user limit of **{voiceChannel.Name}** successfully.");
        }
    }
}
