using System;
using IctBaden.Framework.PropertyProvider;

// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.IniFile
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Types;

    public class ProfileSection : IPropertyProvider
    {
        private readonly Profile _profile;

        public const string UnnamedGlobalSectionName = " ";
        public bool IsUnnamedGlobalSection => Name.StartsWith(UnnamedGlobalSectionName);

        public ProfileSection(Profile profile, string sectionName)
        {
            _profile = profile;
            Name = sectionName;
            Keys = new ProfileKeyCollection();
        }

        public override string ToString()
        {
            return $"[{Name}]";
        }

        public string Name { get; }

        public ProfileKeyCollection Keys { get; }

        public PropertyBag Properties
        {
            get
            {
                var props = new PropertyBag();
                foreach (var k in Keys)
                {
                    props.Set(k.Name, k.StringValue);
                }
                return props;
            }
        }

        public ProfileKey this[string key]
        {
            get
            {
                foreach (var k in Keys)
                {
                    if (string.Compare(k.Name, 0, key, 0, 2048, StringComparison.InvariantCultureIgnoreCase) == 0)
                        return k;
                }
                var newKey = new ProfileKey(this, key);
                Keys.Add(newKey);
                return newKey;
            }
        }

        public void Remove()
        {
            _profile.Sections.Remove(_profile[Name]);
        }

        internal void Save()
        {
            _profile.Save();
        }

        #region IPropertyProvider Members

        public List<T> GetAll<T>()
        {
            return (from property in Keys select (T)property.ObjectValue).ToList();
        }

        public T Get<T>(string key)
        {
            return Get(key, default(T));
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (!Keys.Contains(key))
                return defaultValue;

            return UniversalConverter.ConvertTo(this[key].ObjectValue, defaultValue);
        }

        public void Set<T>(string key, T newValue)
        {
            this[key].ObjectValue = newValue;
        }

        public void Remove(string key)
        {
            Keys.Remove(this[key]);
        }

        public bool Contains(string key)
        {
            return Keys.Contains(key);
        }

        #endregion


        #region IEnumerable<KeyValuePair<string, object>>

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
#if !NET20
            return Keys.Select(key => new KeyValuePair<string, object>(key.Name, key.ObjectValue)).GetEnumerator();
#else
            var list = new List<KeyValuePair<string, object>>();
            foreach (var key in Keys)
            {
                list.Add(new KeyValuePair<string, object>(key.Name, key.ObjectValue));
            }
            return list.GetEnumerator();
#endif
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
