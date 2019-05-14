using System;
using System.IO;
using IctBaden.Framework.AppUtils;
using Xunit;

namespace IctBaden.Framework.Test.AppUtils
{
    public class SystemInfoTest
    {
        [Fact]
        public void GetDiskUsageShouldReturnPercentage()
        {
            var currentDrive = Path.GetPathRoot(Environment.CurrentDirectory);
            var usage = SystemInfo.GetDiskUsage(currentDrive);

            Assert.True(usage > 0.0f);
            Assert.True(usage < 100.0f);
        }
    }
}
