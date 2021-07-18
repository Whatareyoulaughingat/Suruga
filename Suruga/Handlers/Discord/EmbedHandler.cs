using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Suruga.Handlers.Application;
using Suruga.Services;

namespace Suruga.Handlers.Discord
{
    public static class EmbedHandler
    {
        /// <summary>
        /// Creates an embed that is used as output for <see cref="MusicService"/>.
        /// </summary>
        /// <param name="channel">The channel inside a guild where the command was executed.</param>
        /// <param name="member">The guild member who executed a command causing the <see cref="EmbedHandler"/> to fire.</param>
        /// <param name="description">The description of the embed.</param>
        /// <param name="imageUrl">A URL containing an image.</param>
        /// <returns>[<see cref="Task{DiscordMessage}"/>] An asynchronous operation that returns a generic Discord message.</returns>
        public static async Task<DiscordMessage> CreateEmbed(DiscordChannel channel, DiscordMember member, string description, Optional<string> imageUrl)
        {
            DiscordEmbedBuilder embed = new()
            {
                Color = new DiscordColor(ConfigurationHandler.Data.SuccessfulEmbedHexColor),
                Description = description,
                ImageUrl = imageUrl.Value,
                Timestamp = DateTimeOffset.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = member.AvatarUrl,
                    Text = member.DisplayName,
                },
            };

            embed.Build();
            return await channel.SendMessageAsync(embed);
        }

        /// <summary>
        /// Creates an embed that is used as an error message output for <see cref="MusicService"/>.
        /// </summary>
        /// <param name="channel">The channel inside a guild where the command was executed.</param>
        /// <param name="member">The guild member who executed a command causing the <see cref="EmbedHandler"/> to fire.</param>
        /// <param name="description">The description of the embed.</param>
        /// <returns>[<see cref="Task{DiscordMessage}"/>] An asynchronous operation that returns a generic Discord message.</returns>
        public static async Task<DiscordMessage> CreateErrorEmbed(DiscordChannel channel, DiscordMember member, string description)
        {
            DiscordEmbedBuilder errorEmbed = new()
            {
                Color = new DiscordColor(ConfigurationHandler.Data.UnsuccessfulEmbedHexColor),
                Description = description,
                Timestamp = DateTimeOffset.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = member.AvatarUrl,
                    Text = member.DisplayName,
                },
            };

            errorEmbed.Build();
            return await channel.SendMessageAsync(errorEmbed);
        }
    }
}
