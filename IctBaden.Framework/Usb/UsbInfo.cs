using System;
using IctBaden.Framework.AppUtils;

namespace IctBaden.Framework.Usb;

public class UsbInfo
{
    public static UsbDevice[] GetDeviceList()
    {
        return SystemInfo.Platform switch
        {
            Platform.Windows => throw new NotImplementedException(),
            Platform.Linux => new UsbInfoLinux().GetDeviceList(),
            _ => throw new PlatformNotSupportedException("UsbInfo")
        };
    }
}