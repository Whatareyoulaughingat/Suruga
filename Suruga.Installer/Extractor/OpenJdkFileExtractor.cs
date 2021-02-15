using Suruga.Installer.Data;
using System.IO.Compression;
using Suruga.Installer.Downloaders;
using System;
using System.IO;

namespace Suruga.Installer.Extractor
{
    public class OpenJdkFileExtractor
    {
        /// <summary>
        /// Extracts the .zip file downloaded from <see cref="FileDownloader.DownloadOpenJdk13()"/>.
        /// </summary>
        public void ExtractOpenJdk13()
        {
            if (!Directory.Exists(Variables.OpenJdk13FolderPath))
            {
                Console.WriteLine($"\nExtracting OpenJDK 13 to {Variables.OpenJdk13FolderPath}");

                ZipFile.ExtractToDirectory(Variables.BaseFolderPath + "openjdk-13.0.2_windows-x64_bin.zip", Variables.JavaFolderPath);
                File.Delete(Variables.BaseFolderPath + "openjdk-13.0.2_windows-x64_bin.zip");
            }
            else
            {
                Console.WriteLine("\nOpenJDK 13 is already installed.");
                File.Delete(Variables.BaseFolderPath + "openjdk-13.0.2_windows-x64_bin.zip");
            }
        }
    }
}
