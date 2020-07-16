using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IctBaden.Framework.Timer
{
    // forked from https://github.com/kevincolyar/CronNET

    /// <summary>
    /// A cron compatible schedule sepcifiation handler
    /// 
    /// * * * * * *
    /// | | | | | |
    /// | | | | | +--- day of week (0 - 6) (Sunday=0)
    /// | | | | +----- month (1 - 12)
    /// | | | +------- day of month (1 - 31)
    /// | | +--------- hour (0 - 23)
    /// | +----------- min (0 - 59)
    /// +------------- sec (0 - 59)
    /// </summary>
    public class CronSchedule
    {
        private static readonly Regex DividedRegex = new Regex(@"(\*/\d+)");
        private static readonly Regex RangeRegex = new Regex(@"(\d+\-\d+)\/?(\d+)?");
        private static readonly Regex WildRegex = new Regex(@"(\*)");
        private static readonly Regex ListRegex = new Regex(@"(((\d+,)*\d+)+)");
        private static readonly Regex ValidationRegex = new Regex(DividedRegex + "|" + RangeRegex + "|" + WildRegex + "|" + ListRegex);

        private readonly string _expression;
        private List<int> _seconds;
        private List<int> _minutes;
        private List<int> _hours;
        private List<int> _daysOfMonth;
        private List<int> _months;
        private List<int> _daysOfWeek;

        public CronSchedule(string expressions)
        {
            this._expression = expressions;
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

        public bool InTime(DateTime dateTime)
        {
            return _seconds.Contains(dateTime.Second) &&
                   _minutes.Contains(dateTime.Minute) &&
                   _hours.Contains(dateTime.Hour) &&
                   _daysOfMonth.Contains(dateTime.Day) &&
                   _months.Contains(dateTime.Month) &&
                   _daysOfWeek.Contains((int)dateTime.DayOfWeek);
        }

        public TimeSpan TimeToNextSchedule(DateTime from)
        {
            var second = FirstOrDefault(_seconds, from.Second, 0);
            var minute = FirstOrDefault(_minutes, from.Minute, 0);
            var hour = FirstOrDefault(_hours, from.Hour, 0);
            var day = FirstOrDefault(_daysOfMonth, from.Day, 1);
            var month = FirstOrDefault(_months, from.Month, 1);
            var year = from.Year;
            var next = new DateTime(year, month, day, hour, minute, second);
            while (next < from)
            {
                year++;
                next = new DateTime(year, month, day, hour, minute, second);
            }
            return next - from;
        }

        private static int FirstOrDefault(IReadOnlyCollection<int> list, int value, int defaultValue)
        {
            var any = list.Where(n => n >= value).ToArray();
            if (any.Any()) return any.First();
            return list.Any() ? list.First() : defaultValue;
        }
        
        private void Generate()
        {
            if (!IsValid()) return;

            var matches = ValidationRegex.Matches(this._expression);

            generate_seconds(matches[0].ToString());

            generate_minutes(matches.Count > 1 ? matches[1].ToString() : "*");

            generate_hours(matches.Count > 2 ? matches[2].ToString() : "*");

            generate_days_of_month(matches.Count > 3 ? matches[3].ToString() : "*");

            generate_months(matches.Count > 4 ? matches[4].ToString() : "*");

            generate_days_of_weeks(matches.Count > 5 ? matches[5].ToString() : "*");
        }

        private void generate_seconds(string match)
        {
            this._seconds = generate_values(match, 0, 60);
        }
        private void generate_minutes(string match)
        {
            this._minutes = generate_values(match, 0, 60);
        }

        private void generate_hours(string match)
        {
            this._hours = generate_values(match, 0, 24);
        }

        private void generate_days_of_month(string match)
        {
            this._daysOfMonth = generate_values(match, 1, 32);
        }

        private void generate_months(string match)
        {
            this._months = generate_values(match, 1, 13);
        }

        private void generate_days_of_weeks(string match)
        {
            this._daysOfWeek = generate_values(match, 0, 7);
        }

        private List<int> generate_values(string configuration, int start, int max)
        {
            if (DividedRegex.IsMatch(configuration)) return divided_array(configuration, start, max);
            if (RangeRegex.IsMatch(configuration)) return range_array(configuration);
            if (WildRegex.IsMatch(configuration)) return wild_array(configuration, start, max);
            if (ListRegex.IsMatch(configuration)) return list_array(configuration);

            return new List<int>();
        }

        private static List<int> divided_array(string configuration, int start, int max)
        {
            if (!DividedRegex.IsMatch(configuration))
                return new List<int>();

            var ret = new List<int>();
            var split = configuration.Split("/".ToCharArray());
            var divisor = int.Parse(split[1]);

            for (var i = start; i < max; ++i)
            {
                if (i % divisor == 0)
                {
                    ret.Add(i);
                }
            }

            return ret;
        }

        private static List<int> range_array(string configuration)
        {
            if (!RangeRegex.IsMatch(configuration))
                return new List<int>();

            var ret = new List<int>();
            var split = configuration.Split("-".ToCharArray());
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
                return ret;
            }
            end = int.Parse(split[1]);

            for (var i = start; i <= end; ++i)
            {
                ret.Add(i);
            }

            return ret;
        }

        private static List<int> wild_array(string configuration, int start, int max)
        {
            if (!WildRegex.IsMatch(configuration))
                return new List<int>();

            var ret = new List<int>();

            for (var i = start; i < max; ++i)
            {
                ret.Add(i);
            }

            return ret;
        }

        private static List<int> list_array(string configuration)
        {
            if (!ListRegex.IsMatch(configuration))
                return new List<int>();

            var split = configuration.Split(",".ToCharArray());

            return split.Select(int.Parse).ToList();
        }

    }
}
