using System;
using System.IO;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.AppUtils
{
    // ReSharper disable once UnusedMember.Global
    public static class SystemInfo
    { 

        public static float GetDiskUsage(string drive)
        {
            var info = new DriveInfo(drive);
            
            return (info.TotalSize - info.TotalFreeSpace) * 100.0f / info.TotalSize;
        }

        
    }
}
