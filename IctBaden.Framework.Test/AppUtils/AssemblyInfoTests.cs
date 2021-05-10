using IctBaden.Framework.AppUtils;
using IctBaden.Framework.TestLib;
using Xunit;

namespace IctBaden.Framework.Test.AppUtils
{
    public class AssemblyInfoTests
    {
        private readonly AssemblyInfo _info;
        
        public AssemblyInfoTests()
        {
            var assembly = typeof(TestLibClass).Assembly;
            _info = new AssemblyInfo(assembly);
        }

        [Fact]
        public void GivenAssemblyTitleShouldReturnText() => Assert.Equal("IctBaden.Framework.TestLib", _info.Title);

        [Fact]
        public void GivenAssemblyCompanyShouldReturnText() => Assert.Equal("TestLib-Company", _info.CompanyName);

        [Fact]
        public void NotGivenTrademarkShouldReturnEmptyString() => Assert.Equal("", _info.Trademark);

    }
}