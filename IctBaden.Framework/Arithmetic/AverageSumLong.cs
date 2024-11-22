// ReSharper disable UnusedMember.Global

using System;

namespace IctBaden.Framework.Arithmetic
{
    public class AverageSumLong
    {
        private int _averageCount;
        private long _averageSum;

        public const int DefaultCount = 2;

        public AverageSumLong(int count = DefaultCount)
        {
            if (count <= 0) throw new ArgumentException("The count mus be greater than 0", nameof(count));
            _averageCount = count;
        }
        public AverageSumLong(int count, long startValue)
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
        public long GetAverageValue(long newValue)
        {
            _averageSum += newValue;
            var retValue = _averageSum / _averageCount;
            _averageSum -= retValue;
            return retValue;
        }

    }
}
