using System;
using System.Diagnostics;
using System.Threading;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.Timer
{
    /// <summary>
    /// Adapted from Collin Kidder's MicroLibrary
    /// http://c-sharp-snippet.blogspot.com/2015/02/microsecond-and-millisecond-c-timer.html
    /// https://github.com/collin80/GVRET-PC/blob/master/GVRET-PC/MicroLibrary.cs
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class MicrosecondTimer
    {
        private class MicroStopwatch : Stopwatch
        {
            private readonly float _microSecPerTick = 1000000f / Frequency;
            public long ElapsedMicroseconds => (long)(ElapsedTicks * _microSecPerTick);
        }

        // ReSharper disable once UnusedMember.Global
        public static bool IsSupported => Stopwatch.IsHighResolution;
        
        public delegate void MicroTimerElapsedEventHandler(long elapsed, long lateBy);
        
        private readonly MicroTimerElapsedEventHandler _handler;

        private Thread _threadTimer;
        private readonly long _timerIntervalInMicroSec;
        private bool _stopTimer = true;

        public MicrosecondTimer(MicroTimerElapsedEventHandler handler, uint intervalInMicroseconds)
        {
            if(intervalInMicroseconds == 0) throw new ArgumentException("Must not be greater than zero", nameof(intervalInMicroseconds));
                
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _timerIntervalInMicroSec = intervalInMicroseconds;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsRunning => (_threadTimer != null && _threadTimer.IsAlive);

        public void Start()
        {
            if (IsRunning) return;

            _stopTimer = false;
            _threadTimer = new Thread(TimerThread)
            {
                Priority = ThreadPriority.Highest
            };
            _threadTimer.Start();
        }

        public void Stop()
        {
            _stopTimer = true;
            if (!IsRunning || _threadTimer.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                return;
            }
            _threadTimer.Join();
        }

        private void TimerThread()
        {
            long nextNotification = 0;

            var microStopwatch = new MicroStopwatch();
            microStopwatch.Start();

            while (!_stopTimer)
            {
                nextNotification += _timerIntervalInMicroSec;
                long elapsedMicroseconds;

                while ( (elapsedMicroseconds = microStopwatch.ElapsedMicroseconds) < nextNotification)
                {
                    Thread.SpinWait(10);
                }

                var timerLateBy = elapsedMicroseconds - nextNotification;
                
                _handler(elapsedMicroseconds, timerLateBy);
            }

            microStopwatch.Stop();
        }
        
    }
}
