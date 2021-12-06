using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace IctBaden.Framework.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly LogFileNameFactory _fileNameFactory;
        private readonly string _context;

        private readonly ConcurrentDictionary<string, FileLogger> _loggers =
            new ConcurrentDictionary<string, FileLogger>();

        public FileLoggerProvider(LogFileNameFactory fileNameFactory, string context)
        {
            _fileNameFactory = fileNameFactory;
            _context = context;
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, _ => new FileLogger(_fileNameFactory, _context));

        public void Dispose() => _loggers.Clear();
    }
}
