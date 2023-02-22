using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using IctBaden.Framework.Types;

namespace IctBaden.Framework.PropertyProvider
{
    // ReSharper disable once UnusedMember.Global
    public class ClassPropertyProvider
    {
        private readonly object _targetObject;
        private readonly Type _type;

        public ClassPropertyProvider(object targetObject)
        {
            _targetObject = targetObject;
            _type = targetObject.GetType();
        }

        public PropertyBag GetProperties()
        {
            var result = new PropertyBag();

            var properties = _type.GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(_targetObject, null);
                result.Set(property.Name, value);
                Trace.TraceInformation($"Get({_type.Name}) {property.Name} = {value}");
            }

            return result;
        }

        public void SetProperties(IPropertyProvider propertyValues)
        {
            SetProperties(propertyValues, CultureInfo.CurrentCulture);
        }
        
        public void SetProperties(IPropertyProvider propertyValues, IFormatProvider provider)
        {
            var properties = _type.GetProperties();
            foreach (var property in properties)
            {
                var value = propertyValues.Get<object>(property.Name);
                if (value != null)
                {
                    // for array and list properties - split value on ';'s
                    if (property.PropertyType.IsArray
                        || (property.PropertyType.IsGenericType 
                            && property.PropertyType.GenericTypeArguments.Length == 1
                            && property.PropertyType == typeof(List<>).MakeGenericType(property.PropertyType.GenericTypeArguments)))
                    {
                        value = value.ToString()?.Split(';').ToList();
                    }
                    value = UniversalConverter.ConvertToType(value, property.PropertyType, provider);
                    property.SetValue(_targetObject, value, null);
                    Trace.TraceInformation($"Set({_type.Name}) {property.Name} = {property.GetValue(_targetObject, null)}");
                }
            }
        }
    }
}
