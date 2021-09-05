using Suruga.DSharpPlus.Client;
using Suruga.DSharpPlus.Handlers.Application;

namespace Suruga.DSharpPlus;

public class Program
{
    /// <summary>
    /// The main method where execution of this bot starts.
    /// </summary>
    /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
    private static async Task Main()
    {
        Console.Title = "Suruga.DSharpPlus";

        await new ConfigurationHandler().SerializeOnCreationAndDeserializeAsync();
        await new SurugaClient().RunAsync();
    }
}
