using System;
using IctBaden.Framework.IniFile;
using Xunit;

namespace IctBaden.Framework.Test
{
    public class ProfileSectionTests
    {
        [Fact]
        public void SectionCountsShouldMatchCountGivenInSectionCountEntry()
        {
            var profile = new Profile("TestCfg.ini");
            var expected = profile[ProfileSection.UnnamedGlobalSectionName]["SectionCount"].LongValue;
            Assert.Equal(expected, profile.Sections.Count);
        }

        [Fact]
        public void SectionKeyCountsShouldMatchCountGivenInKeyCountEntry()
        {
            var profile = new Profile("TestCfg.ini");

            foreach (var section in profile.Sections)
            {
                var expected = section["KeyCount"].LongValue;
                Assert.Equal(expected, section.Keys.Count);
            }
        }

    }
}
