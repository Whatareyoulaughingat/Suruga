using Microsoft.Extensions.Logging;
using System;

namespace Suruga.Logging;

public partial class ProgramLogger
{
    private readonly ILogger _logger;

    public ProgramLogger(ILogger<ProgramLogger> logger)
        => _logger = logger;

    [LoggerMessage(EventId = 1, EventName = "ProgramLoggingHandler-Log", Message = "{Message}")]
    internal partial void Log(LogLevel level, string message, Exception ex);

    [LoggerMessage(EventId = 2, EventName = "ProgramLoggingHandler-LogInformation", Level = LogLevel.Information, Message = "{Message}")]
    internal partial void LogInformation(string message);

    [LoggerMessage(EventId = 3, EventName = "ProgramLoggingHandler-LogWarning", Level = LogLevel.Debug, Message = "{Message}")]
    internal partial void LogWarning(string message);

    [LoggerMessage(EventId = 4, EventName = "ProgramLoggingHandler-LogError", Level = LogLevel.Error, Message = "{Message}")]
    internal partial void LogError(string message, Exception ex);

    [LoggerMessage(EventId = 5, EventName = "ProgramLoggingHandler-LogCritical", Level = LogLevel.Critical, Message = "{Message}")]
    internal partial void LogCritical(string message, Exception ex);
}
