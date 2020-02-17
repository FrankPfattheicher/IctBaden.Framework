using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using IctBaden.Framework.IniFile;
using Xunit;
// ReSharper disable StringLiteralTypo

namespace IctBaden.Framework.Test.IniFile
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    public class ProfileTests : IDisposable
    {
        private readonly string _testFileName;

        public ProfileTests()
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
        public void ProfileUtilsNeedsUnicode()
        {
            Assert.False(Profile.NeedsUnicode("Stoerung"), "Kein Unicode erforderlich");
            Assert.True(Profile.NeedsUnicode("Störung"), "Unicode erforderlich");
            Assert.True(Profile.NeedsUnicode("Привет"), "Unicode erforderlich");
        }

        [Fact]
        public void ProfileLoadUnicode()
        {
            using (var fileData = new StreamWriter(_testFileName, false, Encoding.Unicode))
            {
                fileData.WriteLine("");
                fileData.WriteLine("");
                fileData.WriteLine("[Test]");
                fileData.WriteLine("Key1=123");
                fileData.WriteLine("Key2=Привет");
                fileData.Close();
            }
            var ini = new Profile(_testFileName);
            var sectionCount = ini.Sections.Contains(ProfileSection.UnnamedGlobalSectionName) ? 2 : 1;
            Assert.True(ini.Sections.Count == sectionCount, "Datei muss genau eine Section haben");
            Assert.True(ini.Sections["Test"].Keys.Count == 2, "Die Section muss genau zwei Keys haben");
            Assert.Equal(Encoding.Unicode, ini.FileEncoding);

            File.Delete(_testFileName);
        }

        [Fact]
        public void ProfileLoadAscii()
        {
            using (var fileData = new StreamWriter(_testFileName, false, Encoding.ASCII))
            {
                fileData.WriteLine("");
                fileData.WriteLine("");
                fileData.WriteLine("[Test]");
                fileData.WriteLine("Key1=123");
                fileData.WriteLine("Key2=456");
                fileData.Close();
            }
            var ini = new Profile(_testFileName);
            var sectionCount = ini.Sections.Contains(ProfileSection.UnnamedGlobalSectionName) ? 2 : 1;
            Assert.True(ini.Sections.Count == sectionCount, "Datei muss genau eine Section haben");
            Assert.True(ini.Sections["Test"].Keys.Count == 2, "Die Section muss genau zwei Keys haben");

            File.Delete(_testFileName);
        }

        [Fact]
        public void ProfileLoadDefaultEncoding()
        {
            using (var fileData = new StreamWriter(_testFileName, false, Encoding.Default))
            {
                fileData.WriteLine("");
                fileData.WriteLine("");
                fileData.WriteLine("[Test]");
                fileData.WriteLine("Key1=123");
                fileData.WriteLine("Key2=456");
                fileData.Close();
            }
            var ini = new Profile(_testFileName);
            var sectionCount = ini.Sections.Contains(ProfileSection.UnnamedGlobalSectionName) ? 2 : 1;
            Assert.True(ini.Sections.Count == sectionCount, "Datei muss genau eine Section haben");
            Assert.True(ini.Sections["Test"].Keys.Count == 2, "Die Section muss genau zwei Keys haben");
            Assert.Equal(Encoding.Default, ini.FileEncoding);
        }

        [Fact]
        public void ProfileCreate()
        {
            var ini = new Profile(_testFileName);
            ini["Test"]["Wert"].StringValue = "1234";
            Assert.True(File.Exists(_testFileName), "Datei muss erzeugt werden");
        }

        [Fact]
        public void SectionNamesAreCaseInsensitive()
        {
            var ini = new Profile(_testFileName);
            ini["Test"]["Wert"].StringValue = "1234";
            
            Assert.True(ini.Sections.Contains("Test"));
            Assert.True(ini.Sections.Contains("test"));
            Assert.True(ini.Sections.Contains("TEST"));

            Assert.True(ini.HasSection("Test"));
            Assert.True(ini.HasSection("test"));
            Assert.True(ini.HasSection("TEST"));

            Assert.True(ini["Test"].Keys.Contains("Wert"));
            Assert.True(ini["test"].Keys.Contains("Wert"));
            Assert.True(ini["TEST"].Keys.Contains("Wert"));
        }

        [Fact]
        public void KeyNamesAreCaseInsensitive()
        {
            var ini = new Profile(_testFileName);
            ini["Test"]["Wert"].StringValue = "1234";
            var section = ini.Sections["Test"];

            Assert.True(section.Contains("Wert"));
            Assert.True(section.Contains("wert"));
            Assert.True(section.Contains("WERT"));

            Assert.Equal("1234", section.Get("Wert", string.Empty));
            Assert.Equal("1234", section.Get("wert", string.Empty));
            Assert.Equal("1234", section.Get("WERT", string.Empty));
        }

        [Fact]
        public void SerializeClass()
        {
            var obj = new TestObject
            {
                Numeric = 1234,
                Float = 5.678,
                Boolean = true,
                Text  = "SerializeClass"
            };
            var ini = new Profile(_testFileName);
            ProfileObjectSerializer.Save(obj, ini);
            Assert.True(File.Exists(_testFileName), "Datei muss erzeugt werden");

            var iniContent = File.ReadAllText(_testFileName);
            Assert.Contains("[TestObject]", iniContent);
            Assert.Contains("Numeric=1234", iniContent);
            Assert.Contains("Float=5.678", iniContent);
            Assert.Contains("Boolean=True", iniContent);
            Assert.Contains("Text=SerializeClass", iniContent);
        }

        [Fact]
        public void DeserializeClass()
        {
            File.WriteAllText(_testFileName, @"[TestObject]
Numeric=1234
Float=5.678
Boolean=True
Text=DeserializeClass
");
            var ini = new Profile(_testFileName);
            var obj = new TestObject();
            ProfileObjectSerializer.Load(obj, ini);

            var section = ini["TestObject"];
            Assert.Equal(1234, section["Numeric"].LongValue);
            Assert.Equal(5.678, section["Float"].DoubleValue);
            Assert.True(section["Boolean"].BoolValue);
            Assert.Equal("DeserializeClass", section["Text"].StringValue);
        }

        [PerformanceFact]
        public void SaveProfileShouldBeFasterThan100Milliseconds()
        {
            // create large file
            var ini = new Profile(_testFileName);

            var watch = new Stopwatch();
            watch.Start();

            for (var s = 1; s <= 10; s++)
            {
                var section = ini["Section" + s];
                for (var i = 1; i <= 10; i++)
                {
                    section["Item" + i].StringValue = "Test Save Profile Speed";
                }
            }
            
            watch.Stop();
            var elapsed = watch.ElapsedMilliseconds;
            Assert.True(elapsed < 100, $"Duration = {elapsed}ms");
        }
        

    }
}
