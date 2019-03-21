using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IctBaden.Framework.AppUtils
{
  public static class Platform
  {
    public static bool Is64BitProcess = (IntPtr.Size == 8);
    public static bool Is64BitOperatingSystem = Is64BitProcess || InternalCheckIsWow64();

    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool wow64Process);

    private static bool InternalCheckIsWow64()
    {
      if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) || Environment.OSVersion.Version.Major >= 6)
      {
        using (var p = Process.GetCurrentProcess())
        {
          bool retVal;
          return IsWow64Process(p.Handle, out retVal) && retVal;
        }
      }
      return false;
    }
  }
}