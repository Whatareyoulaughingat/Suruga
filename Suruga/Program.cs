using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Suruga.Client;
using Suruga.Extensions;
using Suruga.Handlers;

namespace Suruga
{
    public class Program
    {
        /// <summary>
        /// The main method where execution starts.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        private static async Task Main()
        {
            Console.Title = "Suruga v4";

            ConfigurationHandler configurationHandler = new ConfigurationHandler();
            configurationHandler.SerializeConfiguration();
            configurationHandler.DeserializeConfiguration();

            RunLavalink();

            await new SurugaClient().RunAsync();
        }

        /// <summary>
        /// Runs Lavalink and attaches its process to this bot so everytime this bot exits, Lavalink will exit too.
        /// <para></para>
        /// Assuming Java 13 (JRE) is installed on your system in its default location (C:\Program Files\Java\jdk-13.0.2) as well as the Lavalink.jar file is located in the same path as the executable file of this bot.
        /// </summary>
        private static void RunLavalink()
        {
            using Process lavalink = new Process();
            lavalink.StartInfo.CreateNoWindow = true;
            lavalink.StartInfo.UseShellExecute = true;
            lavalink.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            lavalink.StartInfo.FileName = "C:\\Program Files\\Java\\jdk-13.0.2\\bin\\java";
            lavalink.StartInfo.Arguments = $"-jar {ProcessExtensions.EncodeParameterArgument($"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Suruga\\Lavalink.jar")}";
            lavalink.Start();

            // Tracks the child process. If the parent process exits, the child does as well.
            ProcessExtensions.AddProcessAsChild(lavalink);
        }
    }
}
