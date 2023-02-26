using System;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.PropertyProvider
{
    using System.Linq;

    public class PropertyBag : Dictionary<string, object?>, IPropertyProvider
    {
        public void Append(IPropertyProvider source)
        {
            foreach (var prop in source)
            {
                Set(prop.Key, prop.Value);
            }
        }

        public List<T?> GetAll<T>()
        {
            return this
                    .Where(property => property.GetType() == typeof(T))
                    .Select(property => (T?)property.Value)
                    .ToList();
        }

        public T? Get<T>(string key)
        {
            return Get(key, default(T));
        }

        public T? Get<T>(string key, T defaultValue)
        {
            if (!ContainsKey(key) || (this[key] == null))
                return defaultValue;

            return (T?)Convert.ChangeType(this[key], typeof(T));
        }

        public void Set<T>(string key, T newValue)
        {
            if (ContainsKey(key))
            {
                this[key] = newValue;
            }
            else
            {
                Add(key, newValue);
            }
        }

        public new void Remove(string key)
        {
            if (ContainsKey(key))
            {
                base.Remove(key);
            }
        }

        public bool Contains(string key)
        {
            return ContainsKey(key);
        }

    }
}
