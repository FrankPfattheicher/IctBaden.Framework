// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable UnusedMember.Global

using System.Diagnostics;

namespace IctBaden.Framework.Automat
{
    using System;
    using System.Threading;

    public class StateTimeout : IDisposable
    {
        private Timer? _timeout;
        private readonly string _name;
        private readonly long _dueTime;
        public string Name => _name;
        public long DueTime => _dueTime;

        public StateTimeout(string timerName, long timeoutMilliseconds, TimerCallback callback)
        {
            _name = timerName;
            _dueTime = timeoutMilliseconds;
            _timeout = new Timer(callback, _name, Timeout.Infinite, Timeout.Infinite);
        }

        public void Activate()
        {
            var t = _timeout;
            if (t == null) return;

            try
            {
                Trace.TraceInformation("Activate Timeout:" + _name + "=" + _dueTime);
                t.Change(_dueTime, Timeout.Infinite);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            var t = _timeout;
            _timeout = null;

            if (t == null)
                return;

            t.Change(Timeout.Infinite, Timeout.Infinite);
            t.Dispose();
        }

        #endregion
    }
}
