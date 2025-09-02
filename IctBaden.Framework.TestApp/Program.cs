using System.Diagnostics.CodeAnalysis;
using IctBaden.Framework.AppUtils;
using IctBaden.Framework.Logging;
using Microsoft.Extensions.Logging;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace IctBaden.Framework.TestApp;

[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don\'t ignore created IDisposable")]
internal static class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");

        Logger.TimestampFormat = "hh:mm:ss ";
        Logger.LogLevel = LogLevel.Debug;

        var logger = Logger.DefaultFactory.CreateLogger("test");
        logger.LogCritical("TIMESTAMP");

        var temp = SystemInfo.GetSystemTemperature();
        logger.LogInformation("System temperature: {Temp}°C", temp.ToString("F1"));
    }
}