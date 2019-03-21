using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IctBaden.Framework.Tron
{
    public class OutputDebugStringTraceListener : TraceListener
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void OutputDebugString(string lpOutputString);

        public override void Write(string message)
        {
            OutputDebugString(message);
        }

        public override void WriteLine(string message)
        {
            OutputDebugString(message);
        }
    }
}
