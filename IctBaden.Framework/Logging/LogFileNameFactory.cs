using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace IctBaden.Framework.Logging
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LogFileNameFactory
    {
        private readonly string _path;
        private readonly LogFileCycle _cycle;
        private readonly string _name;
        private readonly string _extension;

        public LogFileNameFactory(string path, LogFileCycle cycle, string name, string extension)
        {
            _path = path;
            _cycle = cycle;
            _name = name;
            _extension = extension;
        }

        public string GetLogFileName()
        {
            var now = DateTime.Now;
            var name = _cycle == LogFileCycle.OneFile
                ? _name
                : (string.IsNullOrEmpty(_name) ? "" : _name + "_");

            var period = _cycle switch
            {
                LogFileCycle.Daily => $"D{now.Year:D4}{now.Month:D2}{now.Day:D2}",
                LogFileCycle.Weekly => $"W{Thread.CurrentThread.CurrentCulture.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday):D2}",
                LogFileCycle.Monthly => $"M{now.Year:D4}{now.Month:D2}",
                LogFileCycle.Yearly => $"Y{now.Year:D4}",
                _ => ""
            };

            return Path.Combine(_path, name + period + "." + _extension);
        }
        
    }
}