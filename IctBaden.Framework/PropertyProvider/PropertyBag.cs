using System;
using System.Collections;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.PropertyProvider
{
#if !NET20
    using System.Linq;
#endif

    public class PropertyBag : IPropertyProvider
    {
        private readonly Dictionary<string, object> _data;

        public PropertyBag()
        {
            _data = new Dictionary<string, object>();
        }

        public void Append(IPropertyProvider source)
        {
            foreach (var prop in source)
            {
                Set(prop.Key, prop.Value);
            }
        }

        #region IEnumerable Members

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        #endregion

        
        #region IPropertyProvider Members

        public List<T> GetAll<T>()
        {
            return _data
                    .Where(property => property.GetType() == typeof(T))
                    .Select(property => (T) property.Value)
                    .ToList();
        }

        public T Get<T>(string key)
        {
            return Get(key, default(T));
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (!_data.ContainsKey(key) || (_data[key] == null))
                return defaultValue;

            return (T)Convert.ChangeType(_data[key], typeof(T));
        }

        public void Set<T>(string key, T newValue)
        {
            if (_data.ContainsKey(key))
            {
                _data[key] = newValue;
            }
            else
            {
                _data.Add(key, newValue);
            }
        }

        public void Remove(string key)
        {
            if (_data.ContainsKey(key))
            {
                _data.Remove(key);
            }
        }

        public bool Contains(string key)
        {
            return _data.ContainsKey(key);
        }

        #endregion

    }
}
