﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Lavalink4NET;
using Suruga.Services;
using System.Threading.Tasks;

namespace Suruga.Modules;

public class Music : BaseCommandModule
{
    private readonly MusicService musicService;

    public Music(MusicService _musicService)
        => musicService = _musicService;

    [Command("play")]
    [Description("Plays the specified track.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task PlayCommand(CommandContext commandContext, [RemainingText] string url)
        => await musicService.PlayAsync(commandContext.Client, commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService, url);

    [Command("leave")]
    [Description("Leaves the voice channel.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task LeaveCommand(CommandContext commandContext)
        => await musicService.LeaveAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("stop")]
    [Description("Stops playing the current track.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task StopCommand(CommandContext commandContext)
        => await musicService.StopAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("pause")]
    [Description("Pauses the current track.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task PauseCommand(CommandContext commandContext)
        => await musicService.PauseAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("resume")]
    [Description("Resumes the current track.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task ResumeCommand(CommandContext commandContext)
        => await musicService.ResumeAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("volume")]
    [Description("Sets the volume of this bot. Can be a range from 0 to 100. The default volume is set to 100.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task UpdateVolumeCommand(CommandContext commandContext, float volume)
        => await musicService.UpdateVolumeAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService, volume);

    [Command("skip")]
    [Description("Skips the current track and plays the next one that is on the queue. If a queue does not exist, nothing is played.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task SkipCommand(CommandContext commandContext, int skipCount = 1)
        => await musicService.SkipAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService, skipCount);

    [Command("shuffle")]
    [Description("Shuffles the current queue, if it exists.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task ShuffleCommand(CommandContext commandContext)
        => await musicService.ShuffleAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("queue")]
    [Description("Lists the queued tracks that are going to be played.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task ListQueueCommand(CommandContext commandContext)
        => await musicService.ListQueueAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("replay")]
    [Description("Replays the current track.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task ReplayCommand(CommandContext commandContext)
        => await musicService.ReplayAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("loop")]
    [Description("Loops or unloops the current track.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task LoopCommand(CommandContext commandContext)
        => await musicService.LoopAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("nowplaying")]
    [Description("Lists some info about the current playing track.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task NowPlayingCommand(CommandContext commandContext)
        => await musicService.NowPlayingAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("clearqueue")]
    [Description("Clears the current queue.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task ClearQueueCommand(CommandContext commandContext)
        => await musicService.ClearQueueAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);

    [Command("lyrics")]
    [Description("Posts the lyrics of the current track if they exist.")]
    [RequireBotPermissions(Permissions.AccessChannels | Permissions.EmbedLinks | Permissions.Speak | Permissions.UseVoice)]
    [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.UseVoice)]
    public async Task LyricsCommand(CommandContext commandContext)
        => await musicService.PostLyricsAsync(commandContext.Channel, commandContext.Member, commandContext.Services.GetService(typeof(IAudioService)) as IAudioService);
}
