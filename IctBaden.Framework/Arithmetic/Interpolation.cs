
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.Arithmetic
{
    public class Interpolation
    {
        private readonly InterpolationPoint[] _points;

        public Interpolation(InterpolationPoint[] interpolationPoints)
        {
            _points = interpolationPoints;
        }

        public long InterpolateLinear(long inputValue)
        {
            // zu verwendende Wertepaare suchen
            int pos;
            for (pos = 1; pos < _points.Length; pos++)
            {
                if (inputValue < _points[pos].Input)
                    break;
            }

            return InterpolateLinear(_points[pos - 1], _points[pos], inputValue);
        }

        static long InterpolateLinear(InterpolationPoint point1, InterpolationPoint point2, long inputValue)
        {
            // Out = (In - In1) * (Out2 - Out1) / (In2 - In1) + Out1

            // Die Berechnungen erfolgen mit Integer-Werten
            // damit positive und negative Steigungen möglich sind.
            var outputValue = inputValue - point1.Input;
            outputValue *= point2.Output - point1.Output;   // * (Out2 - Out1)
            outputValue /= point2.Input - point1.Input;   // / (In2 - In1)
            outputValue += point1.Output;                                     // + Out1

            return outputValue;
        }
    }
}
