using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System;
using System.IO;

namespace Suruga.DNet.Logging;

internal sealed class SimpleColoredConsoleFormatter : ConsoleFormatter
{
    public SimpleColoredConsoleFormatter() : base("simple-colored-console")
    {
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        => textWriter.Write($"[{DateTime.Now:d}] [{DateTime.Now:T}] [{SetSeverityColor(GetSeverity(logEntry.LogLevel))}]: " +
                            $"{logEntry.Formatter(logEntry.State, logEntry.Exception)}{Environment.NewLine}");

    private string GetSeverity(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Critical => "Critical",
        LogLevel.Error => "Error",
        LogLevel.Warning => "Warning",
        LogLevel.Information => "Information",
        LogLevel.Debug => "Debug",
        LogLevel.Trace => "Trace",
        LogLevel.None => "None",
        _ => "Unknown",
    };

    private string SetSeverityColor(string message)
    {
        string severityColor = message switch
        {
            "Critical" => "\u001b[38;5;196m", // Dark Red
            "Error" => "\u001b[38;5;160m", // Red
            "Warning" => "\u001b[38;5;226m", // Yellow
            "Information" => "\u001b[38;5;46m", // Green
            "Debug" or "Trace" => "\u001b[38;5;51m", // Cyan
            "None" => "\u001b[38;5;15m", // White
            _ => "\u001b[38;5;243m", // Gray
        };

        const string resetColorCode = "\u001b[0m";
        return $"{severityColor}{message}{resetColorCode}";
    }
}
