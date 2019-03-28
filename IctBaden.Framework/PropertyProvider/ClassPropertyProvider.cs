using System;
using IctBaden.Framework.Tron;
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
                TronTrace.TraceInformation($"Get({_type.Name}) {property.Name} = {value}");
            }

            return result;
        }

        public void SetProperties(IPropertyProvider propertyValues)
        {
            var properties = _type.GetProperties();
            foreach (var property in properties)
            {
                var value = propertyValues.Get<object>(property.Name);
                if (value != null)
                {
                    value = UniversalConverter.ConvertToType(value, property.PropertyType);
                    property.SetValue(_targetObject, value, null);
                    TronTrace.TraceInformation($"Set({_type.Name}) {property.Name} = {property.GetValue(_targetObject, null)}");
                }
            }
        }
    }
}
