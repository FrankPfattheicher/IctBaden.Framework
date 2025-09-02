using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Framework.Logging;

public static class Logger
{
#pragma warning disable MA0069
#pragma warning disable CA2211
    public static string? TimestampFormat = null;
    public static LogLevel LogLevel = LogLevel.Warning;
#pragma warning restore CA2211
#pragma warning restore MA0069
    public static IConfiguration GetLogConfiguration() => GetLogConfiguration(LogLevel);

    public static IConfiguration GetLogConfiguration(LogLevel level) => new ConfigurationBuilder()
        .Add(new MemoryConfigurationSource
        {
            InitialData = [new KeyValuePair<string, string>("LogLevel", level.ToString())]
        })
        .Build();


    /// <summary>
    /// Tries to find static field of type ILoggerFactory in entry assembly.
    /// If not found return simple factory for console and tron output with default configuration.
    /// </summary>
    public static ILoggerFactory DefaultFactory
    {
        get
        {
            // find a static field of type ILoggerFactory in the entry assembly
            var entry = Assembly.GetEntryAssembly();
            foreach (var entryType in entry!.DefinedTypes)
            {
                try
                {
                    var fieldInfo = entryType.DeclaredFields
                        .Where(f => f.IsStatic)
                        .FirstOrDefault(f => f.FieldType == typeof(ILoggerFactory));
                    var loggerFactory = (ILoggerFactory?)fieldInfo?.GetValue(null);
                    if (loggerFactory == null) continue;

                    Trace.TraceInformation($"Using LoggerFactory '{fieldInfo?.Name}' of type '{entryType.Name}'.");
                    return loggerFactory;
                }
                catch
                {
                    // ignore
                }
            }

            Trace.TraceInformation("No LoggerFactory found. Using console factory.");
#pragma warning disable IDISP012
            return CreateConsoleAndTronFactory(GetLogConfiguration());
#pragma warning restore IDISP012
        }
    }

    /// <summary>
    /// Creates logger factory for console and tron output with given configuration.
    /// Configuration settings:
    /// LogLevel - overall level (LogLevel.Information)
    /// TronTraceLogLevel - TRON output level (LogLevel.Information)
    /// </summary>
    /// <param name="configuration">Configuration to use</param>
    /// <returns>Logger factory</returns>
    public static ILoggerFactory CreateConsoleAndTronFactory(IConfiguration configuration)
    {
        var minimumLogLevel = configuration.GetValue("LogLevel", LogLevel);
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                if (TimestampFormat != null)
                {
                    options.TimestampFormat = TimestampFormat;
                }
            });
#pragma warning disable IDISP001
            var tronLoggerProvider = new TronLoggerProvider(configuration);
#pragma warning restore IDISP001
            builder.AddProvider(tronLoggerProvider);
            builder.SetMinimumLevel(minimumLogLevel);
        });
        return loggerFactory;
    }


    public static LogLevel GetLevelFromFirstChar(string? logLevel, LogLevel defaultLevel = LogLevel.Trace)
    {
        var firstChar = string.IsNullOrEmpty(logLevel) ? "_" : logLevel.Substring(0, 1);
        var possibleLevels = Enum.GetNames(typeof(LogLevel));
        var matchedLevel =
            possibleLevels.FirstOrDefault(n => n.StartsWith(firstChar, StringComparison.InvariantCultureIgnoreCase))
            ?? defaultLevel.ToString();

        return (LogLevel)Enum.Parse(typeof(LogLevel), matchedLevel);
    }
    
    public static TLogger? GetLogger<TLogger>(this ILogger logger) where TLogger : ILogger
    {
        var loggersInfo = logger.GetType()
            .GetProperty("Loggers", BindingFlags.Instance | BindingFlags.Public);
        
        var loggers = loggersInfo?.GetValue(logger);
        if (loggers is not Array loggerInformation) return default;

        foreach (var loggerInfoObject in loggerInformation)
        {
            var loggerInfo = loggerInfoObject?.GetType()
                .GetProperty("Logger", BindingFlags.Instance | BindingFlags.Public);
            
            if (loggerInfo?.GetValue(loggerInfoObject) is TLogger foundLogger)
            {
                return foundLogger;
            }
        }
        return default;
    }

}