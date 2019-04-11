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
            var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
            var usage = SystemInfo.GetDiskUsage(systemDrive);

            Assert.True(usage > 0.0f);
            Assert.True(usage < 100.0f);
        }
    }
}
