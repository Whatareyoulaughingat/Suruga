using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using DSharpPlus.Entities;

namespace Suruga.Handlers
{
    public class ConfigurationHandler
    {
        /// <summary>
        /// The path of the bot's configuration file.
        /// </summary>
        private readonly string configurationFilePath = $"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Suruga\\Configuration.json";

        /// <summary>
        /// Gets or sets the configuration data of this discord bot such as, its token, command prefix, etc.
        /// </summary>
        public static ConfigurationData Configuration { get; set; }

        /// <summary>
        /// Serializes the configuration data from .NET types to a JSON format.
        /// </summary>
        public void SerializeConfiguration()
        {
            Directory.CreateDirectory($"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Suruga");

            if (!File.Exists(configurationFilePath))
            {
                string serializedData = JsonSerializer.Serialize(new ConfigurationData(), new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText(configurationFilePath, serializedData);

                Console.WriteLine($"A new configuration file has been generated in: {configurationFilePath}. Close the program, enter the appropriate details inside the file and re-open this program.");
                Thread.Sleep(-1);
            }
        }

        /// <summary>
        /// Deserializes the configuration data from a JSON format to .NET types.
        /// </summary>
        public void DeserializeConfiguration()
        {
            string deserializedData = File.ReadAllText(configurationFilePath);
            Configuration = JsonSerializer.Deserialize<ConfigurationData>(deserializedData);
        }
    }

    [Serializable]
    public class ConfigurationData
    {
        public ConfigurationData()
        {
            Token = "Discord Token";
            ActivityType = ActivityType.Playing;
            Activity = "Activity Status";
            CommandPrefix = "Prefix";
        }

        /// <summary>
        /// Gets or sets the token required for this bot.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the activity type of this bot that will show under its name.
        /// <para></para>
        /// 0 = <see cref="ActivityType.Playing"/>.
        /// <para></para>
        /// 1 = <see cref="ActivityType.Streaming"/>.
        /// <para></para>
        /// 2 = <see cref="ActivityType.ListeningTo"/>.
        /// <para></para>
        /// 3 = <see cref="ActivityType.Watching"/>.
        /// <para></para>
        /// 4 = <see cref="ActivityType.Custom"/>.
        /// <para></para>
        /// 5 = <see cref="ActivityType.Competing"/>.
        /// </summary>
        public ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the name of the activity that will show under its name alongside the <see cref="ActivityType"/>.
        /// </summary>
        public string Activity { get; set; }

        /// <summary>
        /// Gets or sets the prefix (such as '?', '!', '-', '.'), that is going to be used in each command.
        /// </summary>
        public string CommandPrefix { get; set; }
    }
}
