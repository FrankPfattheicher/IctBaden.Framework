using Microsoft.Extensions.Logging;

namespace IctBaden.Framework.TestApp;

internal static class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");

        Logging.Logger.TimestampFormat = "hh:mm:ss ";
        var logger = Logging.Logger.DefaultFactory.CreateLogger("test");
        logger.LogCritical("TIMESTAMP");
    }
}