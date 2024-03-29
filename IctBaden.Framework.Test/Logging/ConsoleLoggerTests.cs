using System;
using System.IO;
using System.Text.RegularExpressions;
using IctBaden.Framework.Logging;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace IctBaden.Framework.Test.Logging;

public sealed class ConsoleLoggerTests : IDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly StringWriter _consoleWriter;
    private readonly ILogger _logger;
    private bool _disposed;

    public ConsoleLoggerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _consoleWriter = new StringWriter();
        Console.SetOut(_consoleWriter);
        
        Logger.TimestampFormat = "hh:mm:ss ";
        var config = Logger.GetLogConfiguration(LogLevel.Warning);
#pragma warning disable IDISP004
        _logger = Logger.CreateConsoleAndTronFactory(config).CreateLogger("ConsoleLogger");
#pragma warning restore IDISP004
    }
    
    [Fact]
    public void ConsoleLogShouldHaveTimestamp()
    {
        _logger.LogError("LogWithTimestamp");
        var logText = _consoleWriter.ToString();
        _testOutputHelper.WriteLine(">>>> Log output: " + logText);
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
        _testOutputHelper.WriteLine(">>>> Log output 1: " + logText);
        Assert.DoesNotContain("LogTrace", logText);
        
        _logger.LogDebug("LogDebug");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        logTextSoFar = _consoleWriter.ToString();
        if (string.IsNullOrEmpty(logTextSoFar)) logTextSoFar = " ";
        _testOutputHelper.WriteLine(">>>> Log output 2: " + logText);
        Assert.DoesNotContain("LogDebug", logText);

        _logger.LogInformation("LogInformation");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        logTextSoFar = logText;
        if (string.IsNullOrEmpty(logTextSoFar)) logTextSoFar = " ";
        _testOutputHelper.WriteLine(">>>> Log output 3: " + logText);
        Assert.DoesNotContain("LogInformation", logText);
        
        _logger.LogWarning("LogWarning");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        logTextSoFar = logText;
        _testOutputHelper.WriteLine(">>>> Log output 4: " + logText);
        Assert.Contains("ConsoleLogger", logText);
        Assert.Contains("LogWarning", logText);

        _logger.LogError("LogError");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        logTextSoFar = logText;
        _testOutputHelper.WriteLine(">>>> Log output 5: " + logText);
        Assert.Contains("ConsoleLogger", logText);
        Assert.Contains("LogError", logText);

        _logger.LogCritical("LogCritical");
        logText = _consoleWriter.ToString().Replace(logTextSoFar, "");
        _testOutputHelper.WriteLine(">>>> Log output 6: " + logText);
        Assert.Contains("ConsoleLogger", logText);
        Assert.Contains("LogCritical", logText);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _consoleWriter.Dispose();
    }

    // ReSharper disable once UnusedMember.Local
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}