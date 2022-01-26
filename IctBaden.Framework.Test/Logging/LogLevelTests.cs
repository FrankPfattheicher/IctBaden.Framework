using IctBaden.Framework.Logging;
using Microsoft.Extensions.Logging;
using Xunit;
// ReSharper disable StringLiteralTypo

namespace IctBaden.Framework.Test.Logging;

public class LogLevelTests
{
    [Fact]
    public void GetLevelFromFirstCharShouldMatchSingleChar()
    {
        var logLevel = Logger.GetLevelFromFirstChar("W");
        Assert.Equal(LogLevel.Warning, logLevel);
    }
    
    [Fact]
    public void GetLevelFromFirstCharShouldMatchString()
    {
        var logLevel = Logger.GetLevelFromFirstChar("trce");
        Assert.Equal(LogLevel.Trace, logLevel);
    }

    [Fact]
    public void GetLevelFromFirstCharShouldReturnDefaultOnNullInput()
    {
        var logLevel = Logger.GetLevelFromFirstChar(null, LogLevel.Debug);
        Assert.Equal(LogLevel.Debug, logLevel);
    }

    [Fact]
    public void GetLevelFromFirstCharShouldReturnDefaultOnEmptyInput()
    {
        var logLevel = Logger.GetLevelFromFirstChar("", LogLevel.Debug);
        Assert.Equal(LogLevel.Debug, logLevel);
    }

    [Fact]
    public void GetLevelFromFirstCharShouldReturnDefaultOnNoMatch()
    {
        var logLevel = Logger.GetLevelFromFirstChar("X", LogLevel.Debug);
        Assert.Equal(LogLevel.Debug, logLevel);
    }

}