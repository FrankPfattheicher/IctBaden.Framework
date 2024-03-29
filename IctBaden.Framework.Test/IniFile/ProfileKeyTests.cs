using System;
using System.IO;
using System.Text;
using IctBaden.Framework.IniFile;
using Xunit;

namespace IctBaden.Framework.Test.IniFile;

public sealed class ProfileKeyTests : IDisposable
{
    private readonly string _testFileName;

    public ProfileKeyTests()
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
    public void ProfileKeyShouldBeGetAsInt()
    {
        const int expected = 123;
        using (var fileData = new StreamWriter(_testFileName, false, Encoding.Unicode))
        {
            fileData.WriteLine("");
            fileData.WriteLine("");
            fileData.WriteLine("[Test]");
            fileData.WriteLine($"Key1={expected}");
            fileData.Close();
        }
        var ini = new Profile(_testFileName);
        var section = ini.Sections["Test"];
            
        Assert.Equal(expected, section.Get<int>("Key1"));
    }

    [Fact]
    public void ProfileKeyShouldBeGetAsBool()
    {
        const bool expected = true;
        using (var fileData = new StreamWriter(_testFileName, false, Encoding.Unicode))
        {
            fileData.WriteLine("");
            fileData.WriteLine("");
            fileData.WriteLine("[Test]");
            fileData.WriteLine($"Key1={expected}");
            fileData.Close();
        }
        var ini = new Profile(_testFileName);
        var section = ini.Sections["Test"];
            
        Assert.Equal(expected, section.Get<bool>("Key1"));
    }

//        [Fact]
//        public void ProfileKeyShouldBeGetAsStringArray()
//        {
//            var expected = new string[] {"aaa", "bbb"};
//            using (var fileData = new StreamWriter(_testFileName, false, Encoding.Unicode))
//            {
//                fileData.WriteLine("");
//                fileData.WriteLine("");
//                fileData.WriteLine("[Test]");
//                fileData.WriteLine($"Key1={string.Join(";", expected)}");
//                fileData.Close();
//            }
//            var ini = new Profile(_testFileName);
//            var section = ini.Sections["Test"];
//            
//            Assert.Equal(expected, section.Get<string[]>("Key1"));
//        }


}