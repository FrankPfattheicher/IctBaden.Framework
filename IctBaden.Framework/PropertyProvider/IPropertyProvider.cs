using System.Collections.Generic;

namespace IctBaden.Framework
{
    public interface IPropertyProvider : IEnumerable<KeyValuePair<string, object>>
    {
        List<T> GetAll<T>();
        T Get<T>(string key);
        T Get<T>(string key, T defaultValue);

        void Set<T>(string key, T newValue);

        void Remove(string key);
        bool Contains(string key);
    }
}
