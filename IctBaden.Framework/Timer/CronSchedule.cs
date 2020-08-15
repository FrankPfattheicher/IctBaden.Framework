using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IctBaden.Framework.Timer
{
    // forked from https://github.com/kevincolyar/CronNET

    /// <summary>
    /// A cron compatible schedule specification handler
    /// 
    /// * * * * * *
    /// | | | | | |
    /// | | | | | +--- day of week (0 - 6) (Sunday=0)
    /// | | | | +----- month (1 - 12)
    /// | | | +------- day of month (1 - 31)
    /// | | +--------- hour (0 - 23)
    /// | +----------- min (0 - 59)
    /// +------------- sec (0 - 59)
    ///
    /// 0 - Sun      Sunday
    /// 1 - Mon      Monday
    /// 2 - Tue      Tuesday
    /// 3 - Wed      Wednesday
    /// 4 - Thu      Thursday
    /// 5 - Fri      Friday
    /// 6 - Sat      Saturday
    ///
    /// </summary>
    public class CronSchedule
    {
        private static readonly Regex DividedRegex = new Regex(@"(\*/\d+)");
        private static readonly Regex RangeRegex = new Regex(@"(\d+\-\d+)\/?(\d+)?");
        private static readonly Regex WildRegex = new Regex(@"(\*)");
        private static readonly Regex ListRegex = new Regex(@"(((\d+,)*\d+)+)");
        private static readonly Regex ValidationRegex = new Regex(DividedRegex + "|" + RangeRegex + "|" + WildRegex + "|" + ListRegex);

        private readonly string _expression;
        private int[] _seconds = new int[0];
        private int[] _minutes = new int[0];
        private int[] _hours = new int[0];
        private int[] _days = new int[0];
        private int[] _months = new int[0];
        private int[] _weekdays = new int[0];

        public CronSchedule(string expressions)
        {
            _expression = expressions.ToUpper();
            var weekday = new[] {"SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"};
            for (var w = 0; w < weekday.Length; w++)
            {
                _expression = _expression.Replace(weekday[w], w.ToString());
            }
            var month = new[] {"JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
            for (var m = 0; m < month.Length; m++)
            {
                _expression = _expression.Replace(month[m], m.ToString());
            }
            Generate();
        }

        public bool IsValid()
        {
            return IsValid(_expression);
        }

        public static bool IsValid(string expression)
        {
            var matches = ValidationRegex.Matches(expression);
            return matches.Count > 0;
        }

        public bool IsMatch(DateTime dateTime)
        {
            return _seconds.Contains(dateTime.Second) &&
                   _minutes.Contains(dateTime.Minute) &&
                   _hours.Contains(dateTime.Hour) &&
                   _days.Contains(dateTime.Day) &&
                   _months.Contains(dateTime.Month) &&
                   _weekdays.Contains((int)dateTime.DayOfWeek);
        }
        
        private static DateTime StripMillis(DateTime dt) => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

        /// <summary>
        /// Examines the time span to the next schedule.
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public TimeSpan TimeToNextSchedule(DateTime from) => NextSchedule(from) - from;

        /// <summary>
        /// Finds next time stamp matching specification.
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public DateTime NextSchedule(DateTime from)
        {
            var start = StripMillis(from);
            var next = start;
            var maxIterations = 10 * 365;
            
            while ((maxIterations-- > 0) && (next <= from || !IsMatch(next)))
            {
                try
                {
                    while (true)
                    {
                        var month = Next(_months, next.Month, 1);
                        next = new DateTime(next.Year, month, next.Day, next.Hour, next.Minute, next.Second);
                        if (next >= start) break;
                        start = next = (start + TimeSpan.FromDays(30));
                    }
                    next = new DateTime(next.Year, next.Month, next.Day, next.Hour, next.Minute, next.Second);
                    while (!_weekdays.Contains((int) next.DayOfWeek))
                    {
                        next += TimeSpan.FromDays(1);
                    }
                    while (true)
                    {
                        var day = Next(_days, next.Day, 1);
                        next = new DateTime(next.Year, next.Month, day, next.Hour, next.Minute, next.Second);
                        if (next >= start) break;
                        start = next = (start + TimeSpan.FromDays(1));
                    }
                    while (true)
                    {
                        var hour = Next(_hours, next.Hour, 0);
                        next = new DateTime(next.Year, next.Month, next.Day, hour, next.Minute, next.Second);
                        if (next >= start) break;
                        start = next = (start + TimeSpan.FromHours(1));
                    }
                    while (true)
                    {
                        var minute = Next(_minutes, next.Minute, 0);
                        next = new DateTime(next.Year, next.Month, next.Day, next.Hour, minute, next.Second);
                        if (next >= start) break;
                        start = next = (start + TimeSpan.FromMinutes(1));
                    }
                    while (true)
                    {
                        var second = Next(_seconds, next.Second, 0);
                        next = new DateTime(next.Year, next.Month, next.Day, next.Hour, next.Minute, second);
                        if (next >= start) break;
                        start = next = (start + TimeSpan.FromSeconds(1));
                    }
                }
                catch (Exception)
                {
                    start = next = (start + TimeSpan.FromDays(1));
                }
            }
            return next;
        }

        private static int Next(int[] schedule, int value, int defaultValue)
        {
            var any = schedule.Where(n => n >= value).ToArray();
            if (any.Any()) return any.First();
            return schedule.Any() ? schedule.First() : defaultValue;
        }
        
        private void Generate()
        {
            if (!IsValid()) return;

            var matches = ValidationRegex.Matches(_expression);

            _seconds = GenerateSecondValues(matches.Count > 0 ? matches[0].ToString() : "*");
            _minutes = GenerateMinuteValues(matches.Count > 1 ? matches[1].ToString() : "*");
            _hours = GenerateHourValues(matches.Count > 2 ? matches[2].ToString() : "*");
            _days =GenerateDayValues(matches.Count > 3 ? matches[3].ToString() : "*");
            _months = GenerateMonthValues(matches.Count > 4 ? matches[4].ToString() : "*");
            _weekdays = GenerateWeekdayValues(matches.Count > 5 ? matches[5].ToString() : "*");
        }

        private static int[] GenerateSecondValues(string match) => GenerateScheduleValues(match, 0, 60);
        private static int[] GenerateMinuteValues(string match) => GenerateScheduleValues(match, 0, 60);
        private static int[] GenerateHourValues(string match) => GenerateScheduleValues(match, 0, 24);
        private static int[] GenerateDayValues(string match) => GenerateScheduleValues(match, 1, 32);
        private static int[] GenerateMonthValues(string match) => GenerateScheduleValues(match, 1, 13);
        private static int[] GenerateWeekdayValues(string match) => GenerateScheduleValues(match, 0, 7);

        private static int[] GenerateScheduleValues(string configuration, int start, int max)
        {
            if (DividedRegex.IsMatch(configuration)) return GetDivided(configuration, start, max);
            if (RangeRegex.IsMatch(configuration)) return GetRange(configuration);
            if (WildRegex.IsMatch(configuration)) return GetWild(configuration, start, max);
            if (ListRegex.IsMatch(configuration)) return GetList(configuration);

            return new int[0];
        }

        private static int[] GetDivided(string configuration, int start, int max)
        {
            if (!DividedRegex.IsMatch(configuration))
                return new int[0];

            var split = configuration.Split('/');
            var divisor = int.Parse(split[1]);

            return Enumerable.Range(start, max - start - 1)
                .Where(i => i % divisor == 0)
                .ToArray();
        }

        private static int[] GetRange(string configuration)
        {
            if (!RangeRegex.IsMatch(configuration))
                return new int[0];

            var ret = new List<int>();
            var split = configuration.Split('-');
            var start = int.Parse(split[0]);
            int end;
            if (split[1].Contains("/"))
            {
                split = split[1].Split("/".ToCharArray());
                end = int.Parse(split[0]);
                var divisor = int.Parse(split[1]);

                for (var i = start; i < end; ++i)
                {
                    if (i % divisor == 0)
                    {
                        ret.Add(i);
                    }
                }
                return ret.ToArray();
            }
            end = int.Parse(split[1]);

            return Enumerable.Range(start, end - start).ToArray();
        }

        private static int[] GetWild(string configuration, int start, int max)
        {
            return !WildRegex.IsMatch(configuration) 
                ? new int[0] 
                : Enumerable.Range(start, max - start).ToArray();
        }

        private static int[] GetList(string configuration)
        {
            if (!ListRegex.IsMatch(configuration))
                return new int[0];

            return configuration
                .Split(new []{','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
        }

    }
}
