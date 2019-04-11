using IctBaden.Framework.Arithmetic;
using Xunit;

namespace IctBaden.Framework.Test.Arithmetic
{
    public class ReverseBitsTests
    {
        [Fact]
        public void ReverseByte()
        {
            var by = Binary.ReverseBits(0x5A);
            Assert.Equal(0x5A, by);
        }

        [Fact]
        public void ReverseWord()
        {
            var word = Binary.ReverseBits(0xA5EF);
            Assert.Equal(0xF7A5, word);
        }

    }
}
