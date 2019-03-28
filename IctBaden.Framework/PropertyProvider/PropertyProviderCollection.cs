// ReSharper disable UnusedMember.Global

using System.Collections;
using System.Collections.Generic;

namespace IctBaden.Framework.PropertyProvider
{
#if !NET20
    using System.Linq;
#endif

    // ReSharper disable once UnusedMember.Global
    public class PropertyProviderCollection : IPropertyProvider
    {
        private readonly List<IPropertyProvider> _configurationProviderList;


        public PropertyProviderCollection()
        {
            _configurationProviderList = new List<IPropertyProvider>();
        }

        public void AddConfigurationProvider(IPropertyProvider configurationProvider)
        {
            _configurationProviderList.Insert(0, configurationProvider);
        }

        #region IPropertyProvider Members

        public List<T> GetAll<T>()
        {
            var list = new List<T>();
            foreach (var cfg in _configurationProviderList)
            {
                list.AddRange(cfg.GetAll<T>());
            }
            return list;
        }

        public T Get<T>(string key)
        {
            foreach (var cfg in _configurationProviderList)
            {
                if (cfg.Contains(key))
                    return cfg.Get<T>(key);
            }
            return default(T);
        }

        public T Get<T>(string key, T defaultValue)
        {
            foreach (var cfg in _configurationProviderList)
            {
                if (cfg.Contains(key))
                    return cfg.Get<T>(key);
            }
            return defaultValue;
        }

        public void Set<T>(string key, T newValue)
        {
            foreach (var cfg in _configurationProviderList)
            {
                if (cfg.Contains(key))
                {
                    cfg.Set(key, newValue);
                    return;
                }
            }
            if (_configurationProviderList.Count == 0)
                return;

            _configurationProviderList[0].Set(key, newValue);
        }

        public void Remove(string key)
        {
            foreach (var cfg in _configurationProviderList)
            {
                if (cfg.Contains(key))
                    cfg.Remove(key);
            }
        }

        public bool Contains(string key)
        {
            return _configurationProviderList.Any(cfg => cfg.Contains(key));
        }

        #endregion

        #region IEnumerable<KeyValuePair<string, object>>

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
