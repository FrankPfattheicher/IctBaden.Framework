using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
#pragma warning disable IDISP004
            foreach (var baseObject in searcher.Get())
            {
                var obj = (ManagementObject)baseObject;
                var temperature = Convert.ToDouble(obj["CurrentTemperature"].ToString(), CultureInfo.InvariantCulture);
                temperature = temperature / 10.0 - 273.15;  // Convert the value to celsius degrees
                temperatures.Add(temperature);
            }
#pragma warning restore IDISP004

            return (float)temperatures.Max();
        }
        catch (Exception ex)
        {
            Trace.TraceError($"GetSystemTemperature: {ex.Message}");
            return 0.0f;
        }
    }
}