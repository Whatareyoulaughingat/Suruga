using Discord.Interactions;
using Discord.WebSocket;
using LLama;
using LLama.Common;
using LLama.Exceptions;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Suruga.Contexts;
using Suruga.Contexts.Entities;
using Suruga.Options;
using System.Collections.Concurrent;
using SemanticChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;

#pragma warning disable CS1591
namespace Suruga.Commands;

[Group("llm", "Large language model related commands.")]
public sealed class LargeLanguageModelCommands(IOptions<LargeLanguageModelCommandsOptions> options, DatabaseContext database) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly ConcurrentQueue<KeyValuePair<ulong, string>> Sessions = new();

    private static readonly SemaphoreSlim SessionLimiter = new(1);

    private readonly string? _instructions = options.Value.Instructions;

    private readonly ModelParams _modelParams = new(options.Value.Model!.FullName);

    [SlashCommand("prompt", "Prompts the LLM model with a message.")]
    public async Task PromptAsync
    (
        [Summary(description: "The message to prompt the bot with.")] string input,
        [Summary(description: "Set the LLM model's personality and traits. Use on an empty session or run /clear first.")] string? characteristics = null
    )
    {
        await DeferAsync();

        Sessions.Enqueue(KeyValuePair.Create(Context.Interaction.Id, input));

        if (SessionLimiter.CurrentCount == 0)
        {
            await ModifyOriginalResponseAsync(x => x.Content = "Session has been added to the queue.");
        }

        await SessionLimiter.WaitAsync();

        if (!Sessions.TryDequeue(out KeyValuePair<ulong, string> session))
        {
            SessionLimiter.Release();
            return;
        }

        using LLamaWeights weights = LLamaWeights.LoadFromFile(_modelParams);
        StatelessExecutor executor = new(weights, _modelParams);
        LLamaSharpChatCompletion chat = new(executor);

        SemanticChatHistory history = await database.RetrieveHistoryAsync(Context.User.Id) ?? chat.CreateNewChat(characteristics ?? _instructions);
        history.AddUserMessage($"{session.Value} - {((SocketGuildUser)Context.User).Nickname ?? Context.User.Username}");

        try
        {
            ChatMessageContent reply = await chat.GetChatMessageContentAsync(history);
            history.AddAssistantMessage(reply.Content ?? string.Empty);

            await database.SaveHistoryAsync(history, Context.User.Id);
            await ModifyOriginalResponseAsync(x => x.Content = CleanResponse(history.Last().Content ?? "Failed to load response."));
        }
        catch (LLamaDecodeError ex)
        {
            await ModifyOriginalResponseAsync(x => x.Content = $"Failed to load response (Message: {ex.Message})");
        }
        finally
        {
            SessionLimiter.Release();
        }
    }

    [SlashCommand("clear", "Clears the chat history.")]
    public async Task ClearHistoryAsync()
    {
        bool result = await database.ClearAsync<LargeLanguageModelSessionEntity>(x => x.UserId == Context.User.Id);

        if (!result)
        {
            await RespondAsync("Could not clear session history.");
            return;
        }

        await RespondAsync("Session history cleared.");
    }

    private static string CleanResponse(string prompt)
        => prompt.EndsWith("User:") ? prompt[..^"User:".Length] : prompt;
}
