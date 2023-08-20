using Discord;
using Microsoft.Extensions.Logging;

namespace Suruga.DNet.Extensions;

internal static class LogSeverityExtensions
{
    internal static LogLevel ToLogLevel(this LogSeverity severity) => severity switch
    {
        LogSeverity.Critical => LogLevel.Critical,
        LogSeverity.Error => LogLevel.Error,
        LogSeverity.Warning => LogLevel.Warning,
        LogSeverity.Info => LogLevel.Information,
        LogSeverity.Verbose or LogSeverity.Debug => LogLevel.Debug,
        _ => LogLevel.None,
    };
}
