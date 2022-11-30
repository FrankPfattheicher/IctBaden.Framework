using System;
using IctBaden.Framework.Tron;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IctBaden.Framework.Logging
{
    public class TronLogger : ILogger
    {
        private readonly string _name;
        private readonly IConfiguration _config;

        public TronLogger(string name, IConfiguration config)
        {
            _name = name;
            _config = config;
        }

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= _config.GetValue("LogLevel", LogLevel.Information);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                    TronTrace.SetColor(TraceColor.Text);
                    break;
                case LogLevel.Debug:
                    TronTrace.SetColor(TraceColor.Text);
                    break;
                case LogLevel.Information:
                    TronTrace.SetColor(TraceColor.Info);
                    break;
                case LogLevel.Warning:
                    TronTrace.SetColor(TraceColor.Warning);
                    break;
                case LogLevel.Error:
                    TronTrace.SetColor(TraceColor.Error);
                    break;
                case LogLevel.Critical:
                    TronTrace.SetColor(TraceColor.FatalError);
                    break;
                default:
                    TronTrace.SetColor(TraceColor.Text);
                    break;
            }
            TronTrace.PrintLine($"[{logLevel.ToString().Substring(0, 4)}] {_name}: {formatter(state, exception)}");
        }
    }
}
