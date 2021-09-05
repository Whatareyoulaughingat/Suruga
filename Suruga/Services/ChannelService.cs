using DSharpPlus.Entities;
using Suruga.Injectors;

namespace Suruga.Services;

public class ChannelService
{
    public async Task<DiscordMessage> CreateTextChannelAsync(DiscordGuild guild, DiscordChannel channel, DiscordMember member, string name, bool nsfw = false, DiscordChannel parentChannel = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return await Embeds.CreateErrorEmbed(channel, member, "Cannot create an empty or whitespace only text channel.");
        }

        await guild.CreateTextChannelAsync(name, parentChannel, string.Empty, null, nsfw);
        return await Embeds.CreateEmbed(channel, member, $"Successfully created **{name}**");
    }

    public async Task<DiscordMessage> DeleteTextChannelAsync(DiscordGuild guild, DiscordChannel commandExecutedChannel, DiscordMember member, DiscordChannel toBeDeletedChannel)
    {
        string deletedChannelName = toBeDeletedChannel.Name;

        await toBeDeletedChannel.DeleteAsync();
        return await Embeds.CreateEmbed(commandExecutedChannel, member, $"Successfully deleted {deletedChannelName}");
    }
}
