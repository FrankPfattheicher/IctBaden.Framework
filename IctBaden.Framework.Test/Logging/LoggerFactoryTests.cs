using IctBaden.Framework.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Framework.Test.Logging
{
    public class LoggerFactoryTests
    {
        [Fact]
        public void DefaultFactoryShouldReturnInstance()
        {
            var defaultFactory = Logger.DefaultFactory;
            
            Assert.NotNull(defaultFactory);
        }
        
        [Fact]
        public void DefaultFactoryShouldBeAbleToCreateLogger()
        {
            var defaultFactory = Logger.DefaultFactory;
            var logger = defaultFactory.CreateLogger("Test");

            Assert.NotNull(logger);
        }
        
        [Fact]
        public void LoggerShouldBeAbleToLogToTron()
        {
            var defaultFactory = Logger.DefaultFactory;
            var logger = defaultFactory.CreateLogger("Test");

            logger.Log(LogLevel.Information, "Test information");
            logger.Log(LogLevel.Warning, "Test warning");
            logger.Log(LogLevel.Error, "Test error");
        }
        
    }
}