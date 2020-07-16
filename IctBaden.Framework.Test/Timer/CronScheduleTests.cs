using System;
using IctBaden.Framework.Timer;
using Xunit;

namespace IctBaden.Framework.Test.Timer
{
    public class CronScheduleTests
    {
        private DateTime _now;

        public CronScheduleTests()
        {
            var now = DateTime.Now;
            // exact second
            _now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        }

        [Fact]
        public void UniversalCronScheduleShouldBeValid()
        {
            Assert.True(CronSchedule.IsValid("* * * * * *"));

            var schedule = new CronSchedule("* * * * * *");
            Assert.True(schedule.IsValid());
        }
        
        [Fact]
        public void BadCronScheduleShouldBeValid()
        {
            Assert.False(CronSchedule.IsValid("bad"));
        }

        [Fact]
        public void ThisHourCronScheduleShouldBeInTime()
        {
            var schedule = new CronSchedule($"* {_now.Minute} {_now.Hour} {_now.Day} {_now.Month} *");
            Assert.True(schedule.InTime(_now));
        }

        [Fact]
        public void NextHourCronScheduleShouldNotBeInTime()
        {
            var schedule = new CronSchedule($"* {_now.Minute} {_now.Hour} {_now.Day} {_now.Month} *");
            _now += TimeSpan.FromHours(1);
            Assert.False(schedule.InTime(_now));
        }

        [Fact]
        public void NextSecondsCronScheduleShouldBeInGivenSpan()
        {
            var span = TimeSpan.FromSeconds(90);
            var next = _now + span;
            var schedule = new CronSchedule($"{next.Second} {next.Minute} {next.Hour} {next.Day} {next.Month} *");
            var time = schedule.TimeToNextSchedule(_now);
            Assert.Equal((long)span.TotalSeconds, (long)time.TotalSeconds);
        }

        [Fact]
        public void NextMinutesCronScheduleShouldBeInGivenSpan()
        {
            var span = TimeSpan.FromMinutes(90);
            var next = _now + span;
            var schedule = new CronSchedule($"{next.Second} {next.Minute} {next.Hour} {next.Day} {next.Month} *");
            var time = schedule.TimeToNextSchedule(_now);
            Assert.Equal((long)span.TotalSeconds, (long)time.TotalSeconds);
        }
        
        [Fact]
        public void NextHoursCronScheduleShouldBeInGivenSpan()
        {
            var span = TimeSpan.FromHours(90);
            var next = _now + span;
            var schedule = new CronSchedule($"{next.Second} {next.Minute} {next.Hour} {next.Day} {next.Month} *");
            var time = schedule.TimeToNextSchedule(_now);
            Assert.Equal((long)span.TotalSeconds, (long)time.TotalSeconds);
        }
        
        [Fact]
        public void NextDaysCronScheduleShouldBeInGivenSpan()
        {
            var span = TimeSpan.FromDays(90);
            var next = _now + span;
            var schedule = new CronSchedule($"{next.Second} {next.Minute} {next.Hour} {next.Day} {next.Month} *");
            var time = schedule.TimeToNextSchedule(_now);
            Assert.Equal((long)span.TotalSeconds, (long)time.TotalSeconds);
        }
        
        [Fact]
        public void NextMonthsCronScheduleShouldBeInGivenSpan()
        {
            var span = TimeSpan.FromDays(9 * 31);
            var next = _now + span;
            var schedule = new CronSchedule($"{next.Second} {next.Minute} {next.Hour} {next.Day} {next.Month} *");
            var time = schedule.TimeToNextSchedule(_now);
            Assert.Equal((long)span.TotalSeconds, (long)time.TotalSeconds);
        }
        
        [Fact]
        public void NextSundayHighNoonCronScheduleShouldBeInExpectedSpan()
        {
            // var next = _now + span;
            // var schedule = new CronSchedule($"{next.Second} {next.Minute} {next.Hour} {next.Day} {next.Month} *");
            // var time = schedule.TimeToNextSchedule(_now);
            // Assert.Equal((long)span.TotalSeconds, (long)time.TotalSeconds);
        }
        
    }
}