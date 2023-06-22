using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace IctBaden.Framework.AppUtils;

#pragma warning disable CA1416
public class TemperatureInfoWindows : ITemperatureInfo
{
    public float GetSystemTemperature()
    {
        try
        {
            var temperatures = new List<double>();
            var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
            foreach (var baseObject in searcher.Get())
            {
                var obj = (ManagementObject)baseObject;
                var temperature = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                temperature = temperature / 10.0 - 273.15;  // Convert the value to celsius degrees
                temperatures.Add(temperature);
            }

            return (float)temperatures.Max();
        }
        catch (Exception ex)
        {
            Trace.TraceError($"GetSystemTemperature: {ex.Message}");
            return 0.0f;
        }
    }
}