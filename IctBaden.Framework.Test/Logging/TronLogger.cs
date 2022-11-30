using IctBaden.Framework.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Framework.Test.Logging;

public class TronLogger
{
    private readonly ILogger _logger;

    public TronLogger()
    {
        var config = Logger.GetLogConfiguration(LogLevel.Trace);
        _logger = Logger.CreateConsoleAndTronFactory(config).CreateLogger("RepoSync");

    }

    [Fact]
    public void TronShouldShowAllLevels()
    {
        _logger.LogTrace("LogTrace");
        _logger.LogDebug("LogDebug");
        _logger.LogInformation("LogInformation");
        _logger.LogWarning("LogWarning");
    }
}
