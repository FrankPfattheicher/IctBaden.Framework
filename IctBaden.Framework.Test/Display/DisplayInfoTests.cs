using IctBaden.Framework.UserInterface;
using Xunit;

namespace IctBaden.Framework.Test;

public class DisplayInfoTests
{
    [Fact]
    public void GetMonitorsSholudReturnMinOne()
    {
        var monitors = DisplayInfo.GetMonitorCount();
        Assert.True(monitors > 0);
    }

    [Fact]
    public void GetScalingFactorSholudReturnMinOne()
    {
        var scalingFactor = DisplayInfo.GetScalingFactor(1);
        Assert.True(scalingFactor >= 1.0);
    }

    [Fact]
    public void GetVirtualScreenSholudNotBeEmpty()
    {
        var virtualScreen = DisplayInfo.GetVirtualScreen();
        Assert.False(virtualScreen.IsEmpty);
    }

}