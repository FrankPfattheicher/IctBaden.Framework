using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Framework.Test.Logging;

public class ConsoleLoggerTests
{
    [Fact]
    public void ConsoleLogShouldHaveTimestamp()
    {
        IctBaden.Framework.Logging.Logger.TimestampFormat = "hh:mm:ss ";
        var logger = IctBaden.Framework.Logging.Logger.DefaultFactory.CreateLogger("test");
        logger.LogCritical("TIMESTAMP");
    }
}