using System;
using System.IO;
using System.Text;
using IctBaden.Framework.IniFile;
using Xunit;

namespace IctBaden.Framework.Test.IniFile;

public sealed class ProfileValueTests : IDisposable
{
    private readonly string _testFileName;
    private readonly Profile _profile;
    private readonly ProfileSection _testSection;

    public ProfileValueTests()
    {
        _testFileName = Path.GetTempFileName();
        if (File.Exists(_testFileName))
        {
            File.Delete(_testFileName);
        }

        using (var fileData = new StreamWriter(_testFileName, false, Encoding.ASCII))
        {
            fileData.WriteLine("");
            fileData.WriteLine("[Test]");
            fileData.WriteLine("ExistingKey=");
            fileData.Close();
        }

        _profile = new Profile(_testFileName);
        _testSection = _profile["Test"];
    }

    public void Dispose()
    {
        if (File.Exists(_testFileName))
        {
            File.Delete(_testFileName);
        }
    }


    [Fact]
    public void ValueOfExistingKeyShouldBeEmptyString()
    {
        var value = _testSection["ExistingKey"].StringValue;
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void ValueOfNonExistingKeyShouldBeNull()
    {
        var value = _testSection["NonExistingKey"].StringValue;
        Assert.Null(value);
    }

    [Fact]
    public void WritingEmptyStringShouldWriteKeyName()
    {
        _testSection["NewKey"].StringValue = string.Empty;
        _profile.Save();

        var fileText = File.ReadAllText(_testFileName);
        Assert.Contains("NewKey=", fileText);
    }

    [Fact]
    public void WritingNullStringShouldRemoveKeyName()
    {
        _testSection["NewNullKey"].StringValue = null;
        _profile.Save();

        var fileText = File.ReadAllText(_testFileName);
        Assert.DoesNotContain("NewNullKey", fileText);
    }

    [Fact]
    public void DateTimeShouldBeSerializedIsoFormatted()
    {
        _testSection["DateTime"].ObjectValue = new DateTime(2022, 11, 30, 15, 54, 0);
        _profile.Save();

        var fileText = File.ReadAllText(_testFileName);
        Assert.Contains("DateTime=2022-11-30T15:54:00", fileText);
    }

    [Fact]
    public void DateTimeOffsetShouldBeSerializedIsoFormatted()
    {
        _testSection["DateTimeOffset"].ObjectValue = new DateTimeOffset(2022, 11, 30, 15, 54, 1, TimeSpan.Zero);
        _profile.Save();

        var fileText = File.ReadAllText(_testFileName);
        Assert.Contains("DateTimeOffset=2022-11-30T15:54:01", fileText);
    }

}
