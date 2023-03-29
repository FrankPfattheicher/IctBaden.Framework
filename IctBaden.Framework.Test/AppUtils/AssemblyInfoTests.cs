using System.IO;
using IctBaden.Framework.AppUtils;
using IctBaden.Framework.TestLib;
using Xunit;

namespace IctBaden.Framework.Test.AppUtils
{
    public class AssemblyInfoTests
    {
        private readonly AssemblyInfo _info;
        private readonly string _path;
        private readonly string _baseName;
        
        public AssemblyInfoTests()
        {
            var assembly = typeof(TestLibClass).Assembly;
            _path = Path.GetDirectoryName(assembly.Location);
            _baseName = Path.GetFileNameWithoutExtension(assembly.Location);
            _info = new AssemblyInfo(assembly);
        }

        [Fact]
        public void GivenAssemblyShouldReturnExePath() => Assert.Equal(_path, _info.ExePath);

        [Fact]
        public void GivenAssemblyShouldReturnExeBaseName() => Assert.Equal(_baseName, _info.ExeBaseName);

        
        
        [Fact]
        public void GivenAssemblyTitleShouldReturnText() => Assert.Equal("TestLib", _info.Title);

        [Fact]
        public void GivenAssemblyCompanyShouldReturnText() => Assert.Equal("TestLib-Company", _info.CompanyName);

        [Fact]
        public void NotGivenTrademarkShouldReturnEmptyString() => Assert.Equal("TestLib-Trademark", _info.Trademark);

        [Fact]
        public void GivenAssemblyCopyrightShouldReturnText() => Assert.Equal("2021 ICT Baden", _info.Copyright);
    }
}