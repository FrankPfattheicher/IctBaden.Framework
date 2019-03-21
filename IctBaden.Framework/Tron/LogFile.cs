using System;
using System.IO;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.Tron
{
    public class LogFile
    {
        private readonly string _path;
        private readonly string _fileName;

        private int _oldDay;

        public bool AddTimeStamp { get; set; } = false;

        public LogFile(string path, string fileName)
        {
            _path = path;
            _fileName = fileName;
        }

        public static LogFile DailyLog(string path, string prefix)
        {
            if (!string.IsNullOrEmpty(prefix)) prefix += "-";
            return new LogFile(path, prefix + "{YEAR}-{MONTH}-{DAY}.log");
        }
        public static LogFile MontlyLog(string path, string prefix)
        {
            if (!string.IsNullOrEmpty(prefix)) prefix += "-";
            return new LogFile(path, prefix + "{YEAR}-{MONTH}.log");
        }

        public string CurrentFileName => _fileName
            .Replace("{YEAR}", $"{DateTime.Now.Year:D4}")
            .Replace("{MONTH}", $"{DateTime.Now.Month:D2}")
            .Replace("{DAY}", $"{DateTime.Now.Day:D2}");

        public void Write(string message)
        {
            var fileName = Path.Combine(_path, CurrentFileName);
            if (AddTimeStamp)
            {
                message = $"{DateTime.Now:T}.{DateTime.Now.Millisecond:D3}  " + message;

                if (DateTime.Now.Day != _oldDay)
                {
                    _oldDay = DateTime.Now.Day;
                    File.AppendAllText(fileName, Environment.NewLine + $"TRACEDATE {DateTime.Now:d}" + Environment.NewLine);
                }
            }
            File.AppendAllText(fileName, message);
        }

        public void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}
