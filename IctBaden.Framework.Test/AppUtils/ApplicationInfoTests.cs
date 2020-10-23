using System.IO;
using IctBaden.Framework.AppUtils;
using Xunit;

namespace IctBaden.Framework.Test.AppUtils
{
    public class ApplicationInfoTests
    {
        [Fact]
        public void ApplicationDirectoryShouldBeSameAs()
        {
            var here = Path.GetDirectoryName(typeof(ApplicationInfoTests).Assembly.Location);
            var directory = ApplicationInfo.ApplicationDirectory;
            Assert.Equal(here, directory);
        }
        
        [Fact]
        public void IsRunningInUnitTestShouldAlwaysReturnTrueHere()
        {
            var inUnitTest = ApplicationInfo.IsRunningInUnitTest;
            Assert.True(inUnitTest);
        }
        
        
    }
}