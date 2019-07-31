using IctBaden.Framework.Resource;
using Xunit;

namespace IctBaden.Framework.Test.Resource
{
    public class ResourceLoaderTests
    {
        [Fact]
        public void LoadingWithGivenAssemblyAndExactNameShouldSucceed()
        {
            var text = ResourceLoader.LoadString(GetType().Assembly, "IctBaden.Framework.Test.Resource.TextFile1.txt");
            Assert.NotNull(text);
        }

        [Fact]
        public void LoadingOnlyBaseNameShouldSucceed()
        {
            var text = ResourceLoader.LoadString("TextFile1.txt");
            Assert.NotNull(text);
        }

        [Fact]
        public void LoadingWithDifferentCaseShouldSucceed()
        {
            var text = ResourceLoader.LoadString("textfile1.TXT");
            Assert.NotNull(text);
        }

        [Fact]
        public void LoadingShouldContainExpectedText()
        {
            const string expected = "I am a resource.";
            var text = ResourceLoader.LoadString("TextFile1.txt");
            Assert.Equal(expected, text);
        }

    }
}
