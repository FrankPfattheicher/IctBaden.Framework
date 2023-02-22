using System;
using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable ArrangeTypeMemberModifiers

namespace IctBaden.Framework.Usb;

internal class UsbInfoLinux
{
    static string ExecuteBashCommand(string command)
    {
        // according to: https://stackoverflow.com/a/15262019/637142
        // thans to this we will pass everything as one command
        command = command.Replace("\"", "\"\"");

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"" + command + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        proc.Start();
        proc.WaitForExit();

        return proc.StandardOutput.ReadToEnd();
    }

    public UsbDevice[] GetDeviceList()
    {
        var info = ExecuteBashCommand(
            @"for sysdevpath in $(find /sys/bus/usb/devices/usb*/ -name dev); do
            (
                syspath=""${sysdevpath%/dev}""
                devname=""$(udevadm info -q name -p $syspath)""
                [[ ""$devname"" == ""bus/""* ]] && exit
                eval ""$(udevadm info -q property --export -p $syspath)""
                [[ -z ""$ID_SERIAL"" ]] && exit
                echo ""$syspath::/dev/$devname::$ID_VENDOR_FROM_DATABASE::$ID_MODEL""
            )
            done"
            );

        var devices = info
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        var usb = new List<UsbDevice>();
        foreach (var device in devices)
        {
            var devName = device.Split(new [] {"::"}, StringSplitOptions.None);
            usb.Add(new UsbDevice
            {
                DeviceName = devName[1],
                Vendor = devName[2],
                Product = devName[3]
            });
        }
        
        return usb.ToArray();
    }
}