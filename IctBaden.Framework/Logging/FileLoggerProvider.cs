using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace IctBaden.Framework.Logging;

public class FileLoggerProvider(LogFileNameFactory fileNameFactory, string context) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, _ => new FileLogger(fileNameFactory, context));

    public void Dispose() => _loggers.Clear();
}