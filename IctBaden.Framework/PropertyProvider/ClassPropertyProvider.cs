using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using IctBaden.Framework.Types;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IntroduceOptionalParameters.Global

namespace IctBaden.Framework.PropertyProvider;

// ReSharper disable once UnusedMember.Global
public class ClassPropertyProvider(object targetObject)
{
    private readonly Type _type = targetObject.GetType();

    public PropertyBag GetProperties()
    {
        var result = new PropertyBag();

        var properties = _type.GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(targetObject, null);
            result.Set(property.Name, value);
            Trace.TraceInformation($"Get({_type.Name}) {property.Name} = {value}");
        }

        return result;
    }

    public void SetProperties(IPropertyProvider propertyValues)
    {
        SetProperties(propertyValues, CultureInfo.CurrentCulture, false);
    }
    public void SetProperties(IPropertyProvider propertyValues, IFormatProvider provider)
    {
        SetProperties(propertyValues, provider, false);
    }
    public void SetProperties(IPropertyProvider propertyValues, IFormatProvider provider, bool expandEnvironmentVariables)
    {
        var properties = _type.GetProperties();
        foreach (var property in properties)
        {
            var value = propertyValues.Get<object>(property.Name);
            if (value == null) continue;
            
            var propertyType = property.PropertyType;
                    
            if (property.PropertyType.Name.StartsWith("Nullable`1", StringComparison.InvariantCultureIgnoreCase))
            {
                propertyType = property.PropertyType.GenericTypeArguments[0];
            }
            else if (property.PropertyType.IsArray
                     || (property.PropertyType is { IsGenericType: true, GenericTypeArguments.Length: 1 }
                         && property.PropertyType == typeof(List<>).MakeGenericType(property.PropertyType.GenericTypeArguments)))
            {
                // for array and list properties - split value on ';'s
                var listValue = UniversalConverter.ConvertTo(value, string.Empty);
                value = listValue?.Split(';').ToList();
            }

            while(expandEnvironmentVariables && value is string stringValue && stringValue.Contains('%'))
            {
                value = Environment.ExpandEnvironmentVariables(stringValue);
                if (string.Equals((string)value, stringValue, StringComparison.OrdinalIgnoreCase)) break;
            }

            value = UniversalConverter.ConvertToType(value, propertyType, provider);
            property.SetValue(targetObject, value, null);
            Trace.TraceInformation($"Set({_type.Name}) {property.Name} = {property.GetValue(targetObject, null)}");
        }
    }
}