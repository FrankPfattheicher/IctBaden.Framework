﻿// ReSharper disable UnusedMember.Global
namespace IctBaden.Framework.Arithmetic
{
    public class AverageSumDouble
    {
        private int _averageCount;
        private double _averageSum;

        public const int DefaultCount = 2;

        public AverageSumDouble(int count = DefaultCount)
        {
            _averageCount = count;
        }
        public AverageSumDouble(int count, double startValue)
        {
            _averageCount = count;
            _averageSum = startValue * (count - 1);
        }

        public void Reset()
        {
            _averageSum = 0;
        }
        public void SetAverageCount(int newCount)
        {
            _averageCount = newCount;
            Reset();
        }
        public double GetAverageValue(double newValue)
        {
            _averageSum += newValue;
            var retValue = _averageSum / _averageCount;
            _averageSum -= retValue;
            return retValue;
        }

    }
}