using System;
using System.Drawing;
using IctBaden.Framework.AppUtils;

namespace IctBaden.Framework.UserInterface;

// ReSharper disable once UnusedType.Global
public class DisplayInfo
{
    private static readonly IDisplayInfo DisplayInfoImpl;

    static DisplayInfo()
    {
        switch (SystemInfo.Platform)
        {
            case Platform.Windows:
                DisplayInfoImpl = new DisplayInfoWindows();
                break;
            case Platform.Linux:
                DisplayInfoImpl = new DisplayInfoLinux();
                break;
            default:
                throw new PlatformNotSupportedException("DisplayInfo");
        }
    }

    public static int GetMonitorCount() => DisplayInfoImpl.GetMonitorCount();
    public static float GetScalingFactor(int monitor = 0) => DisplayInfoImpl.GetScalingFactor(monitor);
    public static Rectangle GetVirtualScreen() => DisplayInfoImpl.GetVirtualScreen();
}