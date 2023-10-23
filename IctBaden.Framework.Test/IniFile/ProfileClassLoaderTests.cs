using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using IctBaden.Framework.IniFile;
using Xunit;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace IctBaden.Framework.Test.IniFile;

public class ProfileClassLoaderTests : IDisposable
{
    private class TestClass
    {
        public bool Boolean1 { get; set; }
        public bool BooleanTrue { get; set; }
        public int Integer { get; set; }
        public float Numeric1 { get; set; }
        public double Numeric2 { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<string> TextList { get; set; } = new();
        public string[] TextArray { get; set; } = Array.Empty<string>();
        public List<int> IntList { get; set; } = new();
    }

    private class TestNullableClass
    {
        public bool? Boolean1 { get; set; }
        public bool? BooleanTrue { get; set; }
        public int? Integer { get; set; }
        public float? Numeric1 { get; set; }
        public double? Numeric2 { get; set; }
        public string? Text { get; set; }
        public List<string>? TextList { get; set; }
        public string[]? TextArray { get; set; }
        public List<int>? IntList { get; set; }
    }
        
    private readonly string _testFileName;

    public ProfileClassLoaderTests()
    {
        _testFileName = Path.GetTempFileName();
        if (File.Exists(_testFileName))
        {
            File.Delete(_testFileName);
        }
    }

    public void Dispose()
    {
        if (File.Exists(_testFileName))
        {
            File.Delete(_testFileName);
        }
    }

    [Fact]
    public void ClassLoaderShouldDeserializeTypes()
    {
        using (var fileData = new StreamWriter(_testFileName, false, Encoding.Unicode))
        {
            fileData.WriteLine("[TestClass]");
            fileData.WriteLine("Boolean1=1");
            fileData.WriteLine("BooleanTrue=true");
            fileData.WriteLine("Integer=123");
            fileData.WriteLine("Numeric1=4.567");
            fileData.WriteLine("Numeric2=4.567");
            fileData.WriteLine("Text=a b c");
            fileData.WriteLine("TextList=a;b;c");
            fileData.WriteLine("TextArray=a;b;c");
            fileData.WriteLine("IntList=1;2;3");
            fileData.Close();
        }
        var ini = new Profile(_testFileName);
        var testClass = new TestClass();
        var loader = new ProfileClassLoader(CultureInfo.InvariantCulture);
        loader.LoadClass(testClass, ini);
            
        Assert.True(testClass.Boolean1);
        Assert.True(testClass.BooleanTrue);
        Assert.Equal(123, testClass.Integer);
        Assert.Equal(4.567f, testClass.Numeric1);
        Assert.Equal(4.567, testClass.Numeric2);
        Assert.Equal("a b c", testClass.Text);
        Assert.Equal(new List<string> {"a","b","c"}, testClass.TextList);
        Assert.Equal(new[] {"a","b","c"}, testClass.TextArray);
        Assert.Equal(new List<int> {1,2,3}, testClass.IntList);
    }
    
    
    [Fact]
    public void ClassLoaderShouldDeserializeNullableTypes()
    {
        using (var fileData = new StreamWriter(_testFileName, false, Encoding.Unicode))
        {
            fileData.WriteLine("[TestNullableClass]");
            fileData.WriteLine("Boolean1=1");
            fileData.WriteLine("BooleanTrue=true");
            fileData.WriteLine("Integer=123");
            fileData.WriteLine("Numeric1=4.567");
            fileData.WriteLine("Numeric2=4.567");
            fileData.WriteLine("Text=a b c");
            fileData.WriteLine("TextList=a;b;c");
            fileData.WriteLine("TextArray=a;b;c");
            fileData.WriteLine("IntList=1;2;3");
            fileData.Close();
        }
        var ini = new Profile(_testFileName);
        var testClass = new TestNullableClass();
        var loader = new ProfileClassLoader(CultureInfo.InvariantCulture);
        loader.LoadClass(testClass, ini);
            
        Assert.True(testClass.Boolean1);
        Assert.True(testClass.BooleanTrue);
        Assert.Equal(123, testClass.Integer);
        Assert.Equal(4.567f, testClass.Numeric1);
        Assert.Equal(4.567, testClass.Numeric2);
        Assert.Equal("a b c", testClass.Text);
        Assert.Equal(new List<string> {"a","b","c"}, testClass.TextList);
        Assert.Equal(new[] {"a","b","c"}, testClass.TextArray);
        Assert.Equal(new List<int> {1,2,3}, testClass.IntList);
    }

}