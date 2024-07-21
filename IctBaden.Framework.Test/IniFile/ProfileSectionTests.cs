using IctBaden.Framework.IniFile;
using Xunit;

namespace IctBaden.Framework.Test.IniFile;

public class ProfileSectionTests
{
    private readonly Profile _profile = new("TestCfg.ini");

    [Fact]
    public void SectionCountsShouldMatchCountGivenInSectionCountEntry()
    {
        var expected = _profile[ProfileSection.UnnamedGlobalSectionName]["SectionCount"].LongValue;
        Assert.Equal(expected, _profile.Sections.Count);
    }

    [Fact]
    public void SectionKeyCountsShouldMatchCountGivenInKeyCountEntry()
    {
        foreach (var section in _profile.Sections)
        {
            var expected = section["KeyCount"].LongValue;
            Assert.Equal(expected, section.Keys.Count);
        }
    }

    [Fact]
    public void DefaultSectionShouldSetAndGetValues()
    {
        var expected = "123";
        var section = new ProfileSection(_profile, null);
        section.Set("test", expected);
        var value = section.Get<string>("test");
            
        Assert.Equal(expected, value);
    }

}