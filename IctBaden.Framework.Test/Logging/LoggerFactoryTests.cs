using System.Diagnostics.CodeAnalysis;
using IctBaden.Framework.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Framework.Test.Logging;

[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don\'t dispose injected")]
public class LoggerFactoryTests
{
    [Fact]
    public void DefaultFactoryShouldReturnInstance()
    {
        using var defaultFactory = Logger.DefaultFactory;
            
        Assert.NotNull(defaultFactory);
    }
        
    [Fact]
    public void DefaultLoggerShouldContainTronLogger()
    {
        using var defaultFactory = Logger.DefaultFactory;
        var logger = defaultFactory.CreateLogger("Test");
        var tronLogger = logger.GetLogger<TronLogger>();

        Assert.NotNull(tronLogger);
    }
        
    [Fact]
    public void DefaultFactoryShouldBeAbleToCreateLogger()
    {
        using var defaultFactory = Logger.DefaultFactory;
        var logger = defaultFactory.CreateLogger("Test");

        Assert.NotNull(logger);
    }
        
    [Fact]
    public void LoggerShouldBeAbleToLogToTron()
    {
        using var defaultFactory = Logger.DefaultFactory;
        var logger = defaultFactory.CreateLogger("Test");

        logger.Log(LogLevel.Information, "Test information");
        logger.Log(LogLevel.Warning, "Test warning");
        logger.Log(LogLevel.Error, "Test error");
    }

    [Fact]
    public void LogConfigurationShouldBeInitiallyContainLevelWarning()
    {
        var level = Logger.GetLogConfiguration().GetValue("LogLevel", LogLevel.None);
        Assert.Equal(LogLevel.Warning, level);
    }
        
    [Fact]
    public void LogConfigurationShouldBeChanged()
    {
        var level = Logger.GetLogConfiguration(LogLevel.Critical).GetValue("LogLevel", LogLevel.None);
        Assert.Equal(LogLevel.Critical, level);
    }
        
}