using System;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace IctBaden.Framework.AppUtils;

internal sealed partial class CpuInfoWindows : ICpuInfo
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemTimes(ref FILETIME idleTime, ref FILETIME kernelTime, ref FILETIME userTime);

    private static ulong _previousTotalTicks;
    private static ulong _previousIdleTicks;
    private static Thread? _pollThread;
    private static float _cpuUsage = -1.0f;

    public CpuInfoWindows()
    {
        _pollThread = new Thread(Poll)
        {
            IsBackground = true,
            Priority = ThreadPriority.Lowest,
            Name = "CpuInfo"
        };
        _pollThread.Start();
        
        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            _pollThread = null;
        };
    }

    private static void Poll()
    {
        while (_pollThread != null)
        {
            var idleTime = new FILETIME();
            var kernelTime = new FILETIME();
            var userTime = new FILETIME();
            
            _cpuUsage = GetSystemTimes(ref idleTime, ref kernelTime, ref userTime)
                ? CalculateCpuLoad(FileTimeToInt64(idleTime), FileTimeToInt64(kernelTime) + FileTimeToInt64(userTime))
                : -1.0f;
            
            Thread.Sleep(500);
        }
    }

    private static float CalculateCpuLoad(ulong idleTicks, ulong totalTicks)
    {
        var totalTicksSinceLastTime = totalTicks - _previousTotalTicks;
        var idleTicksSinceLastTime = idleTicks - _previousIdleTicks;

        var ret = 1.0f - ((totalTicksSinceLastTime > 0) ? ((float)idleTicksSinceLastTime) / totalTicksSinceLastTime : 0);

        _previousTotalTicks = totalTicks;
        _previousIdleTicks = idleTicks;
        return ret * 100;
    }

    private static ulong FileTimeToInt64(FILETIME ft)
    {
        return ((ulong)ft.dwHighDateTime << 32) | ft.dwLowDateTime;
    }

    // Returns 100.0f for "CPU fully pinned", 0.0f for "CPU idle", or somewhere in between
    // You'll need to call this at regular intervals, since it measures the load between
    // the previous call and the current one.  Returns -1.0 on error.
    public float GetCpuUsage() => _cpuUsage;

}