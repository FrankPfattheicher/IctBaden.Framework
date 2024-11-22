using System.Collections.Generic;

namespace IctBaden.Framework.PropertyProvider;

public interface IPropertyProvider : IEnumerable<KeyValuePair<string, object?>>
{
#pragma warning disable MA0016
    List<T?> GetAll<T>();
#pragma warning restore MA0016
    T? Get<T>(string key);
    T? Get<T>(string key, T defaultValue);

    void Set<T>(string key, T newValue);

    void Remove(string key);
    bool Contains(string key);
}
