using Microsoft.Extensions.Logging;

namespace Suruga.Handlers.Discord
{
    public static class LoggingHandler
    {
        public static ILoggerFactory LoggingFactory { get; } = LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();

            loggingBuilder.AddSimpleConsole(loggingOptions =>
            {
                loggingOptions.TimestampFormat = "hh:mm:ss";
                loggingOptions.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
                loggingOptions.IncludeScopes = true;
                loggingOptions.SingleLine = true;
            });
        });

    }
}
