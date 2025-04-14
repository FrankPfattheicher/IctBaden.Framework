namespace IctBaden.Framework.AppUtils;

internal interface IMemoryInfo
{
    ulong GetSystemRamInstalled();
    float GetSystemRamUsage();
    ulong GetSystemTotalVirtualMemory();
    float GetSystemVirtualMemoryUsage();
}