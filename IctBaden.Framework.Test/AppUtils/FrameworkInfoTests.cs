using IctBaden.Framework.AppUtils;
using Xunit;

namespace IctBaden.Framework.Test.AppUtils
{
    public class FrameworkInfoTests
    {
        [Fact]
        public void CurrentClrVersionShouldBe40X()
        {
            var clr = FrameworkInfo.ClrVersion;
            Assert.Contains("v4.0.", clr);
        }
        
        [Fact]
        public void CurrentRuntimeDirectoryShouldNotBeAppDirectory()
        {
            var dir = FrameworkInfo.RuntimeDirectory;
            Assert.NotEqual(ApplicationInfo.ApplicationDirectory, dir);
        }

        [Fact]
        public void FrameworkVersionShouldBeNetCore31()
        {
            var core = FrameworkInfo.FrameworkVersion;
            Assert.Contains("6.0.", core);
            
            Assert.True(FrameworkInfo.IsNetCore);
        }

        [Fact]
        public void ThisTestsShouldNotBeSelfHosted()
        {
            Assert.False(FrameworkInfo.IsSelfHosted);
        }

    }
}