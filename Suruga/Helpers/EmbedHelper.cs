using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suruga.Helpers;

public enum EmbedType
{
    Successful,
    Unsuccessful,
}

/// <summary>
/// A helper class for creating embeds as result messages.
/// </summary>
internal sealed class EmbedHelper
{
    private static readonly IConfiguration s_configuration = Program.Host.Services.GetRequiredService<IConfiguration>();

    internal static async Task Create(
        IGuildUser user,
        IMessageChannel channel,
        EmbedType type,
        string description = null,
        string title = null,
        List<EmbedFieldBuilder> fields = null,
        string imageUrl = null,
        string thumbnailUrl = null,
        uint? overridableColor = null)
    {
        EmbedBuilder builder = new()
        {
            Color = new Color(overridableColor ?? ToColor(type)),
            Title = title,
            Description = description,
            Fields = fields,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            Timestamp = DateTime.Now,
            Footer = new EmbedFooterBuilder
            {
                IconUrl = user.GetGuildAvatarUrl(),
                Text = user.DisplayName,
            },
        };

        await channel.SendMessageAsync(embeds: new Embed[] { builder.Build() });
    }

    private static uint ToColor(EmbedType type)
        => type is EmbedType.Successful
            ? uint.TryParse(s_configuration["HexColorOnSuccessfulCommand"], out uint successfulEmbedHexColor) ? successfulEmbedHexColor : 0x3CB043
            : uint.TryParse(s_configuration["HexColorOnUnsuccessfulCommand"], out uint unsuccessfulEmbedHexColor) ? unsuccessfulEmbedHexColor : 0xD0312D;
}
