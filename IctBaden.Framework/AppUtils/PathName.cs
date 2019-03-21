using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IctBaden.Framework.AppUtils
{
    public static class PathName
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetLongPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder longPath, int longPathLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] 
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);

        public static string GetLongPathName(string shortPath)
        {
            if (!File.Exists(shortPath))
                return shortPath;

            var longPath = new StringBuilder(255);
            GetLongPathName(shortPath, longPath, longPath.Capacity);
            return longPath.ToString();
        }

        public static string GetShortPathName(string longPath)
        {
            if (!File.Exists(longPath))
                return longPath;

            var shortPath = new StringBuilder(255);
            GetShortPathName(longPath, shortPath, shortPath.Capacity);
            return shortPath.ToString();
        }
    }
}