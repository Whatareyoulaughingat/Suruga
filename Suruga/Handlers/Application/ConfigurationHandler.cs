using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SharpYaml.Serialization;
using Suruga.GlobalData;

namespace Suruga.Handlers.Application
{
    public class ConfigurationHandler
    {
        private readonly Serializer serializer = new();

        /// <summary>
        /// Gets or sets the configuration data of this discord bot such as, its token, command prefix, etc.
        /// </summary>
        public static ConfigurationData Data { get; set; } = new();

        private Paths Paths { get; }

        /// <summary>
        /// Serializes the configuration data from .NET types to a YML format.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task SerializeAsync()
        {
            if (!File.Exists(Paths.Configuration))
            {
                // Try to create the base directory just in case.
                Directory.CreateDirectory(Paths.Base);

                // Serialize the ConfigData class and inform the user that the config has been created.
                string serializedData = serializer.Serialize(new
                {
                    Data.BotToken,
                    Data.CommandPrefix,
                    Data.WaitForLavalinkToOpenInterval,
                    Data.ActivityType,
                    Data.Activity,
                    Data.LavalinkFilePath,
                    Data.SuccessfulEmbedHexColor,
                    Data.UnsuccessfulEmbedHexColor,
                });

                await File.WriteAllTextAsync(Paths.Configuration, serializedData).ConfigureAwait(false);

                await Console.Out.WriteLineAsync($"A new configuration file has been created in: {Paths.Configuration}. Edit the file and re-open this application.").ConfigureAwait(false);
                await Task.Delay(-1).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deserializes the configuration data from a YML format to .NET types.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task DeserializeAsync()
        {
            // Deserialize the config file.
            await using FileStream deserializationStream = File.OpenRead(Paths.Configuration);
            Data = serializer.Deserialize<ConfigurationData>(deserializationStream);
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
