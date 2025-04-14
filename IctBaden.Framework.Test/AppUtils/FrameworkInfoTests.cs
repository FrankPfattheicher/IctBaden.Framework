using IctBaden.Framework.AppUtils;
using Xunit;

namespace IctBaden.Framework.Test.AppUtils;

public class FrameworkInfoTests
{
    [Fact]
    public void CurrentClrVersionShouldBe40X()
    {
        var clr = FrameworkInfo.ClrVersion;
        Assert.Contains("v8.0.", clr);
    }
        
    [Fact]
    public void CurrentRuntimeDirectoryShouldNotBeAppDirectory()
    {
        var dir = FrameworkInfo.RuntimeDirectory;
        Assert.NotEqual(ApplicationInfo.ApplicationDirectory, dir);
    }

    [Fact]
    public void FrameworkVersionShouldBeNet8()
    {
        var core = FrameworkInfo.FrameworkVersion;
        Assert.Contains("8.0.", core);
            
#pragma warning disable CS0618 // Type or member is obsolete
        Assert.True(FrameworkInfo.IsNetCore);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public void ThisTestsShouldNotBeSelfHosted()
    {
        Assert.False(FrameworkInfo.IsSelfHosted);
    }

}