using System;
using System.IO;

namespace Suruga.Global;

internal static class Paths
{
    internal static string BaseDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Suruga");

    internal static string LogsDirectory => Path.Combine(BaseDirectory, "Logs");

    internal static string BinariesDirectory => Path.Combine(BaseDirectory, "Binaries");

    internal static string ConfigurationFile => Path.Combine(BaseDirectory, "Configuration.ini");
}
