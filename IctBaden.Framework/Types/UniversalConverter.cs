using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using IList = System.Collections.IList;

// ReSharper disable ReplaceSubstringWithRangeIndexer

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IntroduceOptionalParameters.Global

namespace IctBaden.Framework.Types;

[SuppressMessage("Security", "MA0009:Add regex evaluation timeout")]
public static partial class UniversalConverter
{
    public static object? GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    public static object? ConvertToType(object? value, Type targetType) => 
        ConvertToType(value, targetType, CultureInfo.CurrentCulture);

    [SuppressMessage("Design", "MA0051:Method is too long")]
    public static object? ConvertToType(object? value, Type targetType, IFormatProvider provider)
    {
        if (value == null) return null;

        var sourceType = value.GetType();
        if (sourceType == targetType)
            return value;

        // check for explicit converters
        var cvm = value.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
        var cvt = cvm.FirstOrDefault(m => (m.ReturnType == targetType)
                                          && m.IsHideBySig
                                          && (m.GetParameters().Length == 1)
                                          && (m.GetParameters()[0].ParameterType == sourceType)
                                          && m.Name.StartsWith("op_Implicit", StringComparison.Ordinal));
        if (cvt != null)
        {
            return cvt.Invoke(value, [value]);
        }

        cvm = targetType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
        cvt = cvm.FirstOrDefault(m => (m.ReturnType == targetType)
                                      && m.IsHideBySig
                                      && (m.GetParameters().Length == 1)
                                      && (m.GetParameters()[0].ParameterType == sourceType)
                                      && m.Name.StartsWith("op_Explicit", StringComparison.Ordinal));
        if (cvt != null)
        {
            return cvt.Invoke(value, [value]);
        }

        // handle enums
        if (targetType.IsEnum)
        {
            var constructedType = typeof(ValidatedEnum<>).MakeGenericType(targetType);
            var enu = Activator.CreateInstance(constructedType, value);
            var hasValueProperty = constructedType.GetProperty("HasValue");
            if (hasValueProperty != null)
            {
                if (hasValueProperty.GetValue(enu, index: null) is true)
                {
                    var enumerationProperty = constructedType.GetProperty("Enumeration");
                    if (enumerationProperty != null)
                    {
                        return enumerationProperty.GetValue(enu, index: null);
                    }
                }
            }
        }

        // handle List<T>
        if (targetType is { IsGenericType: true, GenericTypeArguments.Length: 1 } && 
            targetType == typeof(List<>).MakeGenericType(targetType.GenericTypeArguments))
        {
            var elementType = targetType.GenericTypeArguments[0];
            if (Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType.GenericTypeArguments)) is not IList list) 
                return null;
                
            if (value is IEnumerable enumerableValue)
            {
                foreach (var val in enumerableValue)
                {
                    list.Add(ConvertToType(val, elementType, provider));
                }
            }
            else
            {
                list.Add(value);
            }

            return list;
        }

        // handle single dimensional arrays
        if (targetType.IsArray)
        {
            var elementType = targetType.GetElementType();
            if (elementType != null)
            {
                if (Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) is not IList list) 
                    return null;
                    
                if (value is IEnumerable enumerableValue)
                {
                    foreach (var val in enumerableValue)
                    {
                        list.Add(ConvertToType(val, elementType, provider));
                    }
                }
                else
                {
                    list.Add(value);
                }

                var array = Array.CreateInstance(elementType, list.Count);
                for (var ix = 0; ix < list.Count; ix++)
                {
                    array.SetValue(list[ix], ix);
                }

                return array;
            }
        }

        // check for Parse method
        try
        {
            var parseMethods = targetType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => string.Equals(m.Name, "Parse", StringComparison.OrdinalIgnoreCase))
                .ToArray();
                    
            var parseMethod = parseMethods
                .FirstOrDefault(m => m.GetParameters().Length == 2 
                                     && m.GetParameters()[0].ParameterType == typeof(string) 
                                     && m.GetParameters()[1].ParameterType == typeof(IFormatProvider));
            if (parseMethod != null)
            {
                var strValue = string.Format(provider, "{0}", value); 
                value = parseMethod.Invoke(null, [strValue, provider]);
                return value;
            }
            parseMethod = parseMethods
                .FirstOrDefault(m => m.GetParameters().Length == 1
                                     && m.GetParameters()[0].ParameterType == typeof(string));
            if (parseMethod != null)
            {
                value = parseMethod.Invoke(null, [value]);
                return value;
            }
        }
        catch (Exception)
        {
            // ignore
        }

        // handle "ToString"
        try
        {
            if (targetType == typeof(string))
            {
                return string.Format(provider, "{0}", value);
            }
        }
        catch (Exception)
        {
            // ignore
        }

        // Use default converter
        try
        {
            return Convert.ChangeType(value, targetType, provider);
        }
        catch (Exception)
        {
            // ignore
        }

        return (targetType == typeof(bool))
            ? ConvertTo<bool>(value)
            : GetDefault(targetType);
    }

    public static T? ConvertTo<T>(object? value) => ConvertTo(value, default(T));

    public static T? ConvertTo<T>(object? value, T defaultValue)
    {
        if (value == null) return defaultValue;

        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T?)(value.ToString() as object);
            }

            // ReSharper disable once InvertIf
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(value.ToString(), out var boolValue))
                {
                    return (T)Convert.ChangeType(boolValue, typeof(T), CultureInfo.InvariantCulture);
                }

                if (value is string str)
                {
                    var trueStrings = new[]
                    {
                        "-1", "1", "Y", "J", "T"
                    };
                    var falseStrings = new[]
                    {
                        "0", " ", "", "N", "F"
                    };
                    if (trueStrings.Contains(str.ToUpper(CultureInfo.InvariantCulture), StringComparer.OrdinalIgnoreCase))
                    {
                        return (T)Convert.ChangeType(value: true, typeof(T), CultureInfo.InvariantCulture);
                    }

                    if (falseStrings.Contains(str.ToUpper(CultureInfo.InvariantCulture), StringComparer.OrdinalIgnoreCase))
                    {
                        return (T)Convert.ChangeType(value: false, typeof(T), CultureInfo.InvariantCulture);
                    }
                }
            }
            else
            {
                return (T?)ConvertToType(value, typeof(T), CultureInfo.InvariantCulture);
            }
        }
        catch (Exception)
        {
            return defaultValue;
        }

        return defaultValue;
    }

    public static TimeSpan ParseTimeSpan(string txt)
    {
        if (string.IsNullOrEmpty(txt))
        {
            return TimeSpan.Zero;
        }

        var negative = txt.StartsWith('-');
        if (negative)
            txt = txt.Substring(1);

        //[ws][-]{ d | d.hh:mm[:ss[.ff]] | hh:mm[:ss[.ff]] }[ws]
        var formatDays = RegexFormatDays();
        var match = formatDays.Match(txt);
        if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var days1))
        {
            var result = new TimeSpan(days1, 0, 0, 0);
            return negative ? -result : result;
        }

        var formatFull = RegexFormatFull();
        match = formatFull.Match(txt);
        if (match.Success &&
            int.TryParse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var days) &&
            int.TryParse(match.Groups[4].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var hours) &&
            int.TryParse(match.Groups[5].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var minutes) &&
            int.TryParse(match.Groups[7].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds) &&
            int.TryParse(match.Groups[9].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fraction))
        {
            var result = new TimeSpan(days, hours, minutes, seconds, fraction);
            return negative ? -result : result;
        }

        return TimeSpan.Zero;
    }

    [GeneratedRegex("^([0-9]+)$")]
    private static partial Regex RegexFormatDays();
    //                                  12           34         5        6  7       8  9
    [GeneratedRegex(@"^(([0-9]+)\.)?(([0-9]+)\:([0-9]+))(\:([0-9]+)(\.([0-9]+))?)?$")]
    private static partial Regex RegexFormatFull();
}