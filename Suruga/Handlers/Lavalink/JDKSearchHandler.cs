using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suruga.Handlers.Lavalink
{
    public class JDKSearchHandler
    {
        public string SearchForJDK13()
        {
            IEnumerable<string> java = Directory.EnumerateFiles("C:\\Program Files", "*java.exe", new EnumerationOptions
            {
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.Compressed | FileAttributes.System,
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
            });

            if (!java.Any(x => x.Contains("jdk-13")))
            {
                Console.WriteLine("Could not find Java 13. Install it before running this application.");
                Task.Delay(-1);
            }

            return java.First(x => x.Contains("jdk-13"));
        }
    }
}