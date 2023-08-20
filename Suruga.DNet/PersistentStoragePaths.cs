using System;
using System.IO;

namespace Suruga.DNet;

internal static class PersistentStoragePaths
{
    internal static string BaseDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Suruga");

    internal static string LogsDirectory => Path.Combine(BaseDirectory, "Logs");

    internal static string LavalinkDirectory => Path.Combine(BaseDirectory, "Lavalink");

    internal static string ConfigurationFile => Path.Combine(BaseDirectory, "Configuration.ini");

    internal static string LavalinkYmlFile => Path.Combine(LavalinkDirectory, "application.yml");
}
