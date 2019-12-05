// ReSharper disable UnusedMember.Global

using System.Linq;

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
            if (inputValue < _points[0].Input)
            {
                return _points[0].Output;
            }

            int pos;
            for (pos = 1; pos < _points.Length; pos++)
            {
                if (inputValue < _points[pos].Input)
                    break;
            }

            return pos < _points.Length
                ? InterpolateLinear(_points[pos - 1], _points[pos], inputValue)
                : _points.Last().Output;
        }

        static long InterpolateLinear(InterpolationPoint point1, InterpolationPoint point2, long inputValue)
        {
            // Out = (In - In1) * (Out2 - Out1) / (In2 - In1) + Out1

            // Die Berechnungen erfolgen mit Integer-Werten
            // damit positive und negative Steigungen möglich sind.
            var outputValue = inputValue - point1.Input;
            outputValue *= point2.Output - point1.Output; // * (Out2 - Out1)
            outputValue /= point2.Input == point1.Input
                ? 1
                : point2.Input - point1.Input; // / (In2 - In1)
            outputValue += point1.Output; // + Out1

            return outputValue;
        }
    }
}