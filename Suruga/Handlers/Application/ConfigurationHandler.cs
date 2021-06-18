using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Suruga.GlobalData;

namespace Suruga.Handlers.Application
{
    public class ConfigurationHandler
    {
        /// <summary>
        /// Gets or sets the configuration data of this discord bot such as, its token, command prefix, etc.
        /// </summary>
        public static ConfigurationData Data { get; set; }

        /// <summary>
        /// Serializes the configuration data from .NET types to a JSON format.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public static async Task SerializeConfigurationAsync()
        {
            if (!File.Exists(Paths.Configuration))
            {
                Directory.CreateDirectory($"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Suruga");

                await using FileStream serializationStream = new(
                    path: Paths.Configuration,
                    mode: FileMode.Create,
                    access: FileAccess.Write,
                    share: FileShare.None,
                    bufferSize: 4096,
                    useAsync: true);

                await JsonSerializer.SerializeAsync(serializationStream, new ConfigurationData(), new JsonSerializerOptions() { WriteIndented = true });

                serializationStream.Position = 0;

                await Console.Out.WriteLineAsync($"A new configuration file has been created in: {Paths.Configuration}. Edit the file and re-open this application.");
                await Task.Delay(-1);
            }
        }

        /// <summary>
        /// Deserializes the configuration data from a JSON format to .NET types.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public static async Task DeserializeConfigurationAsync()
        {
            await using Stream deserializationStream = new FileStream(
                path: Paths.Configuration,
                mode: FileMode.Open,
                access: FileAccess.Read,
                share: FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            Data = await JsonSerializer.DeserializeAsync<ConfigurationData>(deserializationStream);
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
