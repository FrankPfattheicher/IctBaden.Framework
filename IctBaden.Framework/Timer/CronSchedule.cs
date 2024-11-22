using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace IctBaden.Framework.Timer;
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
public partial class CronSchedule
{
    private static readonly Regex DividedRegex = RegexDivided();
    private static readonly Regex RangeRegex = RegexRange();
    private static readonly Regex WildRegex = RegexWild();
    private static readonly Regex ListRegex = RegexList();
#pragma warning disable MA0009
    private static readonly Regex ValidationRegex = new(DividedRegex + "|" + RangeRegex + "|" + WildRegex + "|" + ListRegex);
#pragma warning restore MA0009

    private readonly string _expression;
    private int[] _seconds = [];
    private int[] _minutes = [];
    private int[] _hours = [];
    private int[] _days = [];
    private int[] _months = [];
    private int[] _weekdays = [];

    public CronSchedule(string expressions)
    {
        _expression = expressions.ToUpper(CultureInfo.InvariantCulture);
        var weekday = new[] {"SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"};
        for (var w = 0; w < weekday.Length; w++)
        {
            _expression = _expression.Replace(weekday[w], w.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
        }
        var month = new[] {"JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
        for (var m = 0; m < month.Length; m++)
        {
            _expression = _expression.Replace(month[m], m.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
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
                    start = next = (start + TimeSpan.FromDays(1));
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

        const string anytime = "*";
        // ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        _seconds = GenerateSecondValues((matches.Count > 0 ? matches[0].ToString() : anytime) ?? anytime);
        _minutes = GenerateMinuteValues((matches.Count > 1 ? matches[1].ToString() : anytime) ?? anytime);
        _hours = GenerateHourValues((matches.Count > 2 ? matches[2].ToString() : anytime) ?? anytime);
        _days =GenerateDayValues((matches.Count > 3 ? matches[3].ToString() : anytime) ?? anytime);
        _months = GenerateMonthValues((matches.Count > 4 ? matches[4].ToString() : anytime) ?? anytime);
        _weekdays = GenerateWeekdayValues((matches.Count > 5 ? matches[5].ToString() : anytime) ?? anytime);
        // ReSharper restore NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
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

        return [];
    }

    private static int[] GetDivided(string configuration, int start, int max)
    {
        if (!DividedRegex.IsMatch(configuration))
            return [];

        var split = configuration.Split('/');
        var divisor = int.Parse(split[1], CultureInfo.InvariantCulture);

        return Enumerable.Range(start, max - start - 1)
            .Where(i => i % divisor == 0)
            .ToArray();
    }

    private static int[] GetRange(string configuration)
    {
        if (!RangeRegex.IsMatch(configuration))
            return [];

        var ret = new List<int>();
        var split = configuration.Split('-');
        var start = int.Parse(split[0], CultureInfo.InvariantCulture);
        int end;
        if (split[1].Contains('/', StringComparison.Ordinal))
        {
            split = split[1].Split("/".ToCharArray());
            end = int.Parse(split[0], CultureInfo.InvariantCulture);
            var divisor = int.Parse(split[1], CultureInfo.InvariantCulture);

            for (var i = start; i < end; ++i)
            {
                if (i % divisor == 0)
                {
                    ret.Add(i);
                }
            }
            return ret.ToArray();
        }
        end = int.Parse(split[1], CultureInfo.InvariantCulture);

        return Enumerable.Range(start, end - start).ToArray();
    }

    private static int[] GetWild(string configuration, int start, int max)
    {
        return !WildRegex.IsMatch(configuration) 
            ? [] 
            : Enumerable.Range(start, max - start).ToArray();
    }

    private static int[] GetList(string configuration)
    {
        if (!ListRegex.IsMatch(configuration))
            return [];

        return configuration
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray();
    }

#pragma warning disable MA0009
    [GeneratedRegex(@"(\*/\d+)")]
    private static partial Regex RegexDivided();
    [GeneratedRegex(@"(\d+\-\d+)\/?(\d+)?")]
    private static partial Regex RegexRange();
    [GeneratedRegex(@"(\*)")]
    private static partial Regex RegexWild();
    [GeneratedRegex(@"(((\d+,)*\d+)+)")]
    private static partial Regex RegexList();
#pragma warning restore MA0009
}