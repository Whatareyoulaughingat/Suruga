using Discord;
using Discord.Interactions;
using Suruga.Services;
using System.Threading.Tasks;

namespace Suruga.Modules.InteractionFramework;

public sealed class MusicInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly MusicService _music;

    public MusicInteractionModule(MusicService music)
        => _music = music;

    [SlashCommand("leave", "Leaves from a voice channel.")]
    public async Task LeaveAsyncInteraction()
        => await _music.LeaveAsync(Context.User as IGuildUser, Context.Channel);

    [SlashCommand("play", "Plays a song or playlist.")]
    public async Task PlayAsyncInteraction(string query, IVoiceChannel channel = null)
        => await _music.PlayAsync(Context.User as IGuildUser, Context.Channel, query, channel);

    [SlashCommand("stop", "Stops the currently playing song.")]
    public async Task StopAsyncInteraction()
        => await _music.StopAsync(Context.User as IGuildUser, Context.Channel);
}
