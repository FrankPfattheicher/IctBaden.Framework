using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IctBaden.Framework.Types;
using Xunit;
// ReSharper disable UnusedMember.Local

namespace IctBaden.Framework.Test.Types;

public class ConverterTests
{
    private enum TestEnum
    {
        Zero,
        One,
        Two,
        Three,
        Four
    }
        
        
    [Fact]
    public void UniversalConverterShouldConvertIntegersToBoolean()
    {
        Assert.False((bool) UniversalConverter.ConvertToType(0, typeof(bool))!);
        Assert.True((bool) UniversalConverter.ConvertToType(1, typeof(bool))!);
    }

    [Fact]
    public void UniversalConverterShouldConvertStringsToBoolean()
    {
        Assert.False((bool) UniversalConverter.ConvertToType("0", typeof(bool))!);
        Assert.False((bool) UniversalConverter.ConvertToType("N", typeof(bool))!);
        Assert.False((bool) UniversalConverter.ConvertToType("F", typeof(bool))!);
        Assert.False((bool) UniversalConverter.ConvertToType("false", typeof(bool))!);
        Assert.False((bool) UniversalConverter.ConvertToType("False", typeof(bool))!);
        Assert.False((bool) UniversalConverter.ConvertToType("X", typeof(bool))!);
        Assert.False((bool) UniversalConverter.ConvertToType("", typeof(bool))!);

        Assert.True((bool) UniversalConverter.ConvertToType("1", typeof(bool))!);
        Assert.True((bool) UniversalConverter.ConvertToType("true", typeof(bool))!);
        Assert.True((bool) UniversalConverter.ConvertToType("True", typeof(bool))!);
        Assert.True((bool) UniversalConverter.ConvertToType("Y", typeof(bool))!);
        Assert.True((bool) UniversalConverter.ConvertToType("J", typeof(bool))!);
        Assert.True((bool) UniversalConverter.ConvertToType("T", typeof(bool))!);
    }
        
    [Fact]
    public void UniversalConverterShouldConvertListsToArrays()
    {
        var list = new List<int> {1, 2, 3, 4, 5};
        var array = list.ToArray();
            
        Assert.Equal(array, UniversalConverter.ConvertToType(list, typeof(int[])));
    }

    [Fact]
    public void UniversalConverterShouldConvertArraysToLists()
    {
        var list = new List<int> {1, 2, 3, 4, 5};
        var array = list.ToArray();
            
        Assert.Equal(list, UniversalConverter.ConvertToType(array, typeof(List<int>)));
    }

    [Fact]
    public void UniversalConverterShouldConvertElementTypes()
    {
        var intList = new List<int> {1, 2, 3, 4, 5};
        var stringList = intList.Select(e => e.ToString()).ToList();
            
        Assert.Equal(stringList, UniversalConverter.ConvertToType(intList, typeof(List<string>)));
    }
        
    [Fact]
    public void UniversalConverterShouldConvertIntegersToEnums()
    {
        Assert.Equal(TestEnum.Three, UniversalConverter.ConvertToType(3, typeof(TestEnum)));
    }
        
    [Fact]
    public void UniversalConverterShouldConvertUlongObjectToUlong()
    {
        var expected = (object)(ulong)123;
        var value = UniversalConverter.ConvertTo<ulong>(expected);
        Assert.Equal((ulong)expected, value);
    }

    [Fact]
    public void UniversalConverterShouldConvertStringObjectToUlong()
    {
        var expected = (object)"123";
        var value = UniversalConverter.ConvertTo<ulong>(expected);
        Assert.Equal((ulong)123, value);
    }

    [Fact]
    public void UniversalConverterShouldConvertUIntToUlong()
    {
        var expected = (object)(uint)123;
        var value = UniversalConverter.ConvertTo<ulong>(expected);
        Assert.Equal((uint)expected, value);
    }

    [Fact]
    public void UniversalConverterShouldConvertDoubleToFloat()
    {
        var expected = (object)44.439999999999998;
        var value = (float)(UniversalConverter.ConvertToType(expected,typeof(float), CultureInfo.InvariantCulture) ?? 0.0f);
        Assert.Equal((double)expected, value, 2, MidpointRounding.AwayFromZero);
    }

        [Fact]
        public void UniversalConverterShouldConvertStringToBool()
        {
            var value = (bool)(UniversalConverter.ConvertToType("0", typeof(bool), new CultureInfo("DE-de")) ?? true);
            Assert.False(value);
            
            value = (bool)(UniversalConverter.ConvertToType("1", typeof(bool), new CultureInfo("DE-de")) ?? false);
            Assert.True(value);
        }

}