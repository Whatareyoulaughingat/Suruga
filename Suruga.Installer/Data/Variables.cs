using System;

namespace Suruga.Installer.Data
{
    /// <summary>
    /// Various links, folder and file paths used for downloading and extracting the neccessary files to run this bot.
    /// </summary>
    public struct Variables
    {
        /// <summary>
        /// The bot's registry key.
        /// </summary>
        public const string REGISTRY_KEY = @"HKEY_CURRENT_USER\Suruga";

        /// <summary>
        /// The bot's registry value inside the registry key.
        /// </summary>
        public const string REGISTY_VALUE = "FirstRun";

        /// <summary>
        /// The base folder path of this bot, where everything is.
        /// </summary>
        public static string BaseFolderPath = $"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Suruga\\";

        /// <summary>
        /// The default path where all the downloaded Java versions are located.
        /// </summary>
        public static string JavaFolderPath = $"C:\\Program Files\\Java\\";

        /// <summary>
        /// The default path where Java 13 is located.
        /// </summary>
        public static string OpenJdk13FolderPath = $"C:\\Program Files\\Java\\jdk-13.0.2\\";

        /// <summary>
        /// A download link of OpenJDK 13.
        /// </summary>
        public static string OpenJdk13DownloadLink = "https://download.java.net/java/GA/jdk13.0.2/d4173c853231432d94f001e99d882ca7/8/GPL/openjdk-13.0.2_windows-x64_bin.zip";

        /// <summary>
        /// A download link of Lavalink.
        /// </summary>
        public static string LavalinkDownloadLink = "https://github.com/Frederikam/Lavalink/releases/download/3.3.2.3/Lavalink.jar";

        /// <summary>
        /// A download link of Lavalink's application.yml file.
        /// </summary>
        public static string LavalinkApplicationYmlDownloadLink = "https://github.com/Frederikam/Lavalink/blob/master/LavalinkServer/application.yml.example";
    }
}
