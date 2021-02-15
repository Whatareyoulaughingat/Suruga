using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Suruga.Services;

namespace Suruga.Modules
{
    public class Music : BaseCommandModule
    {
        private readonly MusicService musicService;

        public Music(MusicService musicservice)
            => musicService = musicservice;

        [Command("play")]
        [Description("Plays the specified track.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
        public async Task PlayCommand(CommandContext commandContext, [RemainingText] string url)
            => await musicService.PlayAsync(commandContext.Client, commandContext.Channel, commandContext.Member, url);

        [Command("leave")]
        [Description("Leaves the voice channel.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
        public async Task LeaveCommand(CommandContext commandContext)
            => await musicService.LeaveAsync(commandContext.Client, commandContext.Channel, commandContext.Member);

        [Command("stop")]
        [Description("Stops playing the current track.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
        public async Task StopCommand(CommandContext commandContext)
            => await musicService.StopAsync(commandContext.Client, commandContext.Channel, commandContext.Member);

        [Command("pause")]
        [Description("Pauses the current track.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
        public async Task PauseCommand(CommandContext commandContext)
            => await musicService.PauseAsync(commandContext.Client, commandContext.Channel, commandContext.Member);

        [Command("resume")]
        [Description("Resumes the current track.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
        public async Task ResumeCommand(CommandContext commandContext)
            => await musicService.ResumeAsync(commandContext.Client, commandContext.Channel, commandContext.Member);

        [Command("volume")]
        [Description("Sets the volume of this bot. Can be a range from 0 to 100. The default volume is set to 50.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
        public async Task UpdateVolumeCommand(CommandContext commandContext, int volume)
            => await musicService.UpdateVolumeAsync(commandContext.Client, commandContext.Channel, commandContext.Member, volume);

        [Command("skip")]
        [Description("Skips the current track and plays the next one that is on the queue. If a queue does not exist, nothing is played.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
        public async Task SkipCommand(CommandContext commandContext)
            => await musicService.SkipAsync(commandContext.Client, commandContext.Channel, commandContext.Member);

        [Command("shuffle")]
        [Description("Shuffles the current queue, if it exists.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
        public async Task ShuffleCommand(CommandContext commandContext)
            => await musicService.ShuffleAsync(commandContext.Channel, commandContext.Member);

        [Command("listqueue")]
        [Description("Lists the queued tracks that are going to be played.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
        public async Task ListQueueCommand(CommandContext commandContext)
            => await musicService.ListQueueAsync(commandContext.Client, commandContext.Channel, commandContext.Member);
    }
}