using IctBaden.Framework.AppUtils;
using Xunit;

namespace IctBaden.Framework.Test.AppUtils;

public class FrameworkInfoTests
{
    [Fact]
    public void CurrentClrVersionShouldBe10X()
    {
        var clr = FrameworkInfo.ClrVersion;
        Assert.Contains("v10.0.", clr);
    }
        
    [Fact]
    public void CurrentRuntimeDirectoryShouldNotBeAppDirectory()
    {
        var dir = FrameworkInfo.RuntimeDirectory;
        Assert.NotEqual(ApplicationInfo.ApplicationDirectory, dir);
    }

    [Fact]
    public void FrameworkVersionShouldBeNet10()
    {
        var core = FrameworkInfo.FrameworkVersion;
        Assert.Contains("10.0.", core);
            
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