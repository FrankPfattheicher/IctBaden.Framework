using IctBaden.Framework.Arithmetic;
using Xunit;

namespace IctBaden.Framework.Test.Arithmetic;

public class HysteresisTests
{
    [Fact]
    public void OutputShouldNotBeSetValueRisingAboveLower()
    {
        var hysteresis = Hysteresis.FromRange(100, 200);

        var output = hysteresis.Get(50);
        Assert.False(output);

        output = hysteresis.Get(150);
        Assert.False(output);
    }
    
    [Fact]
    public void OutputShouldBeSetValueRisingAboveUpper()
    {
        var hysteresis = Hysteresis.FromRange(100, 200);

        var output = hysteresis.Get(50);
        Assert.False(output);

        output = hysteresis.Get(210);
        Assert.True(output);
    }
    
    [Fact]
    public void OutputShouldNotBeResetValueFallingAboveLower()
    {
        var hysteresis = Hysteresis.FromRange(100, 200);

        var output = hysteresis.Get(250);
        Assert.True(output);
        
        output = hysteresis.Get(110);
        Assert.True(output);
    }

    [Fact]
    public void OutputShouldBeResetValueFallingBelowLower()
    {
        var hysteresis = Hysteresis.FromRange(100, 200);

        var output = hysteresis.Get(250);
        Assert.True(output);
        
        output = hysteresis.Get(90);
        Assert.False(output);
    }

    [Fact]
    public void OutputShouldStayLowWithinRange()
    {
        var hysteresis = Hysteresis.FromRange(100, 200);

        var output = hysteresis.Get(50);
        Assert.False(output);
        
        output = hysteresis.Get(110);
        Assert.False(output);
        
        output = hysteresis.Get(150);
        Assert.False(output);

        output = hysteresis.Get(190);
        Assert.False(output);

        output = hysteresis.Get(150);
        Assert.False(output);
        
        output = hysteresis.Get(110);
        Assert.False(output);
    }

    [Fact]
    public void OutputShouldStayHighWithinRange()
    {
        var hysteresis = Hysteresis.FromRange(100, 200);

        var output = hysteresis.Get(250);
        Assert.True(output);
        
        output = hysteresis.Get(110);
        Assert.True(output);
        
        output = hysteresis.Get(150);
        Assert.True(output);

        output = hysteresis.Get(190);
        Assert.True(output);

        output = hysteresis.Get(150);
        Assert.True(output);
        
        output = hysteresis.Get(110);
        Assert.True(output);
    }

}