using Suruga.Client;
using Suruga.Handlers.Application;

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

        await new ConfigurationHandler().SerializeOnCreationAndDeserializeAsync();
        await new SurugaClient().RunAsync();
    }
}
