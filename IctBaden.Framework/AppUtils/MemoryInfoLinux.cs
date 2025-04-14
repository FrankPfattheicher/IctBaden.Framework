using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace IctBaden.Framework.AppUtils;

public partial class MemoryInfoLinux : IMemoryInfo
{
    private const string MemInfo = "/proc/meminfo";

    private ulong _ramTotalBytes;
    private ulong _ramAvailableBytes;
    private ulong _virtualTotalBytes;
    private ulong _virtualAvailableBytes;
        
        
    private void ParseMemInfo()
    {
        if (!File.Exists(MemInfo))
        {
            Trace.TraceError($"System file {MemInfo} does not exist.");
            return;
        }
            
        var info = File.ReadAllText(MemInfo);
        var memTotal = RegexMemTotal().Match(info);
        _ramTotalBytes = (memTotal.Success) ? ulong.Parse(memTotal.Groups[1].Value) : 0;

        var memAvailable = RegexMemAvailable().Match(info);
        _ramAvailableBytes = (memAvailable.Success) ? ulong.Parse(memAvailable.Groups[1].Value) : 0;
            
        var virtualTotal = RegexVirtualTotal().Match(info);
        _virtualTotalBytes = (virtualTotal.Success) ? ulong.Parse(virtualTotal.Groups[1].Value) : 0;

        var virtualAvailable = RegexVirtualAvailable().Match(info);
        _virtualAvailableBytes = (virtualAvailable.Success) ? ulong.Parse(virtualAvailable.Groups[1].Value) : 0;
    }
        
    public ulong GetSystemRamInstalled()
    {
        ParseMemInfo();
        return _ramTotalBytes;
    }

    public float GetSystemRamUsage()
    {
        ParseMemInfo();
        return (_ramTotalBytes - _ramAvailableBytes) * 100.0f / _ramTotalBytes;
    }

    public ulong GetSystemTotalVirtualMemory()
    {
        ParseMemInfo();
        return _virtualTotalBytes;
    }

    public float GetSystemVirtualMemoryUsage()
    {
        ParseMemInfo();
        return (_virtualTotalBytes - _virtualAvailableBytes) / (float)_virtualTotalBytes;
    }

#pragma warning disable MA0009
    [GeneratedRegex(@"MemTotal\:\s+(\w+)\s+")]
    private static partial Regex RegexMemTotal();
    [GeneratedRegex(@"MemAvailable\:\s+(\w+)\s+")]
    private static partial Regex RegexMemAvailable();
    [GeneratedRegex(@"SwapTotal\:\s+(\w+)\s+")]
    private static partial Regex RegexVirtualTotal();
    [GeneratedRegex(@"SwapFree\:\s+(\w+)\s+")]
    private static partial Regex RegexVirtualAvailable();
#pragma warning restore MA0009
}