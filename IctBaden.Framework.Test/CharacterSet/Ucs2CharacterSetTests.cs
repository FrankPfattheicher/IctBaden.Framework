using IctBaden.Framework.CharacterSet;
using Xunit;
// ReSharper disable StringLiteralTypo

namespace IctBaden.Framework.Test.CharacterSet
{
    public class Ucs2CharacterSetTests
    {
        [Fact]
        public void ToUcs2En()
        {
            const string text = "Hello";
            const string encoded = "00480065006C006C006F";

            var actual = Ucs2CharacterSet.EncodeText(text);

            Assert.Equal(encoded, actual);
        }

        [Fact]
        public void ToUcs2De()
        {
            const string text = "ÄÖÜäöüß@";
            const string encoded = "00C400D600DC00E400F600FC00DF0040";

            var actual = Ucs2CharacterSet.EncodeText(text);

            Assert.Equal(encoded, actual);
        }

        [Fact]
        public void ToUcs2Ar()
        {
            const string text = "مرحبًا عالم !";
            const string encoded = "06450631062D0628064B06270020063906270644064500200021";

            var actual = Ucs2CharacterSet.EncodeText(text);

            Assert.Equal(encoded, actual);
        }

        [Fact]
        public void FromUcs2En()
        {
            const string text = "Hello";
            const string encoded = "00480065006C006C006F";

            var actual = Ucs2CharacterSet.DecodeText(encoded);

            Assert.Equal(text, actual);
        }

        [Fact]
        public void FromUcs2De()
        {
            const string text = "ÄÖÜäöüß@";
            const string encoded = "00C400D600DC00E400F600FC00DF0040";

            var actual = Ucs2CharacterSet.DecodeText(encoded);

            Assert.Equal(text, actual);
        }

        [Fact]
        public void FromUcs2Ar()
        {
            const string text = "مرحبًا عالم !";
            const string encoded = "06450631062D0628064B06270020063906270644064500200021";

            var actual = Ucs2CharacterSet.DecodeText(encoded);

            Assert.Equal(text, actual);
        }

    }
}