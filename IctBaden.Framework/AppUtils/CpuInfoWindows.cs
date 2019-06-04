using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace IctBaden.Framework.AppUtils
{
    internal class CpuInfoWindows : ICpuInfo
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GetSystemTimes([In, Out] FILETIME idleTime, [In, Out] FILETIME kernelTime, [In, Out] FILETIME userTime);

        private static ulong _previousTotalTicks;
        private static ulong _previousIdleTicks;

        static float CalculateCpuLoad(UInt64 idleTicks, UInt64 totalTicks)
        {
            var totalTicksSinceLastTime = totalTicks - _previousTotalTicks;
            var idleTicksSinceLastTime = idleTicks - _previousIdleTicks;

            float ret = 1.0f - ((totalTicksSinceLastTime > 0) ? ((float)idleTicksSinceLastTime) / totalTicksSinceLastTime : 0);

            _previousTotalTicks = totalTicks;
            _previousIdleTicks = idleTicks;
            return ret;
        }

        private static ulong FileTimeToInt64(FILETIME ft)
        {
            return (((ulong)ft.dwHighDateTime) << 32) | ft.dwLowDateTime;
        }

        // Returns 1.0f for "CPU fully pinned", 0.0f for "CPU idle", or somewhere in between
        // You'll need to call this at regular intervals, since it measures the load between
        // the previous call and the current one.  Returns -1.0 on error.
        public float GetCpuUsage()
        {
            var idleTime = new FILETIME();
            var kernelTime = new FILETIME();
            var userTime = new FILETIME();
            return GetSystemTimes(idleTime, kernelTime, userTime)
                ? CalculateCpuLoad(FileTimeToInt64(idleTime), FileTimeToInt64(kernelTime) + FileTimeToInt64(userTime))
                : -1.0f;
        }

    }
}
