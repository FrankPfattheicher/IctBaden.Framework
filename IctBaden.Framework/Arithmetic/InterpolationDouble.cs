// ReSharper disable UnusedMember.Global

using System;
using System.Linq;

namespace IctBaden.Framework.Arithmetic
{
    public class InterpolationDouble
    {
        private readonly InterpolationPointDouble[] _points;

        public InterpolationDouble(InterpolationPointDouble[] interpolationPoints)
        {
            _points = interpolationPoints;
        }

        public double InterpolateLinear(double inputValue)
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

        static double InterpolateLinear(InterpolationPointDouble point1, InterpolationPointDouble point2,
            double inputValue)
        {
            // Out = (In - In1) * (Out2 - Out1) / (In2 - In1) + Out1

            var outputValue = inputValue - point1.Input;
            outputValue *= point2.Output - point1.Output; // * (Out2 - Out1)
            outputValue /= Math.Abs(point2.Input - point1.Input) < .001
                ? 1.0
                : point2.Input - point1.Input; // / (In2 - In1)
            outputValue += point1.Output; // + Out1

            return outputValue;
        }
    }
}