using System.Threading.Tasks;
using BooruSharp.Booru;
using BooruSharp.Search.Post;
using DSharpPlus.Entities;
using NekosSharp;
using Suruga.Handlers;

namespace Suruga.Services
{
    public class HentaiService
    {
        private readonly NsfwEndpoints_v3 nsfwEndpoints = new(new NekoClient("Suruga"));
        private readonly Rule34 rule34 = new();
        private Request nsfwRequest;

        public async Task<DiscordMessage> Rule34PostAsync(DiscordChannel channel, DiscordMember member, string tags)
        {
            SearchResult relatedImage = await rule34.GetRandomPostAsync(tags);
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, relatedImage.FileUrl.AbsoluteUri);
        }

        public async Task<DiscordMessage> AhegaoAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Ahegao();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> AnalAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Anal();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> AnalGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.AnalGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> BdsmAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Bdsm();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> BlowjobAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Blowjob();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> BlowjobGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.BlowjobGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> BoobsAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Boobs();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> BoobsGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.BoobsGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> CumAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Cum();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> CumGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.CumGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> FeetAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Feet();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> FeetGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.FeetGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> FoxAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Fox();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> FutanariAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Futanari();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> HentaiAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Hentai();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> HentaiGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.HentaiGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> LewdAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Lewd();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> NekoAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Neko();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> NekoGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.NekoGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> PantyhoseAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Pantyhose();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> PussyAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Pussy();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> PussyGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.PussyGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> SmallBoobsAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.SmallBoobs();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> SpankGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.SpankGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> WallpaperAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Wallpaper();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> YiffAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Yiff();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> YiffGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.YiffGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> YuriAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.Yuri();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }

        public async Task<DiscordMessage> YuriGifAsync(DiscordChannel channel, DiscordMember member)
        {
            nsfwRequest = await nsfwEndpoints.YuriGif();
            return await EmbedHandler.CreateEmbed(channel, member, string.Empty, nsfwRequest.ImageUrl);
        }
    }
}
