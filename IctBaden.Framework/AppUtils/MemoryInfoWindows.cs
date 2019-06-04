using System.Runtime.InteropServices;
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
#pragma warning disable 649

// ReSharper disable InconsistentNaming

namespace IctBaden.Framework.AppUtils
{
    internal class MemoryInfoWindows : IMemoryInfo
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        // ReSharper disable once InconsistentNaming
        private class MEMORYSTATUSEX
        {
            // ReSharper disable NotAccessedField.Local
            private uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            // ReSharper restore NotAccessedField.Local
            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public ulong GetSystemRamInstalled()
        {
            ulong installedMemory = 0;
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                installedMemory = memStatus.ullTotalPhys;
            }
            return installedMemory;
        }

        public float GetSystemRamUsage()
        {
            var usage = 0.0f;
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                usage = (memStatus.ullTotalPhys - memStatus.ullAvailPhys) * 100.0f / memStatus.ullTotalPhys;
            }
            return usage;
        }

        public ulong GetSystemTotalVirtualMemory()
        {
            ulong systemTotalVirtualMemory = 0;
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                systemTotalVirtualMemory = memStatus.ullTotalPageFile;
            }
            return systemTotalVirtualMemory;
        }

        public float GetSystemVirtualMemoryUsage()
        {
            var usage = 0.0f;
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                usage = (memStatus.ullTotalPageFile - memStatus.ullAvailPageFile) * 100.0f / memStatus.ullTotalPageFile;
            }
            return usage;
        }

    }
}
