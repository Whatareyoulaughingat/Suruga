using Discord;
using Microsoft.Extensions.Logging;
using System;

namespace Suruga.Logging;

internal partial class ClientLogger
{
    private readonly ILogger _logger;

    public ClientLogger(ILogger<ClientLogger> logger)
        => _logger = logger;

    [LoggerMessage(EventId = 0, EventName = "ClientLogger-Log", Message = "{Message}")]
    internal partial void Log(LogLevel level, string message, Exception ex);

    internal LogLevel ToLogLevel(LogSeverity severity) => severity switch
    {
        LogSeverity.Critical => LogLevel.Critical,
        LogSeverity.Error => LogLevel.Error,
        LogSeverity.Warning => LogLevel.Warning,
        LogSeverity.Info => LogLevel.Information,
        LogSeverity.Verbose or LogSeverity.Debug => LogLevel.Debug,
        _ => LogLevel.None,
    };
}
