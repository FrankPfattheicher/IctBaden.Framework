using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Framework.Types
{
    public static class UniversalConverter
    {
        public static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static object ConvertToType(object value, Type targetType)
        {
            return ConvertToType(value, targetType, CultureInfo.CurrentCulture);
        }
        public static object ConvertToType(object value, Type targetType, IFormatProvider provider)
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
                && m.Name.StartsWith("op_Implicit"));
            if (cvt != null)
            {
                return cvt.Invoke(value, new[] { value });
            }

            cvm = targetType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
            cvt = cvm.FirstOrDefault(m => (m.ReturnType == targetType)
                && m.IsHideBySig
                && (m.GetParameters().Length == 1)
                && (m.GetParameters()[0].ParameterType == sourceType)
                && m.Name.StartsWith("op_Explicit"));
            if (cvt != null)
            {
                return cvt.Invoke(value, new[] { value });
            }

            // handle enums
            if (targetType.IsEnum)
            {
                var constructedType = typeof(ValidatedEnum<>).MakeGenericType(targetType);
                var enu = Activator.CreateInstance(constructedType, value);
                var hasValueProperty = constructedType.GetProperty("HasValue");
                if(hasValueProperty != null)
                {
                    if (hasValueProperty.GetValue(enu, null) is bool hasValue && hasValue)
                    {
                        var enumerationProperty = constructedType.GetProperty("Enumeration");
                        if (enumerationProperty != null)
                        {
                            return enumerationProperty.GetValue(enu, null);
                        }
                    }
                }
            }
            
            // handle List<T>
            if (targetType.IsGenericType 
                && targetType.GenericTypeArguments.Length == 1
                && targetType == typeof(List<>).MakeGenericType(targetType.GenericTypeArguments))
            {
                var elementType = targetType.GenericTypeArguments[0];
                var list  = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType.GenericTypeArguments));
                if(value is IEnumerable enumerableValue)
                {
                    foreach (var val in enumerableValue)
                    {
                        list.Add(UniversalConverter.ConvertToType(val, elementType));
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
                var list  = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(new[]{elementType}));
                if(value is IEnumerable enumerableValue)
                {
                    foreach (var val in enumerableValue)
                    {
                        list.Add(UniversalConverter.ConvertToType(val, elementType));
                    }
                }
                else
                {
                    list.Add(value);
                }
                var array = Array.CreateInstance(elementType, list.Count);
                for(var ix = 0; ix < list.Count; ix++)
                {
                    array.SetValue(list[ix], ix);
                }

                return array;
            }

            // check for Parse method
            try
            {
                var parseMethod = targetType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
                if (parseMethod != null)
                {
                    value = parseMethod.Invoke(null, new[] {value});
                    return value;
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

            return (targetType == typeof (bool))
                ? ConvertTo<bool>(value) 
                : GetDefault(targetType);
        }

        public static T ConvertTo<T>(object value)
        {
            return ConvertTo(value, default(T));
        }

        public static T ConvertTo<T>(object value, T defaultValue)
        {
            try
            {
                if ((value != null) && (typeof(T) == typeof(string)))
                {
                    return (T)(object)value.ToString();
                }
                if ((value != null) && (typeof (T) == typeof (bool)))
                {
                    if (value.ToString() == "1")
                        return (T)Convert.ChangeType(true, typeof(T));
                    if (value.ToString() == "0")
                        return (T)Convert.ChangeType(false, typeof(T));
                }
                return (T)ConvertToType(value, typeof(T));
            }
            catch (FormatException)
            {
                if ((value != null) && (typeof(T) == typeof(bool)))
                {
                    if (bool.TryParse(value.ToString(), out var boolValue))
                        return (T)Convert.ChangeType(boolValue, typeof(T));
                    return (T)Convert.ChangeType((value.ToString() == "T") || (value.ToString() == "Y"), typeof(T));
                }
            }
            catch (InvalidCastException)
            {
                return defaultValue;
            }
            return defaultValue;
        }

        public static TimeSpan ParseTimeSpan(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return new TimeSpan();
            }
            var negative = txt.StartsWith("-");
            if (negative)
                txt = txt.Substring(1);

            //[ws][-]{ d | d.hh:mm[:ss[.ff]] | hh:mm[:ss[.ff]] }[ws]
            var formatDays = new Regex(@"^([0-9]+)$");
            var match = formatDays.Match(txt);
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out var days);
                var result = new TimeSpan(days, 0, 0, 0);
                return negative ? -result : result;
            }
            //                            12           34         5        6  7       8  9
            var formatFull = new Regex(@"^(([0-9]+)\.)?(([0-9]+)\:([0-9]+))(\:([0-9]+)(\.([0-9]+))?)?$");
            match = formatFull.Match(txt);
            if (match.Success)
            {
                int.TryParse(match.Groups[2].Value, out var days);
                int.TryParse(match.Groups[4].Value, out var hours);
                int.TryParse(match.Groups[5].Value, out var minutes);
                int.TryParse(match.Groups[7].Value, out var seconds);
                int.TryParse(match.Groups[9].Value, out var fraction);
                var result = new TimeSpan(days, hours, minutes, seconds, fraction);
                return negative ? -result : result;
            }
            return new TimeSpan();
        }

    }
}
