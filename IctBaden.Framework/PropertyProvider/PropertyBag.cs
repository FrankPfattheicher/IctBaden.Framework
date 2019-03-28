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
#if !NET20
            return (from property in _data where property.GetType() == typeof(T) select (T)property.Value).ToList();
#else
            var list = new List<T>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var property in data)
            {
                if (property.GetType() == typeof (T))
                {
                    list.Add((T)property.Value);
                }
            }
            return list;
#endif
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
