using Suruga.GlobalData;
using System.Text.Json;

namespace Suruga.Handlers.Application;

public class ConfigurationHandler
{
    /// <summary>
    /// Gets or sets the configuration data of this discord bot such as, its token, command prefix, etc.
    /// </summary>
    public static ConfigurationData Data { get; private protected set; }

    /// <summary>
    /// Serializes .NET types to a JSON format.
    /// </summary>
    /// <param name="configurationData">The data of <see cref="ConfigurationData"/>.</param>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    public async Task SerializeOnCreationAndDeserializeAsync()
    {
        if (!File.Exists(Paths.Configuration))
        {
            Directory.CreateDirectory(Paths.Base);

            string serializedData = JsonSerializer.Serialize(new ConfigurationData(), new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(Paths.Configuration, serializedData);

            Console.WriteLine($"A new configuration file has been created in: {Paths.Configuration}. Edit the file and re-open this application.");
            await Task.Delay(-1).ConfigureAwait(false);
        }

        await DeserializeAsync();
    }

    /// <summary>
    /// Deserializes the configuration data from a JSON format to .NET types.
    /// </summary>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    private async Task DeserializeAsync()
    {
        await using FileStream deserializationStream = File.OpenRead(Paths.Configuration);
        Data = await JsonSerializer.DeserializeAsync<ConfigurationData>(deserializationStream);
    }
}

public record ConfigurationData
{
    public ConfigurationData()
    {
        Token = string.Empty;
        CommandPrefixes = new();
        ActivityType = string.Empty;
        Activity = string.Empty;
        SuccessfulCommandResultInEmbedHexColor = "#007fff";
        UnsuccessfulCommandResultInEmbedHexColor = "#ff0000";
        DisconnectInVoiceChannelAfterInMinutes = "5";
    }

    public string Token { get; init; }

    /// <summary>
    /// TKey is the guild and TValue is the command prefix.
    /// </summary>
    public Dictionary<ulong, string> CommandPrefixes { get; init; }

    public string ActivityType { get; init; }

    public string Activity { get; init; }

    public string SuccessfulCommandResultInEmbedHexColor { get; init; }

    public string UnsuccessfulCommandResultInEmbedHexColor { get; init; }

    public string DisconnectInVoiceChannelAfterInMinutes { get; init; }
}
