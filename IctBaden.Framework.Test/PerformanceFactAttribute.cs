using System;
using Xunit;

namespace IctBaden.Framework.Test
{
    public class PerformanceFactAttribute : FactAttribute
    {
        public PerformanceFactAttribute()
        {
            if (Environment.GetEnvironmentVariable("SkipPerformanceTests") == "true")
            {
                Skip = "Ignored Performance Testing Fact";
            }
        }
    }
}