using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace IctBaden.Framework.UserInterface;

internal partial class DisplayInfoWindows : IDisplayInfo
{
    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    [SuppressMessage("Interoperability", "SYSLIB1054:Verwenden Sie \\\"LibraryImportAttribute\\\" anstelle von \\\"DllImportAttribute\\\", um P/Invoke-Marshallingcode zur Kompilierzeit zu generieren.")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc fnEnum, IntPtr dwData);

    // Struct for RECT (Windows API)
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    // Monitor DPI types
    private enum MonitorDpiType
    {
        MDT_EFFECTIVE_DPI = 0,
        MDT_ANGULAR_DPI = 1,
        MDT_RAW_DPI = 2,
        MDT_DEFAULT = MDT_EFFECTIVE_DPI
    }

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hMonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);
    
    public int GetMonitorCount()
    {
        var monitorCount = 0;

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);
        return monitorCount;

        bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData)
        {
            monitorCount++;
            return true; // Continue enumeration
        }
    }

    public float GetScalingFactor(int monitor = 1)
    {
        var scalingFactor = 1.0f;
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);
        return scalingFactor; // 1.25 = 125%
        
        bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData)
        {
            // Get DPI for each monitor
            var result = GetDpiForMonitor(hMonitor, MonitorDpiType.MDT_ANGULAR_DPI, out var dpiX, out _);
            if (result == 0) // S_OK
            {
                if ((hMonitor & 0xFF) == monitor)
                {
                    // Calculate scaling factor
                    scalingFactor = (float)(Math.Round(96.0 / dpiX * 100) / 100.0f);
                }
            }
            return true; // Continue enumeration
        }
    }

    public Rectangle GetVirtualScreen()
    {
        // Vollbild über alle Monitore
        var virtualScreen = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue,int.MinValue);
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);
        return virtualScreen;

        bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData)
        {
            var rectMonitor = Marshal.PtrToStructure<RECT>(lprcMonitor);
            virtualScreen.X = Math.Min(virtualScreen.X, rectMonitor.left);
            virtualScreen.Y = Math.Min(virtualScreen.Y, rectMonitor.top);
            virtualScreen.Width = Math.Max(virtualScreen.Width, rectMonitor.right);
            virtualScreen.Height = Math.Max(virtualScreen.Height, rectMonitor.bottom);
            return true; // Continue enumeration
        }

    }
    
}