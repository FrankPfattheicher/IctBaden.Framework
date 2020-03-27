using System.Diagnostics;

namespace IctBaden.Framework.AppUtils
{
    public static class Shell
    {

        public static bool OpenFile(string fileName)
        {
            if (SystemInfo.Platform == Platform.Windows)
            {
                var si = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = true
                };
                Process.Start(si);
            }
            else if (SystemInfo.Platform == Platform.Linux)
            {
                Process.Start("xdg-open", fileName);
            }
            else if(SystemInfo.Platform == Platform.MacOSX)
            {
                Process.Start("open", fileName);
            }

            return false;
        }
        
    }
}