using System.IO;
using IctBaden.Framework.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IctBaden.Framework.Test.Logging
{
    public class FileLoggerTests
    {
        private readonly string _path = Path.GetTempPath();

        [Fact]
        public void FileLoggerShouldCreateFileIfNotExits()
        {
            var factory = new LogFileNameFactory(_path, LogFileCycle.OneFile, "test", "log");

            var logFileName = Path.Combine(_path, "test.log");
            if (File.Exists(logFileName))
            {
                File.Delete(logFileName);
            }

            var logger = new FileLogger(factory, "test-logger");
            
            logger.LogInformation("information-message");
            
            Assert.True(File.Exists(logFileName));
            var logContent = File.ReadAllText(logFileName);
            Assert.Contains("test-logger", logContent);
            Assert.Contains("information-message", logContent);
        }

        [Fact]
        public void FileLoggerShouldAppendFileIfExits()
        {
            var factory = new LogFileNameFactory(_path, LogFileCycle.OneFile, "test", "log");

            var logFileName = Path.Combine(_path, "test.log");
            if (File.Exists(logFileName))
            {
                File.Delete(logFileName);
            }

            var logger = new FileLogger(factory, "test-logger");
            
            logger.LogInformation("information-message");
            logger.LogError("error-message");
            
            Assert.True(File.Exists(logFileName));
            
            var logContent = File.ReadAllText(logFileName);
            Assert.Contains("test-logger", logContent);
            Assert.Contains("information-message", logContent);
            Assert.Contains("error-message", logContent);
        }

       
    }
}