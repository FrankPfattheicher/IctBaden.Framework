using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace IctBaden.Framework.Logging;

// ReSharper disable once ClassNeverInstantiated.Global
public class LogFileNameFactory
{
    private readonly string _path;
    private readonly LogFileCycle _cycle;
    private readonly string _name;
    private readonly string _extension;
        
    private string _lastFileName = string.Empty;
    private int _lastDay;

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
        if (now.Day == _lastDay) return _lastFileName;
            
        _lastDay = now.Day; 
        var name = _cycle == LogFileCycle.OneFile
            ? _name
            : (string.IsNullOrEmpty(_name) ? "" : _name + "_");

        var period = _cycle switch
        {
            LogFileCycle.Daily => $"{now.Year:D4}{now.Month:D2}{now.Day:D2}",
            LogFileCycle.Weekly => $"{Thread.CurrentThread.CurrentCulture.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday):D2}",
            LogFileCycle.Monthly => $"{now.Year:D4}{now.Month:D2}",
            LogFileCycle.Yearly => $"{now.Year:D4}",
            _ => ""
        };

        _lastFileName = Path.Combine(_path, name + period + "." + _extension);
        return _lastFileName;
    }
        
}