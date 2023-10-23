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
    public static string? TimestampFormat = null;
    public static LogLevel LogLevel = LogLevel.Warning;
    public static IConfiguration GetLogConfiguration() => GetLogConfiguration(LogLevel);

    public static IConfiguration GetLogConfiguration(LogLevel level) => new ConfigurationBuilder()
        .Add(new MemoryConfigurationSource
        {
            InitialData = new[] { new KeyValuePair<string, string>("LogLevel", level.ToString()) }
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
            // find static field of type ILoggerFactory in entry assembly
            var entry = Assembly.GetEntryAssembly();
            foreach (var entryType in entry!.DefinedTypes)
            {
                var fieldInfo = entryType.DeclaredFields.FirstOrDefault(f => f.FieldType == typeof(ILoggerFactory));
                var loggerFactory = (ILoggerFactory?)fieldInfo?.GetValue(null);
                if (loggerFactory == null) continue;

                Trace.TraceInformation($"Using LoggerFactory '{fieldInfo?.Name}' of type '{entryType.Name}'.");
                return loggerFactory;
            }

            Trace.TraceInformation($"No LoggerFactory found. Using console factory.");
            return CreateConsoleAndTronFactory(GetLogConfiguration());
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
            builder.AddConsole(options =>
            {
#pragma warning disable CS0618
                if(TimestampFormat != null) options.TimestampFormat = TimestampFormat;
#pragma warning restore CS0618
            });
            builder.AddProvider(new TronLoggerProvider(configuration));
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
}