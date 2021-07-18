using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Suruga.Client;
using Suruga.Handlers.Application;
using Suruga.Handlers.Lavalink;
using Suruga.Handlers.Win32;

namespace Suruga
{
    public class Program
    {
        /// <summary>
        /// The main method where execution of this bot starts.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        private static async Task Main()
        {
            Console.Title = "Suruga";

            ConfigurationHandler configHandler = new();
            await configHandler.SerializeAsync();
            await configHandler.DeserializeAsync();

            await RunLavalink();
            await Task.Delay(TimeSpan.FromSeconds(int.Parse(ConfigurationHandler.Data.WaitForLavalinkToOpenInterval)));

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
                lavalink.StartInfo.FileName = "\"" + LavalinkExecutionHandler.SearchForJavaAsync() + "\"";
                lavalink.StartInfo.Arguments = $"-jar {ConfigurationHandler.Data.LavalinkFilePath}";
                lavalink.Start();

                // Tracks the child process. If the parent process exits, the child does as well.
                new ProcessAttachmentHandler().AddProcessAsChild(lavalink);
            }
            catch (Win32Exception)
            {
                await Console.Out.WriteLineAsync("Could not find Lavalink. Place it where this application is or configure a custom file path in the congiguration file if you have it or download it from: https://github.com/freyacodes/Lavalink and follow the installation guide.");
                await Task.Delay(-1).ConfigureAwait(false);
            }
        }
    }
}