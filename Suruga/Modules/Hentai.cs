using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Suruga.Services;

namespace Suruga.Modules
{
    [Group("nsfw")]
    public class Hentai : BaseCommandModule
    {
        private readonly HentaiService nsfwService;

        public Hentai(HentaiService nsfwservice)
            => nsfwService = nsfwservice;

        [Command("ahegao")]
        [Description("Posts an image that is related to ahegao stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task AhegaoCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.AhegaoAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("anal")]
        [Description("Posts an image that is related to anal stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task AnalCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.AnalAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("analgif")]
        [Description("Posts an image that is related to anal gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task AnalGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.AnalGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("bdsm")]
        [Description("Posts an image that is related to bdsm stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BdsmCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.BdsmAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("blowjob")]
        [Description("Posts an image that is related blowjob stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BlowjobCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.BlowjobAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("blowjobgif")]
        [Description("Posts an image that is related to blowjob gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BlowjobGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.BlowjobGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("boobs")]
        [Description("Posts an image marked with the boobs tag.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BoobsCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.BoobsAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("boobsgif")]
        [Description("Posts an image that is related to boobs gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BoobsGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.BoobsGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("cum")]
        [Description("Posts an image that is related to cum stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task CumCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.CumAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("cumgif")]
        [Description("Posts an image that is related to cum gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task CumGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.CumGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("feet")]
        [Description("Posts an image that is related to feet stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task FeetCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.FeetAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("feetgif")]
        [Description("Posts an image that is related to feet gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task FeetGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.FeetGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("fox")]
        [Description("Posts an image that is related to fox stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task FoxCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.FoxAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("futanari")]
        [Description("Posts an image that is related futanari stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task FutanariCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.FutanariAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("hentai")]
        [Description("Posts an image that is related to hentai stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task HentaiCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.HentaiAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("hentaigif")]
        [Description("Posts an image that is related to hentai gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task HentaiGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.HentaiGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("lewd")]
        [Description("Posts an image that is related to lewd stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task LewdCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.LewdAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("neko")]
        [Description("Posts an image that is related to neko stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task NekoCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.NekoAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("nekogif")]
        [Description("Posts an image that is related to neko gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task NekoGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.NekoGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("pantyhose")]
        [Description("Posts an image that is related to pantyhose stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task PantyhoseCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.PantyhoseAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("pussy")]
        [Description("Posts an image that is related to pussy stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task PussyCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.PussyAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("pussygif")]
        [Description("Posts an image that is related to pussy gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task PussyGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.PussyGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("smallboobs")]
        [Description("Posts an image that is related to small boob stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task SmallBoobsCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.SmallBoobsAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("spank")]
        [Description("Posts an image that is related to spank stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task SpankGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.SpankGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("wallpaper")]
        [Description("Posts an image that is related to wallpaper stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task WallpaperCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.WallpaperAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("yiff")]
        [Description("Posts an image that is related to yiff stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task YiffCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.YiffAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("yiffgif")]
        [Description("Posts an image that is related to yiff gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task YiffGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.YiffGifAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("yuri")]
        [Description("Posts an image marked with the yuri tag.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task YuriCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.YuriAsync(commandContext.Channel, commandContext.Member, imageCount);

        [Command("yurigif")]
        [Description("Posts an image that is related to yuri gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task YuriGifCommand(CommandContext commandContext, int imageCount = 1)
            => await nsfwService.YuriGifAsync(commandContext.Channel, commandContext.Member, imageCount);
    }
}