using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace IctBaden.Framework.AppUtils;

public class TemperatureInfoLinux : ITemperatureInfo
{
    private const string Thermal = "/sys/class/thermal";

    public float GetSystemTemperature()
    {
        try
        {
            var zones = Directory
                .EnumerateDirectories(Thermal, "thermal_zone*")
                .ToArray();
        
            if (!zones.Any())
            {
                Trace.TraceError($"No system folders {Thermal} found.");
                return 0.0f;
            }

            var temperatures = zones
                .Select(z => File.ReadAllText(Path.Combine(z, "temp")).Trim())
                .ToArray();

            var maxTemp = temperatures
                .Select(int.Parse)
                .Max();

            return maxTemp / 1000.0f;
        }
        catch (Exception ex)
        {
            Trace.TraceError($"GetSystemTemperature: {ex.Message}");
            return 0.0f;
        }
    }
}
