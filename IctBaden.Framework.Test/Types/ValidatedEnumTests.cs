using IctBaden.Framework.Types;
using Xunit;

namespace IctBaden.Framework.Test.Types;

public class ValidatedEnumTests
{
    [Fact]
    public void NullShouldHaveNoValue()
    {
        var ve = new ValidatedEnum<TestEnum>(null);
            
        Assert.False(ve.IsValid);
        Assert.False(ve.HasValue);
    }
        
    [Fact]
    public void EnumShouldSucceedFromValue()
    {
        var ve = new ValidatedEnum<TestEnum>(2);
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestEnum.Two, ve.Enumeration);
    }
        
    [Fact]
    public void EnumShouldSucceedFromString()
    {
        var ve = new ValidatedEnum<TestEnum>("2");
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestEnum.Two, ve.Enumeration);
    }
        
    [Fact]
    public void EnumShouldSucceedFromEnum()
    {
        var ve = new ValidatedEnum<TestEnum>(TestEnum.Two);
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestEnum.Two, ve.Enumeration);
    }
        
    [Fact]
    public void FlagsShouldSucceedFromValue()
    {
        var ve = new ValidatedEnum<TestFlags>(4);
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestFlags.Four, ve.Enumeration);
    }
        
    [Fact]
    public void FlagsShouldSucceedFromString()
    {
        var ve = new ValidatedEnum<TestFlags>("4");
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestFlags.Four, ve.Enumeration);
    }
        
    [Fact]
    public void FlagsShouldSucceedFromEnum()
    {
        var ve = new ValidatedEnum<TestFlags>(TestFlags.Eight);
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestFlags.Eight, ve.Enumeration);
    }
        
    [Fact]
    public void FlagsShouldSucceedFromMultipleValue()
    {
        var ve = new ValidatedEnum<TestFlags>(5);
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestFlags.One | TestFlags.Four, ve.Enumeration);
    }
        
    [Fact]
    public void FlagsShouldSucceedFromMultipleString()
    {
        var ve = new ValidatedEnum<TestFlags>("One | Eight");
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestFlags.One | TestFlags.Eight, ve.Enumeration);
            
        ve = new ValidatedEnum<TestFlags>("One, Eight");
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestFlags.One | TestFlags.Eight, ve.Enumeration);
    }
        
    [Fact]
    public void FlagsShouldSucceedFromMultipleFlags()
    {
        var ve = new ValidatedEnum<TestFlags>(TestFlags.One | TestFlags.Eight);
            
        Assert.True(ve.IsValid);
        Assert.True(ve.HasValue);
        Assert.Equal(TestFlags.One | TestFlags.Eight, ve.Enumeration);
    }
        
}