using IctBaden.Framework.IniFile;
using Xunit;

namespace IctBaden.Framework.Test.IniFile
{
    public class ProfileSectionTests
    {
        private readonly Profile _profile;

        public ProfileSectionTests()
        {
            _profile = new Profile("TestCfg.ini");
        }

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

    }
}
