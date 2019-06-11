using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Text;

namespace IctBaden.Framework.AppUtils
{
    public static class PostMortemDebugging
    {
        public enum HandlerMode
        {
            // ReSharper disable once UnusedMember.Global
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

                // error file path
                var errorFileName = $"{AssemblyInfo.Default.ExeBaseName}_{DateTime.Now:u}.err";
                errorFileName = errorFileName.Replace(' ', '_').Replace(':', '-');
                errorFileName = Path.Combine(Path.GetTempPath(), errorFileName);

                // show internal error
                info.Append("ERROR: ");
                info.Append(e.ExceptionObject.GetType().Name);
                info.Append(" ");
                info.Append(_dumpReason.Message);
                info.Append(Environment.NewLine);
                info.Append(Environment.NewLine);
                info.Append(_stackTrace);
                info.Append(Environment.NewLine);
                
                // write dump
                File.WriteAllText(errorFileName, info.ToString());
            }
            finally
            {
                var text = $"{AssemblyInfo.Default.Title} - Internal Error (terminating={e.IsTerminating})" +
                           Environment.NewLine + info;
                
                Trace.TraceError(text);

                var errFileName = $"{AssemblyInfo.Default.ExeBaseName}_{DateTime.Now:u}.err";
                File.WriteAllText(errFileName, text);

                if (_restartOnFailure)
                {
                    var process = Assembly.GetEntryAssembly()?.Location;
                    if (process != null)
                    {
                        Trace.TraceInformation($"PostMortemDebugging.CurrentDomainUnhandledException trying to restart {process}");
                        var restart = new ProcessStartInfo
                        {
                            FileName = process,
                            UseShellExecute = true
                        };
                        Process.Start(restart);
                    }
                }
                if (_exitOnFailure || e.IsTerminating)
                {
                    Environment.Exit(1);
                }
            }
        }

    }
}
