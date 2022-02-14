using System;
using System.Threading.Tasks;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Suruga.Client;
using Suruga.GlobalData;
using Suruga.Handlers;

namespace Suruga;

public class Program
{
    /// <summary>
    /// The main method where execution of this bot starts.
    /// </summary>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    private static async Task Main()
    {
        Console.Title = "Suruga v2";

        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .WriteTo.Console(theme: SystemConsoleTheme.Literate)
            .WriteTo.File(Paths.Log, shared: true, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        await new ConfigurationHandler().SerializeOnCreationAndDeserializeAsync();
        await new SurugaClient().RunAsync();
    }
}
