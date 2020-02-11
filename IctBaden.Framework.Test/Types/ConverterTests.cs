using System.Collections.Generic;
using System.Linq;
using IctBaden.Framework.Types;
using Xunit;
// ReSharper disable UnusedMember.Local

namespace IctBaden.Framework.Test.Types
{
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
            Assert.False((bool) UniversalConverter.ConvertToType(0, typeof(bool)));
            Assert.True((bool) UniversalConverter.ConvertToType(1, typeof(bool)));
        }

        [Fact]
        public void UniversalConverterShouldConvertStringsToBoolean()
        {
            Assert.False((bool) UniversalConverter.ConvertToType("0", typeof(bool)));
            Assert.False((bool) UniversalConverter.ConvertToType("N", typeof(bool)));
            Assert.False((bool) UniversalConverter.ConvertToType("F", typeof(bool)));
            Assert.False((bool) UniversalConverter.ConvertToType("false", typeof(bool)));
            Assert.False((bool) UniversalConverter.ConvertToType("False", typeof(bool)));
            Assert.False((bool) UniversalConverter.ConvertToType("X", typeof(bool)));
            Assert.False((bool) UniversalConverter.ConvertToType("", typeof(bool)));

            Assert.True((bool) UniversalConverter.ConvertToType("1", typeof(bool)));
            Assert.True((bool) UniversalConverter.ConvertToType("true", typeof(bool)));
            Assert.True((bool) UniversalConverter.ConvertToType("True", typeof(bool)));
            Assert.True((bool) UniversalConverter.ConvertToType("Y", typeof(bool)));
            Assert.True((bool) UniversalConverter.ConvertToType("J", typeof(bool)));
            Assert.True((bool) UniversalConverter.ConvertToType("T", typeof(bool)));
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
        
        
    }
}