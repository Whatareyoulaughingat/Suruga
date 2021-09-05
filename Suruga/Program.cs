using System.ComponentModel;
using System.Diagnostics;
using Suruga.Client;
using Suruga.Handlers.Application;
using Suruga.Handlers.Lavalink;
using Suruga.Handlers.Win32;

namespace Suruga
{
    public static class Program
    {
        /// <summary>
        /// The main method where execution of this bot starts.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        private static async Task Main()
        {
            Console.Title = "Suruga";

            ConfigurationHandler configHandler = new();
            await configHandler.SerializeAsync(new ConfigurationData());
            await configHandler.DeserializeAsync();

            await RunLavalink().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(int.Parse(ConfigurationHandler.CurrentConfigurationDataInstance.WaitForLavalinkToOpenInterval))).ConfigureAwait(false);

            await new SurugaClient().RunAsync();
        }

        /// <summary>
        /// Runs Lavalink and attaches its process to this bot so everytime this bot exits, Lavalink will exit as well.
        /// <para></para>
        /// Assuming Java 13 (JRE) is installed on your system in its default location (C:\Program Files\Java\jdk-13.0.2) as well as the Lavalink.jar file is located in the same path as the executable file of this bot.
        /// </summary>
        private static async Task RunLavalink()
        {
            try
            {
                using Process lavalink = new();
                lavalink.StartInfo.CreateNoWindow = true;
                lavalink.StartInfo.UseShellExecute = true;
                lavalink.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                lavalink.StartInfo.FileName = "\"" + new JDKSearchHandler().SearchForJDK13() + "\"";
                lavalink.StartInfo.Arguments = $"-jar {ConfigurationHandler.CurrentConfigurationDataInstance.LavalinkFilePath}";
                lavalink.Start();

                // Tracks the child process. If the parent process exits, the child does as well.
                new ProcessAttachmentHandler().AddProcessAsChild(lavalink);
            }
            catch (Win32Exception)
            {
                await Console.Out.WriteLineAsync("Could not find Lavalink. Place it where this application is or configure a custom file path in the congiguration file if you have it or download it from: https://github.com/freyacodes/Lavalink and follow the installation guide.").ConfigureAwait(false);
                await Task.Delay(-1).ConfigureAwait(false);
            }
        }
    }
}