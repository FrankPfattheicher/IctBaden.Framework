using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace IctBaden.Framework.Test;

public sealed class PerformanceFactAttribute : FactAttribute
{
    public PerformanceFactAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (Environment.GetEnvironmentVariable("SkipPerformanceTests") == "true")
        {
            Skip = "Ignored Performance Testing Fact";
        }
    }
}