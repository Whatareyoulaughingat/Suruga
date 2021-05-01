using System;
using Lavalink4NET.Logging;

namespace Suruga.Handlers
{
    public class LoggingHandler : ILogger
    {
        public void Log(object source, string message, LogLevel level = LogLevel.Information, Exception exception = null)
        {
            try
            {
                Console.WriteLine($"{level} {message} (from {source.GetType().FullName})");
            }
            catch
            {
                Console.WriteLine(exception);
            }
        }
    }
}
