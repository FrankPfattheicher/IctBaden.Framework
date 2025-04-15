using System;
using System.Runtime.InteropServices;

namespace IctBaden.Framework.AppUtils;

public class EnvironmentInfoWindows : IEnvironmentInfo
{
    [DllImport("user32.dll")]
    static extern IntPtr GetThreadDesktop(uint dwThreadId);

    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();

    
    public bool CanAccessDesktop()
    {
        try
        {
            var desktop = GetThreadDesktop(GetCurrentThreadId());
            if (desktop != IntPtr.Zero)
            {
                return true;
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }
    
}