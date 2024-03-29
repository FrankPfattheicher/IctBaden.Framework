using System;
using IctBaden.Framework.Tron;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IctBaden.Framework.Logging;

public class TronLogger(string name, IConfiguration config) : ILogger
{
    private sealed class EmptyScope : IDisposable
    {
        public void Dispose() { }
    }
    public IDisposable BeginScope<TState>(TState state) => new EmptyScope();

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel >= config.GetValue("LogLevel", LogLevel.Information);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
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
            
        TronTrace.PrintLine($"[{logLevel.ToString().Substring(0, 4)}] {name}: {formatter(state, exception ?? new Exception())}");
    }
}