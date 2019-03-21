using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace IctBaden.Framework.AppUtils
{
    public static class PostMortemDebugging
    {
        public enum HandlerMode
        {
            KeepRunning,
            ExitApplication,
            ExitAndRestart
        }
        
        private static Exception _dumpReason;
        private static string _stackTrace;
        private static bool _exitOnFailure;
        private static bool _restartOnFailure;

        /// <summary>
        /// Enables AppDomain wide exception handling
        /// </summary>
        /// <param name="mode">Specifies how to handle application exception.</param>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void Enable(HandlerMode mode)
        {
            _exitOnFailure = mode == HandlerMode.ExitApplication || mode == HandlerMode.ExitAndRestart;
            _restartOnFailure = mode == HandlerMode.ExitAndRestart;

            //Application.SetUnhandledExceptionMode(_exitOnFailure ? UnhandledExceptionMode.ThrowException : UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is OutOfMemoryException)
            {
                // could not be handled useful here
                Environment.Exit(1);
            }

            var info = new StringBuilder();
            try
            {
                // save information
                _dumpReason = (Exception)e.ExceptionObject;
                _stackTrace = _dumpReason.StackTrace;

                // dump file path
                var dumpFileName = $"{AssemblyInfo.Default.ExeBaseName}_{DateTime.Now:u}.dmp";
                dumpFileName = dumpFileName.Replace(' ', '_').Replace(':', '-');
                dumpFileName = Path.Combine(Path.GetTempPath(), dumpFileName);

                // show internal error
                info.Append("ERROR: ");
                info.Append(e.ExceptionObject.GetType().Name);
                info.Append(" ");
                info.Append(_dumpReason.Message);
                info.Append(Environment.NewLine);
                info.Append(Environment.NewLine);
                info.Append(_stackTrace);
                info.Append(Environment.NewLine);
                info.Append(Environment.NewLine);
                info.Append("DUMP: ");
                info.Append(dumpFileName);

                // write dump
                WriteDumpFile(dumpFileName);
            }
            finally
            {
                var text = $"{AssemblyInfo.Default.Title} - Interner Fehler (terminating={e.IsTerminating})" +
                           Environment.NewLine + info;
                
                Trace.TraceError(text);

                var errFileName = $"{AssemblyInfo.Default.ExeBaseName}_{DateTime.Now:u}.err";
                File.WriteAllText(errFileName, text);

                if (_restartOnFailure)
                {
                    var process = Assembly.GetEntryAssembly().Location;
                    Trace.TraceInformation($"PostMortemDebugging.CurrentDomainUnhandledException trying to restart {process}");
                    var restart = new ProcessStartInfo
                    {
                        FileName = process,
                        UseShellExecute = true
                    };
                    Process.Start(restart);
                }
                if (_exitOnFailure || e.IsTerminating)
                {
                    Environment.Exit(1);
                }
            }
        }

        internal enum MinidumpType
        {
            MiniDumpNormal = 0x00000000,
            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpFilterMemory = 0x00000008,
            MiniDumpScanMemory = 0x00000010,
            MiniDumpWithUnloadedModules = 0x00000020,
            MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
            MiniDumpFilterModulePaths = 0x00000080,
            MiniDumpWithProcessThreadData = 0x00000100,
            MiniDumpWithPrivateReadWriteMemory = 0x00000200,
            MiniDumpWithoutOptionalData = 0x00000400,
            MiniDumpWithFullMemoryInfo = 0x00000800,
            MiniDumpWithThreadInfo = 0x00001000,
            MiniDumpWithCodeSegs = 0x00002000
        }

        [DllImport("dbghelp.dll")]
        static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            Int32 processId,
            IntPtr hFile,
            MinidumpType dumpType,
            IntPtr exceptionParam,
            IntPtr userStreamParam,
            IntPtr callackParam);

        static void WriteDumpFile(String fileToDump)
        {
            var fsToDump = File.Create(fileToDump);
            var thisProcess = Process.GetCurrentProcess();
            if (fsToDump.SafeFileHandle != null)
                MiniDumpWriteDump(thisProcess.Handle, thisProcess.Id, fsToDump.SafeFileHandle.DangerousGetHandle(), MinidumpType.MiniDumpWithFullMemory, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            fsToDump.Close();
        }

    }
}
