using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo

namespace IctBaden.Framework.Logging;

public class FileLogger : ILogger
{
    private string _scopeContext = string.Empty;
    private LogLevel _logLevel;
    private bool _timestamp = true;
    private readonly LogFileNameFactory _fileNameFactory;
    private readonly string _context;

    public FileLogger(LogFileNameFactory fileNameFactory, string context)
    {
        _fileNameFactory = fileNameFactory;
        _context = context;
    }

    private sealed class LogScope : IDisposable
    {
        private readonly FileLogger _logger;

        public LogScope(FileLogger logger, string context)
        {
            _logger = logger;
            _logger._scopeContext = context;
        }

        public void Dispose()
        {
            _logger._scopeContext = string.Empty;
        }
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return new LogScope(this, state?.ToString() ?? "");
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _logLevel;
    public void SetLogLevel(LogLevel logLevel) => _logLevel = logLevel;
    public void SetTimestamp(bool timestamp) => _timestamp = timestamp;

    private static string GetLogLevelString(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRIT",
            LogLevel.None => "    ",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        try
        {
            if (!IsEnabled(logLevel)) return;
            if (_fileNameFactory.Cycle == LogFileCycle.None) return;

            var fileName = _fileNameFactory.GetLogFileName();

            var logLine = new StringBuilder();
            if (_timestamp)
            {
                logLine.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            }

            logLine.Append('\t');
            logLine.Append(GetLogLevelString(logLevel));
            logLine.Append('\t');
            logLine.Append(_context);
            logLine.Append('\t');
            if (!string.IsNullOrEmpty(_scopeContext))
            {
                logLine.Append("=> ");
                logLine.Append(_scopeContext);
                logLine.Append('\t');
            }

            logLine.Append(state);
            if (exception != null)
            {
                logLine.Append(", ");
                logLine.Append(exception.Message);
            }

            logLine.AppendLine();
            File.AppendAllText(fileName, logLine.ToString());
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.Message);
        }
    }
}