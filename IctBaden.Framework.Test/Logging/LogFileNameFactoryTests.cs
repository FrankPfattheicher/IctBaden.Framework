using System;
using System.Globalization;
using System.IO;
using System.Threading;
using IctBaden.Framework.Logging;
using Xunit;

namespace IctBaden.Framework.Test.Logging
{
    public class LogFileNameFactoryTests
    {
        private readonly string _path = Path.GetTempPath();

        [Fact]
        public void FilenameForCycleOneFileShouldBeNameOnly()
        {
            var factory = new LogFileNameFactory(_path, LogFileCycle.OneFile, "test", "log");

            var logFileName = factory.GetLogFileName();
            
            Assert.StartsWith(_path, logFileName);
            var fileName = Path.GetFileName(logFileName);
            Assert.Equal("test.log", fileName);
        }
        
        [Fact]
        public void FilenameForCycleDailyShouldBeNameAndDay()
        {
            var factory = new LogFileNameFactory(_path, LogFileCycle.Daily, "test", "log");
            var now = DateTime.Now;
            var expectedName = $"test_{now.Year:D4}{now.Month:D2}{now.Day:D2}.log";

            var logFileName = factory.GetLogFileName();
            
            Assert.StartsWith(_path, logFileName);
            var fileName = Path.GetFileName(logFileName);
            Assert.Equal(expectedName, fileName);
        }
        
        [Fact]
        public void FilenameForCycleWeeklyShouldBeNameAndWeek()
        {
            var factory = new LogFileNameFactory(_path, LogFileCycle.Weekly, "test", "log");
            var now = DateTime.Now;
            var expectedName = $"test_{Thread.CurrentThread.CurrentCulture.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday):D2}.log";

            var logFileName = factory.GetLogFileName();
            
            Assert.StartsWith(_path, logFileName);
            var fileName = Path.GetFileName(logFileName);
            Assert.Equal(expectedName, fileName);
        }
        
        [Fact]
        public void FilenameForCycleMonthlyShouldBeNameAndMonth()
        {
            var factory = new LogFileNameFactory(_path, LogFileCycle.Monthly, "test", "log");
            var now = DateTime.Now;
            var expectedName = $"test_{now.Year:D4}{now.Month:D2}.log";

            var logFileName = factory.GetLogFileName();
            
            Assert.StartsWith(_path, logFileName);
            var fileName = Path.GetFileName(logFileName);
            Assert.Equal(expectedName, fileName);
        }
        
        [Fact]
        public void FilenameForCycleYearlyShouldBeNameAndYear()
        {
            var factory = new LogFileNameFactory(_path, LogFileCycle.Yearly, "test", "log");
            var now = DateTime.Now;
            var expectedName = $"test_{now.Year:D4}.log";

            var logFileName = factory.GetLogFileName();
            
            Assert.StartsWith(_path, logFileName);
            var fileName = Path.GetFileName(logFileName);
            Assert.Equal(expectedName, fileName);
        }
        
    }
}