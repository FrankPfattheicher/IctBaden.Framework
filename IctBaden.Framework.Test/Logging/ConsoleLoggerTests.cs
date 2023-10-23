using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using IctBaden.Framework.Logging;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace IctBaden.Framework.Test.Logging;

public class ConsoleLoggerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly StringWriter _consoleWriter;
    private readonly ILogger _logger;

    public ConsoleLoggerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _consoleWriter = new StringWriter();
        Console.SetOut(_consoleWriter);
        
        Logger.TimestampFormat = "hh:mm:ss ";
        var config = Logger.GetLogConfiguration(LogLevel.Warning);
        _logger = Logger.CreateConsoleAndTronFactory(config).CreateLogger("ConsoleLogger");
    }
    
    [Fact]
    public void ConsoleLogShouldHaveTimestamp()
    {
        _logger.LogError("LogWithTimestamp");
        var logText = _consoleWriter.ToString();
        _testOutputHelper.WriteLine("Log output: " + logText);
        Assert.True(new Regex("[0-9]+:[0-9]+:[0-9]+").Match(logText).Success);
    }

    [Fact]
    public void ConsoleLogShouldOnlyShowWarningsAndErrors()
    {
        var logTextSoFar = _consoleWriter.ToString();
        if (string.IsNullOrEmpty(logTextSoFar)) logTextSoFar = " ";

        _logger.LogTrace("LogTrace");
        var logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        logTextSoFar = _consoleWriter.ToString();
        if (string.IsNullOrEmpty(logTextSoFar)) logTextSoFar = " ";
        Assert.DoesNotContain("LogTrace", logText);
        
        _logger.LogDebug("LogDebug");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        logTextSoFar = _consoleWriter.ToString();
        if (string.IsNullOrEmpty(logTextSoFar)) logTextSoFar = " ";
        Assert.DoesNotContain("LogDebug", logText);

        _logger.LogInformation("LogInformation");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        logTextSoFar = logText;
        if (string.IsNullOrEmpty(logTextSoFar)) logTextSoFar = " ";
        Assert.DoesNotContain("LogInformation", logText);
        
        _logger.LogWarning("LogWarning");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        logTextSoFar = logText;
        Assert.Contains("ConsoleLogger", logText);
        Assert.Contains("LogWarning", logText);

        _logger.LogError("LogError");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        logTextSoFar = logText;
        Assert.Contains("ConsoleLogger", logText);
        Assert.Contains("LogError", logText);

        _logger.LogCritical("LogCritical");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        Assert.Contains("ConsoleLogger", logText);
        Assert.Contains("LogCritical", logText);
    }



    
    
}