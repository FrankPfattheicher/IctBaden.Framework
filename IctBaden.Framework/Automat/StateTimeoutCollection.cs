namespace IctBaden.Framework.Automat;

using System.Collections.Generic;
using System.Linq;

public class StateTimeoutCollection : List<StateTimeout>
{
    private readonly object _accessLock = new object();

    internal void Stop(string timerName)
    {
        lock (_accessLock)
        {
            var timers = (from timer in this where timer.Name == timerName select timer).ToList();
            foreach (var t in timers)
            {
                Remove(t);
                t.Dispose();
            }
        }
    }

    internal bool IsRunning(string timerName)
    {
        lock (_accessLock)
        {
            return this.Any(timer => timer.Name == timerName);
        }
    }

    internal void StopAll()
    {
        lock (_accessLock)
        {
            while (Count > 0)
            {
                var t = this[0];
                Remove(t);
#pragma warning disable IDISP007
                t.Dispose();
#pragma warning restore IDISP007
            }
        }
    }
}