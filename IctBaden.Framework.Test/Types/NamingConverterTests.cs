using IctBaden.Framework.Types;
using Xunit;

namespace IctBaden.Framework.Test.Types
{
    public class NamingConverterTests
    {
        private const string Pascal = "ThisIsTheTestString";
        private const string Camel = "thisIsTheTestString";
        private const string Kebab = "this-is-the-test-string";
        private const string Snake = "this_is_the_test_string";
        
        [Fact]
        public void UpperPascalToKebab()
        {
            var result = NamingConverter.PascalToKebabCase(Pascal);
            Assert.Equal(Kebab, result);
        }
        
        [Fact]
        public void LowerPascalToKebab()
        {
            var result = NamingConverter.PascalToKebabCase(Camel);
            Assert.Equal(Kebab, result);
        }

        [Fact]
        public void LowerPascalToSnake()
        {
            var result = NamingConverter.PascalToSnakeCase(Camel);
            Assert.Equal(Snake, result);
        }

        [Fact]
        public void KebabToPascal()
        {
            var result = NamingConverter.KebabToPascalCase(Kebab);
            Assert.Equal(Pascal, result);
        }

        [Fact]
        public void SnakeToPascal()
        {
            var result = NamingConverter.SnakeToPascalCase(Snake);
            Assert.Equal(Pascal, result);
        }

        [Fact]
        public void UpperPascalToLowerPascal()
        {
            var result = NamingConverter.ToCamelCase(Pascal);
            Assert.Equal(Camel, result);
        }

        [Fact]
        public void LowerPascalUpperToPascal()
        {
            var result = NamingConverter.ToPascalCase(Camel);
            Assert.Equal(Pascal, result);
        }

        [Fact]
        public void ConvertTextToPascalIdentifier()
        {
            const string expected = "ThisIsThe1stIdentifier";
            var texts = new[]
            {
                "THIS_IS_THE_1ST_IDENTIFIER",
                "This is the 1st identifier!!!",
                "This is the 1st identifier.",
                "This, is the 1st identifier?",
                "This! is the 1st-identifier"
            };
            foreach (var text in texts)
            {
                var result = NamingConverter.ToPascalIdentifier(text);
                Assert.Equal(expected, result);
            }
        }

    }
}