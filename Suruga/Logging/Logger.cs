using Microsoft.Extensions.Logging;

namespace Suruga.Logging;

internal sealed partial class Logger
{
    private readonly ILogger _logger;

    public Logger(ILogger<Logger> logger)
        => _logger = logger;

    [LoggerMessage(EventId = 1, EventName = "Bot", Message = "{Message}")]
    internal partial void Log(LogLevel level, string message, Exception? ex = null);

    [LoggerMessage(EventId = 2, EventName = "Bot-Information", Level = LogLevel.Information, Message = "{Message}")]
    internal partial void LogInformation(string message);

    [LoggerMessage(EventId = 3, EventName = "Bot-Warning", Level = LogLevel.Debug, Message = "{Message}")]
    internal partial void LogWarning(string message);

    [LoggerMessage(EventId = 4, EventName = "Bot-Error", Level = LogLevel.Error, Message = "{Message}")]
    internal partial void LogError(string message, Exception? ex = null);

    [LoggerMessage(EventId = 5, EventName = "Bot-Critical", Level = LogLevel.Critical, Message = "{Message}")]
    internal partial void LogCritical(string message, Exception? ex = null);
}
