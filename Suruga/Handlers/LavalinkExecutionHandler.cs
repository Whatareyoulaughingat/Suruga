using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suruga.Handlers
{
    public class LavalinkExecutionHandler
    {
        public static string SearchForJava()
        {
            IEnumerable<string> java = Directory.EnumerateFiles($"C:\\Program Files", "*java.exe", new EnumerationOptions()
            {
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.Compressed | FileAttributes.System,
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
            });

            if (!java.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not find any Java version installed!");
                Console.ResetColor();
            }

            return java.Where(x => x.Contains("jdk-13.0.2")).First();
        }
    }
}
