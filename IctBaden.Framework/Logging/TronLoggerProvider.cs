using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IctBaden.Framework.Logging
{
    public sealed class TronLoggerProvider : ILoggerProvider
    {
        private readonly IConfiguration _config;
        private readonly ConcurrentDictionary<string, TronLogger> _loggers =
            new ConcurrentDictionary<string, TronLogger>();

        public TronLoggerProvider(IConfiguration config) =>
            _config = config;

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new TronLogger(name, _config));

        public void Dispose() => _loggers.Clear();
    }
}
