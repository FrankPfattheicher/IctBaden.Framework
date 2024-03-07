using IctBaden.Framework.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Framework.Test.Logging;

public class TestTronLogger
{
    private readonly ILogger _logger;
    private string _logText = string.Empty;

    public TestTronLogger()
    {
        var config = Logger.GetLogConfiguration(LogLevel.Trace);
        _logger = Logger.CreateConsoleAndTronFactory(config).CreateLogger("TronLogger");
        Tron.TronTrace.OnPrint += TronTraceOnOnPrint;
    }

    private void TronTraceOnOnPrint(string logText)
    {
        _logText = logText;
    }

    [Fact]
    public void TronShouldShowAllLevels()
    {
        _logger.LogTrace("LogTrace");
        Assert.Contains("TronLogger: LogTrace", _logText);
        
        _logger.LogDebug("LogDebug");
        Assert.Contains("TronLogger: LogDebug", _logText);

        _logger.LogInformation("LogInformation");
        Assert.Contains("TronLogger: LogInformation", _logText);
        
        _logger.LogWarning("LogWarning");
        Assert.Contains("TronLogger: LogWarning", _logText);
    }
}
