using System.Text.Json;
using Suruga.GlobalData;

namespace Suruga.Handlers.Application
{
    public class ConfigurationHandler
    {
        /// <summary>
        /// Gets or sets the configuration data of this discord bot such as, its token, command prefix, etc.
        /// </summary>
        public static ConfigurationData CurrentConfigurationDataInstance { get; set; }

        /// <summary>
        /// Serializes .NET types to a JSON format.
        /// </summary>
        /// <param name="configurationData">The data of <see cref="ConfigurationData"/>.</param>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task SerializeAsync(ConfigurationData configurationData)
        {
            if (!File.Exists(Paths.Configuration) || !File.Exists(Paths.Base))
            {
                Directory.CreateDirectory(Paths.Base);

                using FileStream serializationStream = File.OpenWrite(Paths.Configuration);
                await JsonSerializer.SerializeAsync(serializationStream, configurationData, new JsonSerializerOptions { WriteIndented = true });

                await Console.Out.WriteLineAsync($"A new configuration file has been created in: {Paths.Configuration}. Edit the file and re-open this application.").ConfigureAwait(false);
                await Task.Delay(-1).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deserializes the configuration data from a JSON format to .NET types.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task DeserializeAsync()
        {
            // Deserialize the config file.
            await using FileStream deserializationStream = File.OpenRead(Paths.Configuration);
            CurrentConfigurationDataInstance = await JsonSerializer.DeserializeAsync<ConfigurationData>(deserializationStream);
        }
    }

    [Serializable]
    public class ConfigurationData
    {
        public ConfigurationData()
        {
            BotToken = "Insert a token for the bot to work.";
            CommandPrefix = "?";
            WaitForLavalinkToOpenInterval = "5";
            ActivityType = DSharpPlus.Entities.ActivityType.Playing.ToString();
            Activity = "A description of the bot's activity.";
            LavalinkFilePath = $"{Directory.GetCurrentDirectory()}\\Lavalink.jar";
            SuccessfulEmbedHexColor = "#007fff";
            UnsuccessfulEmbedHexColor = "#ff0000";
        }

        public string BotToken { get; set; }

        public string CommandPrefix { get; set; }

        public string WaitForLavalinkToOpenInterval { get; set; }

        public string ActivityType { get; set; }

        public string Activity { get; set; }

        public string LavalinkFilePath { get; set; }

        public string SuccessfulEmbedHexColor { get; set; }

        public string UnsuccessfulEmbedHexColor { get; set; }
    }
}
