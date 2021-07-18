using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suruga.Handlers.Lavalink
{
    public static class LavalinkExecutionHandler
    {
        public static string SearchForJavaAsync()
        {
            IEnumerable<string> java = Directory.EnumerateFiles($"C:\\Program Files", "*java.exe", new EnumerationOptions
            {
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.Compressed | FileAttributes.System,
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
            });

            if (!java.Where(x => x.Contains("jdk-13")).Any())
            {
                Console.WriteLine("Could not find Java 13. Install it before running this application.");
                Task.Delay(-1);
            }

            return java.Where(x => x.Contains("jdk-13")).First();
        }
    }
}