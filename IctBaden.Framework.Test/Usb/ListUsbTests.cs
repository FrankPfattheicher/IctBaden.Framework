using IctBaden.Framework.Usb;
using Xunit;

namespace IctBaden.Framework.Test.Usb;

public class ListUsbTests
{
    [Fact]
    public void ListUsbDevicesShouldSucceed()
    {
        var devices = UsbInfo.GetDeviceList();
        Assert.NotNull(devices);
    }
    
}