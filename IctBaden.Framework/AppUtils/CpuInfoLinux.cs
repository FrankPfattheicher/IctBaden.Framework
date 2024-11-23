using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace IctBaden.Framework.AppUtils;

public partial class CpuInfoLinux : ICpuInfo
{
    private const string StatInfo = "/proc/stat";

    private ulong _prevCpuTime;
    private ulong _prevCpuIdle;

    public float GetCpuUsage()
    {
        if (!File.Exists(StatInfo))
        {
            Trace.TraceError($"System file {StatInfo} does not exist.");
            return 0.0f;
        }
            
        var info = File.ReadLines(StatInfo).FirstOrDefault() ?? string.Empty;
        var numbers = RegexNumbers().Split(info).Where(n => !string.IsNullOrWhiteSpace(n))
            .ToArray();

        // https://github.com/Leo-G/DevopsWiki/wiki/How-Linux-CPU-Usage-Time-and-Percentage-is-calculated
        var user = ulong.Parse(numbers[0]);
        var nice = ulong.Parse(numbers[1]);
        var system = ulong.Parse(numbers[2]);
        var idle = ulong.Parse(numbers[3]);
        var iowait = ulong.Parse(numbers[4]);
        var irq = ulong.Parse(numbers[5]);
        var softirq = ulong.Parse(numbers[6]);
        var steal = ulong.Parse(numbers[7]);
//            var guest = ulong.Parse(numbers[8]);
//            var guest_nice = ulong.Parse(numbers[9]);
            
        // Total CPU time since boot = user+nice+system+idle+iowait+irq+softirq+steal
        var totalCpuTime = user + nice + system + idle + iowait + irq + softirq + steal;

        // Total CPU Idle time since boot = idle + iowait
        var totalCpuIdle = idle + iowait;

        var currentTime = totalCpuTime - _prevCpuTime; 
        var currentIdle = totalCpuIdle - _prevCpuIdle;

        _prevCpuTime = totalCpuTime;
        _prevCpuIdle = totalCpuIdle;
            
        // Total CPU usage time since boot = Total CPU time since boot - Total CPU Idle time since boot
        var currentUsage = currentTime - currentIdle;

        // Total CPU percentage = Total CPU usage time since boot/Total CPU time since boot X 100
        var totalCpuPercentage = currentUsage * 100.0f / currentTime;

        return totalCpuPercentage;
    }

#pragma warning disable MA0009
    [GeneratedRegex(@"\D+", RegexOptions.Singleline)]
    private static partial Regex RegexNumbers();
#pragma warning restore MA0009
}