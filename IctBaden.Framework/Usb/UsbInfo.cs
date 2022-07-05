using System;
using IctBaden.Framework.AppUtils;

namespace IctBaden.Framework.Usb;

public static class UsbInfo
{
    public static UsbDevice[] GetDeviceList()
    {
        return SystemInfo.Platform switch
        {
            Platform.Windows => Array.Empty<UsbDevice>(),
            Platform.Linux => new UsbInfoLinux().GetDeviceList(),
            _ => throw new PlatformNotSupportedException("UsbInfo")
        };
    }
}