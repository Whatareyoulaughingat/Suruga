using Discord;
using Discord.Commands;
using Suruga.Services;
using System.Threading.Tasks;

namespace Suruga.Modules.Commands;

public sealed class MusicCommandsModule : ModuleBase<SocketCommandContext>
{
    private readonly MusicService _music;

    public MusicCommandsModule(MusicService music)
        => _music = music;

    [Command("leave")]
    public async Task LeaveAsyncCommand()
        => await _music.LeaveAsync(Context.User as IGuildUser, Context.Channel);

    [Command("play")]
    public async Task PlayAsyncCommand(string query, IVoiceChannel channel = null)
        => await _music.PlayAsync(Context.User as IGuildUser, Context.Channel, query, channel);

    [Command("stop")]
    public async Task StopAsyncCommand()
        => await _music.StopAsync(Context.User as IGuildUser, Context.Channel);

    [Command("skip")]
    public async Task SkipAsyncCommand()
        => await _music.SkipAsync(Context.User as IGuildUser, Context.Channel);

    [Command("resume")]
    public async Task PauseAsyncCommand()
        => await _music.PauseAsync(Context.User as IGuildUser, Context.Channel);

    [Command("volume")]
    public async Task SetVolumeAsyncCommand(int volume)
        => await _music.SetVolumeAsync(Context.User as IGuildUser, Context.Channel, volume);

    [Command("shuffle")]
    public async Task ShuffleAsyncCommand()
        => await _music.ShuffleAsync(Context.User as IGuildUser, Context.Channel);

    [Command("lyrics")]
    public async Task FetchLyricsAsyncCommand()
        => await _music.FetchLyricsAsync(Context.User as IGuildUser, Context.Channel);

    [Command("artwork")]
    public async Task FetchArtworkAsyncCommand()
        => await _music.FetchArtworkAsync(Context.User as IGuildUser, Context.Channel);
}
