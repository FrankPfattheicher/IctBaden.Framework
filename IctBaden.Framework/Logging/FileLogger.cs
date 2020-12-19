using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
// ReSharper disable StringLiteralTypo

namespace IctBaden.Framework.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _context;
        private string _scopeContext = "";
    
        private readonly LogFileNameFactory _fileNameFactory;

        private class LogScope : IDisposable
        {
            private readonly FileLogger _logger;
        
            public LogScope(FileLogger logger, string context)
            {
                _logger = logger;
                _logger._scopeContext = context;
            }

            public void Dispose()
            {
                _logger._scopeContext = "";
            }
        }

        public FileLogger(LogFileNameFactory fileNameFactory, string context)
        {
            _fileNameFactory = fileNameFactory;
            _context = context;
        }
        
        public IDisposable BeginScope<TState>(TState state)
        {
            return new LogScope(this, state.ToString());
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        private static string GetLogLevelString(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                LogLevel.None => "    ",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                var fileName = _fileNameFactory.GetLogFileName();
                
                var logLine = $"{GetLogLevelString(logLevel)}: {_context} ";
                if (!string.IsNullOrEmpty(_scopeContext))
                {
                    logLine += $"{_scopeContext} ";
                }
                logLine += state.ToString();
                if (exception != null)
                {
                    logLine += ", " + exception.Message;
                }
                File.AppendAllText(fileName, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

    }
}