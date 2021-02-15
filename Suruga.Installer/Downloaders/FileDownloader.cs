using Suruga.Installer.Converters;
using Suruga.Installer.Data;
using Suruga.Installer.Handler;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Suruga.Installer.Downloaders
{
    // OpenJDK 13: https://jdk.java.net/
    // Lavalink: https://github.com/Frederikam/Lavalink
    public class FileDownloader
    {
        /// <summary>
        /// Asynchronously downloads OpenJDK 13.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task DownloadOpenJdk13()
        {
            try
            {
                using HttpDownloader client = new HttpDownloader(Variables.OpenJdk13DownloadLink, Variables.BaseFolderPath + "openjdk-13.0.2_windows-x64_bin.zip");
                await Console.Out.WriteLineAsync("Downloading OpenJDK 13..");

                client.ProgressChanged += async (totalFileSize, totalBytesDownloaded, progressPercentage) => await Console.Out.WriteLineAsync($"Downloaded: {BytesToMbConverter.SizeSuffix(totalBytesDownloaded)} out of {BytesToMbConverter.SizeSuffix((long)totalFileSize)} - {progressPercentage}%");
                await client.DownloadAsync();
            }
            catch (Exception ax)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("Exception: " + ax.Message);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Asynchronously downloads Lavalink.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task DownloadLavalink()
        {
            try
            {
                using HttpDownloader client = new HttpDownloader(Variables.LavalinkDownloadLink, Variables.BaseFolderPath + "Lavalink.jar");
                await Console.Out.WriteLineAsync("\nDownloading Lavalink..");

                client.ProgressChanged += async (totalFileSize, totalBytesDownloaded, progressPercentage) => await Console.Out.WriteLineAsync($"Downloaded: {BytesToMbConverter.SizeSuffix(totalBytesDownloaded)} out of {BytesToMbConverter.SizeSuffix((long)totalFileSize)} - {progressPercentage}%");
                await client.DownloadAsync();
            }
            catch (Exception bx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("Exception: " + bx.Message);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Asynchronously downloads the 'application.yml' file used by Lavalink.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task DownloadLavalinkApplicationYml()
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                await Console.Out.WriteLineAsync("\nDownloading the 'application.yml' file..");

                File.WriteAllText(Variables.BaseFolderPath + "application.yml", await httpClient.GetStringAsync("https://raw.githubusercontent.com/Frederikam/Lavalink/master/LavalinkServer/application.yml.example"));
            }
            catch (Exception cx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Out.WriteLineAsync("Exception: " + cx.Message);
                Console.ResetColor();
            }
        }
    }
}