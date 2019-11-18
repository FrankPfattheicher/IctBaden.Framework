using IctBaden.Framework.Arithmetic;
using Xunit;

namespace IctBaden.Framework.Test.Arithmetic
{
    public class InterpolationTests
    {
        [Fact]
        public void InterpolationLongShouldReturnHalf()
        {
            var points = new[]
            {
                new InterpolationPoint {Input = 0, Output = 0},
                new InterpolationPoint {Input = 100, Output = 0x10000}
            };

            const long expected = 0x10000 / 2;

            var interpolation = new Interpolation(points);
            var value = interpolation.InterpolateLinear(50);
            
            Assert.Equal(expected, value);
        }
        
        [Fact]
        public void InterpolationDoubleShouldReturnHalf()
        {
            var points = new[]
            {
                new InterpolationPointDouble {Input = 0, Output = 0.0},
                new InterpolationPointDouble {Input = 100, Output = 100000.8}
            };

            const double expected = 100000.8 / 2.0;
            
            var interpolation = new InterpolationDouble(points);
            var value = interpolation.InterpolateLinear(50);
            
            Assert.Equal(expected, value, 4);
        }
        
    }
}