using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Framework.Logging
{
    public static class Logger
    {
        public static IConfiguration GetLogConfiguration() => GetLogConfiguration(LogLevel.Warning);
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
                    var loggerFactory = (ILoggerFactory)fieldInfo?.GetValue(null);
                    if (loggerFactory == null) continue;

                    Trace.TraceInformation($"Using LoggerFactory '{fieldInfo.Name}' of type '{entryType.Name}'.");
                    return loggerFactory;
                }

                Trace.TraceWarning($"No LoggerFactory found. Using console factory.");
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
            var minimumLogLevel = configuration.GetValue("LogLevel", LogLevel.Information);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddProvider(new TronLoggerProvider(configuration));
                builder.SetMinimumLevel(minimumLogLevel);
            });
            return loggerFactory;
        }
    }
}