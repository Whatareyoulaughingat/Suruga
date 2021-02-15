using Suruga.Installer.Downloaders;
using Suruga.Installer.Data;
using Suruga.Installer.Extractor;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.Win32;

namespace Suruga.Installer
{
    public class Program
    {
        /// <summary>
        /// The main method for execution.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public static async Task Main()
        {
            // todo: unsubscribe from all events, display the download percentage info in another thread because it lags.

            Directory.CreateDirectory(Variables.BaseFolderPath);
            Directory.CreateDirectory(Variables.JavaFolderPath);

            FileDownloader fileDownloader = new FileDownloader();
            fileDownloader.DownloadOpenJdk13().Wait();
            fileDownloader.DownloadLavalink().Wait();
            fileDownloader.DownloadLavalinkApplicationYml().Wait();

            new OpenJdkFileExtractor().ExtractOpenJdk13();

            await Console.Out.WriteLineAsync("\nInstallation finished. Press any key to exit.");
            Console.ReadKey();
        }


        // todo: method that checks if this program has run for the first time. If yes: start the installer first. If no: start this program without the installer (since everything neccessary is already installed)
        public static void RunInstallerIfFirstTimeUse()
        {
            if (Convert.ToInt32(Registry.GetValue(Variables.REGISTRY_KEY, Variables.REGISTY_VALUE, 0)) == 0 | null && /* check if suruga.exe doesn't exist*/)
            {
                // download suruga.

                // Change the value since the program has run once now
                Registry.SetValue(Variables.REGISTRY_KEY, Variables.REGISTY_VALUE, 1, RegistryValueKind.DWord);
            }
            else
            {
                // run suruga directly from appdata
            }
        }
    }
}
