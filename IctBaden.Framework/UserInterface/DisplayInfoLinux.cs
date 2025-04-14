using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;

namespace IctBaden.Framework.UserInterface;

/// <summary>
/// sudo apt-get install libgtk-3-dev
/// </summary>
[SuppressMessage("Design", "MA0026:Fix TODO comment")]
internal partial class DisplayInfoLinux : IDisplayInfo
{
    // Importiere gdk_display_open() aus der GDK-Bibliothek
    [LibraryImport("libgdk-3.so.0")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr gdk_display_open([MarshalAs(UnmanagedType.LPStr)] string displayName);
    
    // Importiere gdk_display_get_default() aus der GDK-Bibliothek
    [LibraryImport("libgdk-3.so.0")]
    private static partial IntPtr gdk_display_get_default();

    // Importiere gdk_display_get_n_monitors() aus der GDK-Bibliothek
    [LibraryImport("libgdk-3.so.0")]
    private static partial int gdk_display_get_n_monitors(IntPtr display);

    // Importiere gdk_display_get_monitor() aus der GDK-Bibliothek
    [LibraryImport("libgdk-3.so.0")]
    private static partial IntPtr gdk_display_get_monitor(IntPtr display, int monitorNum);

    // Importiere gdk_monitor_get_scale_factor() aus der GDK-Bibliothek
    [LibraryImport("libgdk-3.so.0")]
    private static partial int gdk_monitor_get_scale_factor(IntPtr monitor);

    // Importiere gdk_monitor_get_geometry() aus der GDK-Bibliothek
    [LibraryImport("libgdk-3.so.0")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void gdk_monitor_get_geometry(IntPtr monitor, out GdkRectangle geometry);

    // Struktur zur Darstellung der Monitor-Geometrie
    [StructLayout(LayoutKind.Sequential)]
    public struct GdkRectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public int GetMonitorCount()
    {
        // Hole das Standard-Display
        var display = gdk_display_get_default();

        return display == IntPtr.Zero 
            ? 1 
            : gdk_display_get_n_monitors(display);
    }

    public float GetScalingFactor(int monitor = 1)
    {
        var display = gdk_display_open(":0");
        if (display == IntPtr.Zero) return 1.0f;
        
        var mon = gdk_display_get_monitor(display, monitor - 1);
        if (mon == IntPtr.Zero) return 1.0f;
        
        var scaleFactor = gdk_monitor_get_scale_factor(mon);
        return scaleFactor;
    }

    public Rectangle GetVirtualScreen()
    {
        var virtualScreen = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue,int.MinValue);
        
        var display = gdk_display_open(":0");
        var monitorCount = GetMonitorCount();

        for (var i = 0; i < monitorCount; i++)
        {
            var monitor = gdk_display_get_monitor(display, i);
            if (monitor == IntPtr.Zero) continue;

            gdk_monitor_get_geometry(monitor, out GdkRectangle geometry);

            if (geometry.X < virtualScreen.X) virtualScreen.X = geometry.X;
            if (geometry.Y < virtualScreen.Y) virtualScreen.Y = geometry.Y;

            var maxRight = geometry.X + geometry.Width;
            var maxBottom = geometry.Y + geometry.Height;

            if (maxRight > virtualScreen.Width) virtualScreen.Width = maxRight;
            if (maxBottom > virtualScreen.Height) virtualScreen.Height = maxBottom;
        }
        
        return virtualScreen;
    }
}