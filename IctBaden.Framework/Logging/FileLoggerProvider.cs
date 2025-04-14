using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace IctBaden.Framework.Logging;

public sealed class FileLoggerProvider(LogFileNameFactory fileNameFactory, string context) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new(System.StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, _ => new FileLogger(fileNameFactory, context));

    public void Dispose() => _loggers.Clear();
}