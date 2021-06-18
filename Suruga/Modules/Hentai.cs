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

        [Command("rule34")]
        [Description("Posts an image from Rule34 that is related to the specified tags.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task Rule34Command(CommandContext commandContext, [RemainingText] string tags)
            => await nsfwService.Rule34Async(commandContext.Channel, commandContext.Member, tags);

        [Command("gelbooru")]
        [Description("Posts an image from Gelbooru that is related to the specified tags.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task GelbooruCommand(CommandContext commandContext, [RemainingText] string tags)
            => await nsfwService.GelbooruAsync(commandContext.Channel, commandContext.Member, tags);

        [Command("ahegao")]
        [Description("Posts an image that is related to ahegao stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task AhegaoCommand(CommandContext commandContext)
            => await nsfwService.AhegaoAsync(commandContext.Channel, commandContext.Member);

        [Command("anal")]
        [Description("Posts an image that is related to anal stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task AnalCommand(CommandContext commandContext)
            => await nsfwService.AnalAsync(commandContext.Channel, commandContext.Member);

        [Command("analgif")]
        [Description("Posts an image that is related to anal gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task AnalGifCommand(CommandContext commandContext)
            => await nsfwService.AnalGifAsync(commandContext.Channel, commandContext.Member);

        [Command("bdsm")]
        [Description("Posts an image that is related to bdsm stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BdsmCommand(CommandContext commandContext)
            => await nsfwService.BdsmAsync(commandContext.Channel, commandContext.Member);

        [Command("blowjob")]
        [Description("Posts an image that is related blowjob stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BlowjobCommand(CommandContext commandContext)
            => await nsfwService.BlowjobAsync(commandContext.Channel, commandContext.Member);

        [Command("blowjobgif")]
        [Description("Posts an image that is related to blowjob gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BlowjobGifCommand(CommandContext commandContext)
            => await nsfwService.BlowjobGifAsync(commandContext.Channel, commandContext.Member);

        [Command("boobs")]
        [Description("Posts an image marked with the boobs tag.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BoobsCommand(CommandContext commandContext)
            => await nsfwService.BoobsAsync(commandContext.Channel, commandContext.Member);

        [Command("boobsgif")]
        [Description("Posts an image that is related to boobs gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task BoobsGifCommand(CommandContext commandContext)
            => await nsfwService.BoobsGifAsync(commandContext.Channel, commandContext.Member);

        [Command("cum")]
        [Description("Posts an image that is related to cum stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task CumCommand(CommandContext commandContext)
            => await nsfwService.CumAsync(commandContext.Channel, commandContext.Member);

        [Command("cumgif")]
        [Description("Posts an image that is related to cum gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task CumGifCommand(CommandContext commandContext)
            => await nsfwService.CumGifAsync(commandContext.Channel, commandContext.Member);

        [Command("feet")]
        [Description("Posts an image that is related to feet stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task FeetCommand(CommandContext commandContext)
            => await nsfwService.FeetAsync(commandContext.Channel, commandContext.Member);

        [Command("feetgif")]
        [Description("Posts an image that is related to feet gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task FeetGifCommand(CommandContext commandContext)
            => await nsfwService.FeetGifAsync(commandContext.Channel, commandContext.Member);

        [Command("fox")]
        [Description("Posts an image that is related to fox stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task FoxCommand(CommandContext commandContext)
            => await nsfwService.FoxAsync(commandContext.Channel, commandContext.Member);

        [Command("futanari")]
        [Description("Posts an image that is related futanari stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task FutanariCommand(CommandContext commandContext)
            => await nsfwService.FutanariAsync(commandContext.Channel, commandContext.Member);

        [Command("hentai")]
        [Description("Posts an image that is related to hentai stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task HentaiCommand(CommandContext commandContext)
            => await nsfwService.HentaiAsync(commandContext.Channel, commandContext.Member);

        [Command("hentaigif")]
        [Description("Posts an image that is related to hentai gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task HentaiGifCommand(CommandContext commandContext)
            => await nsfwService.HentaiGifAsync(commandContext.Channel, commandContext.Member);

        [Command("lewd")]
        [Description("Posts an image that is related to lewd stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task LewdCommand(CommandContext commandContext)
            => await nsfwService.LewdAsync(commandContext.Channel, commandContext.Member);

        [Command("neko")]
        [Description("Posts an image that is related to neko stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task NekoCommand(CommandContext commandContext)
            => await nsfwService.NekoAsync(commandContext.Channel, commandContext.Member);

        [Command("nekogif")]
        [Description("Posts an image that is related to neko gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task NekoGifCommand(CommandContext commandContext)
            => await nsfwService.NekoGifAsync(commandContext.Channel, commandContext.Member);

        [Command("pantyhose")]
        [Description("Posts an image that is related to pantyhose stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task PantyhoseCommand(CommandContext commandContext)
            => await nsfwService.PantyhoseAsync(commandContext.Channel, commandContext.Member);

        [Command("pussy")]
        [Description("Posts an image that is related to pussy stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task PussyCommand(CommandContext commandContext)
            => await nsfwService.PussyAsync(commandContext.Channel, commandContext.Member);

        [Command("pussygif")]
        [Description("Posts an image that is related to pussy gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task PussyGifCommand(CommandContext commandContext)
            => await nsfwService.PussyGifAsync(commandContext.Channel, commandContext.Member);

        [Command("smallboobs")]
        [Description("Posts an image that is related to small boob stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task SmallBoobsCommand(CommandContext commandContext)
            => await nsfwService.SmallBoobsAsync(commandContext.Channel, commandContext.Member);

        [Command("spank")]
        [Description("Posts an image that is related to spank stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task SpankGifCommand(CommandContext commandContext)
            => await nsfwService.SpankGifAsync(commandContext.Channel, commandContext.Member);

        [Command("wallpaper")]
        [Description("Posts an image that is related to wallpaper stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task WallpaperCommand(CommandContext commandContext)
            => await nsfwService.WallpaperAsync(commandContext.Channel, commandContext.Member);

        [Command("yiff")]
        [Description("Posts an image that is related to yiff stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task YiffCommand(CommandContext commandContext)
            => await nsfwService.YiffAsync(commandContext.Channel, commandContext.Member);

        [Command("yiffgif")]
        [Description("Posts an image that is related to yiff gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task YiffGifCommand(CommandContext commandContext)
            => await nsfwService.YiffGifAsync(commandContext.Channel, commandContext.Member);

        [Command("yuri")]
        [Description("Posts an image marked with the yuri tag.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task YuriCommand(CommandContext commandContext)
            => await nsfwService.YuriAsync(commandContext.Channel, commandContext.Member);

        [Command("yurigif")]
        [Description("Posts an image that is related to yuri gif stuff.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages)]
        public async Task YuriGifCommand(CommandContext commandContext)
            => await nsfwService.YuriGifAsync(commandContext.Channel, commandContext.Member);
    }
}