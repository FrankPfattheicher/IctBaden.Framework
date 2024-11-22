using System;
using System.Drawing;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.UserInterface;

public partial class DisplayScalingWindows : IDisplayScaling
{
    [LibraryImport("gdi32.dll")]
    private static partial int GetDeviceCaps(IntPtr hdc, int nIndex);

    private enum DeviceCap
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        VERTRES = 10,
        DESKTOPVERTRES = 117
        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming
    }  
        
    public float GetScalingFactor()
    {
#pragma warning disable MA0144
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return 1.0f;
#pragma warning restore MA0144
            
        var graphics = Graphics.FromHwnd(IntPtr.Zero);
        var desktop = graphics.GetHdc();
        var logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
        var physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES); 
        var screenScalingFactor = (float)physicalScreenHeight / logicalScreenHeight;

        return screenScalingFactor; // 1.25 = 125%
    }
        
}