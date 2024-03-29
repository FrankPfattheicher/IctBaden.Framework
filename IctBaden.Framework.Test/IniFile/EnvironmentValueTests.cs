using System;
using System.Globalization;
using System.IO;
using System.Text;
using IctBaden.Framework.IniFile;
using Xunit;

namespace IctBaden.Framework.Test.IniFile;

public sealed class EnvironmentValueTests : IDisposable
{
    private readonly string _testFileName;

    public EnvironmentValueTests()
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
    public void ClassLoaderShouldReplaceEnvironmentVariables()
    {
        using (var fileData = new StreamWriter(_testFileName, false, Encoding.Unicode))
        {
            fileData.WriteLine("[TestClass]");
            fileData.WriteLine("Text=a %TEST_VAL% c");
            fileData.Close();
        }
        
        Environment.SetEnvironmentVariable("TEST_VAL", "B");
        var ini = new Profile(_testFileName);
        var testClass = new TestClass();
        var loader = new ProfileClassLoader(CultureInfo.InvariantCulture, true);
        loader.LoadClass(testClass, ini);
            
        Assert.Equal("a B c", testClass.Text);
    }

    [Fact]
    public void ClassLoaderShouldReplaceNestedEnvironmentVariables()
    {
        using (var fileData = new StreamWriter(_testFileName, false, Encoding.Unicode))
        {
            fileData.WriteLine("[TestClass]");
            fileData.WriteLine("Text=%TEST_VAL%");
            fileData.Close();
        }
        
        Environment.SetEnvironmentVariable("NESTED_VAL", "bbb");
        Environment.SetEnvironmentVariable("TEST_VAL", "A %NESTED_VAL% C");
        var ini = new Profile(_testFileName);
        var testClass = new TestClass();
        var loader = new ProfileClassLoader(CultureInfo.InvariantCulture, true);
        loader.LoadClass(testClass, ini);
            
        Assert.Equal("A bbb C", testClass.Text);
    }

    [Fact]
    public void ClassLoaderShouldReplaceEnvironmentVariablesInSections()
    {
        using (var fileData = new StreamWriter(_testFileName, false, Encoding.Unicode))
        {
            fileData.WriteLine("[TestSection]");
            fileData.WriteLine("Text=a %TEST_VAL% c");
            fileData.WriteLine("[Section1]");
            fileData.WriteLine("Text=%TEST_VAL% C D");
            fileData.WriteLine("[Section2]");
            fileData.WriteLine("Text=@ A %TEST_VAL%");
            fileData.Close();
        }
        
        Environment.SetEnvironmentVariable("TEST_VAL", "B");
        var ini = new Profile(_testFileName);
        var testClass = new TestClass();
        var loader = new ProfileClassLoader(CultureInfo.InvariantCulture, true);
        loader.LoadClass(testClass, ini, "TestSection");
            
        Assert.Equal("a B c", testClass.Text);
        Assert.Equal("B C D", testClass.Section1.Text);
        Assert.Equal("@ A B", testClass.Section2.Text);
    }

}