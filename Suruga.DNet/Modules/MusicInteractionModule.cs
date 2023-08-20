using Discord.Interactions;
using Suruga.DNet.Services;
using System.Threading.Tasks;

namespace Suruga.DNet.Modules;

public sealed class MusicInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly MusicInteractionService _music;

    public MusicInteractionModule(MusicInteractionService music)
        => _music = music;

    [SlashCommand("leave", "Leaves from a voice channel.")]
    [RequireContext(ContextType.Guild)]
    public async Task LeaveAsyncInteraction()
        => await _music.LeaveAsync(Context.Interaction);

    [SlashCommand("play", "Plays a song or playlist.")]
    [RequireContext(ContextType.Guild)]
    public async Task PlayAsyncInteraction(string query)
        => await _music.PlayAsync(Context.Interaction, query);

    [SlashCommand("stop", "Stops the currently playing song.")]
    [RequireContext(ContextType.Guild)]
    public async Task StopAsyncInteraction()
        => await _music.StopAsync(Context.Interaction);

    [SlashCommand("skip", "Skips the current playing song.")]
    [RequireContext(ContextType.Guild)]
    public async Task SkipAsyncCommand()
    => await _music.SkipAsync(Context.Interaction);

    [SlashCommand("resume", "Resumes the current playing song.")]
    [RequireContext(ContextType.Guild)]
    public async Task PauseAsyncCommand()
        => await _music.PauseAsync(Context.Interaction);

    [SlashCommand("volume", "Sets the audio volume.")]
    [RequireContext(ContextType.Guild)]
    public async Task SetVolumeAsyncCommand(int volume)
        => await _music.SetVolumeAsync(Context.Interaction, volume);

    [SlashCommand("shuffle", "Shuffles the queue.")]
    [RequireContext(ContextType.Guild)]
    public async Task ShuffleAsyncCommand()
        => await _music.ShuffleAsync(Context.Interaction);

    [SlashCommand("lyrics", "Fetches the lyrics of the current playing song.")]
    [RequireContext(ContextType.Guild)]
    public async Task FetchLyricsAsyncCommand()
        => await _music.FetchLyricsAsync(Context.Interaction);

    [SlashCommand("artwork", "Fetches the artwork of the current playing song.")]
    [RequireContext(ContextType.Guild)]
    public async Task FetchArtworkAsyncCommand()
        => await _music.FetchArtworkAsync(Context.Interaction);
}
