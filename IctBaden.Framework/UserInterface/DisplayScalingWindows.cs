using System;
using System.Drawing;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.UserInterface
{
    public class DisplayScalingWindows : IDisplayScaling
    {
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

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
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return 1.0f;
            
            var graphics = Graphics.FromHwnd(IntPtr.Zero);
            var desktop = graphics.GetHdc();
            var logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            var physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES); 
            var screenScalingFactor = (float)physicalScreenHeight / logicalScreenHeight;

            return screenScalingFactor; // 1.25 = 125%
        }
        
    }
}