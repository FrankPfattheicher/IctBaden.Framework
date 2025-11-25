using System;
using System.IO;

// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.AppUtils;

// ReSharper disable once UnusedMember.Global
public static class SystemInfo
{
    private static readonly IMemoryInfo MemoryInfo;
    private static readonly ICpuInfo CpuInfo;
    private static readonly ITemperatureInfo TemperatureInfo;
    private static readonly IEnvironmentInfo EnvironmentInfo;

    static SystemInfo()
    {
        switch (Platform)
        {
            case Platform.Windows:
                MemoryInfo = new MemoryInfoWindows();
                CpuInfo = new CpuInfoWindows();
                TemperatureInfo = new TemperatureInfoWindows();
                EnvironmentInfo = new EnvironmentInfoWindows();
                break;
            case Platform.Linux:
                MemoryInfo = new MemoryInfoLinux();
                CpuInfo = new CpuInfoLinux();
                TemperatureInfo = new TemperatureInfoLinux();
                EnvironmentInfo = new EnvironmentInfoLinux();
                break;
            default:
                throw new PlatformNotSupportedException("SystemInfo");
        }
    }

    /// <summary>
    /// Gets the runtime the application is running on.
    /// </summary>
    public static Platform Platform
    {
        get
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    return Platform.MacOSX;

                case PlatformID.Unix:
                case (PlatformID)128:   // Framework (1.0 and 1.1) didn't include any PlatformID value for Unix, so Mono used value 128.
                    return IsRunningOnMac()
                        ? Platform.MacOSX
                        : Platform.Linux;

                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                case PlatformID.Xbox:
                    return Platform.Windows;

                default:
                    return Platform.NotSupported;
            }

        }
    }
    private static bool IsRunningOnMac()
    {
        var osName = Environment.OSVersion.VersionString;
        return osName.Contains("darwin", StringComparison.OrdinalIgnoreCase);
    }


    public static float GetDiskUsage(string drive)
    {
        var info = new DriveInfo(drive);
            
        return (info.TotalSize - info.TotalFreeSpace) * 100.0f / info.TotalSize;
    }

    public static ulong GetSystemRamInstalled()
    {
        return MemoryInfo.GetSystemRamInstalled();
    }
    public static float GetSystemRamUsage()
    {
        return MemoryInfo.GetSystemRamUsage();
    }
    public static ulong GetSystemTotalVirtualMemory()
    {
        return MemoryInfo.GetSystemTotalVirtualMemory();
    }
    public static float GetSystemVirtualMemoryUsage()
    {
        return MemoryInfo.GetSystemVirtualMemoryUsage();
    }


    public static float GetCpuUsage()
    {
        return CpuInfo.GetCpuUsage();
    }

    public static float GetSystemTemperature()
    {
        return TemperatureInfo.GetSystemTemperature();
    }

    public static bool CanAccessDesktop()
    {
        return EnvironmentInfo.CanAccessDesktop();
    }

}