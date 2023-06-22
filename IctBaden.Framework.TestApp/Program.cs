using IctBaden.Framework.AppUtils;
using Microsoft.Extensions.Logging;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace IctBaden.Framework.TestApp;

internal static class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");

        Logging.Logger.TimestampFormat = "hh:mm:ss ";
        Logging.Logger.LogLevel = LogLevel.Debug;
        
        var logger = Logging.Logger.DefaultFactory.CreateLogger("test");
        logger.LogCritical("TIMESTAMP");

        var temp = SystemInfo.GetSystemTemperature();
        logger.LogInformation($"System temperature: {temp:F1}°C");
    }
}