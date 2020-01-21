using System;
using IctBaden.Framework.PropertyProvider;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.IniFile
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Types;

    public class ProfileSection : IPropertyProvider
    {
        public Profile Profile { get; private set; }

        public const string UnnamedGlobalSectionName = " ";
        public bool IsUnnamedGlobalSection { get; } 
        public string Name { get; }
        public string Header { get; }


        public ProfileSection(Profile profile, string sectionName)
        {
            Profile = profile;
            Name = sectionName;
            IsUnnamedGlobalSection = (Name != null) && Name.StartsWith(UnnamedGlobalSectionName);
            Header = IsUnnamedGlobalSection ? string.Empty : ToString();
            Keys = new ProfileKeyCollection();
        }

        public sealed override string ToString()
        {
            return $"[{Name}]";
        }

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
                var existingKey = Keys
                    .FirstOrDefault(k => string.Compare(k.Name, key, StringComparison.InvariantCultureIgnoreCase) == 0);
                
                if (existingKey != null)
                        return existingKey;

                var newKey = new ProfileKey(this, key);
                Keys.Add(newKey);
                return newKey;
            }
        }

        public void Remove()
        {
            Profile.Sections.Remove(Profile[Name]);
        }

        internal void Save()
        {
            Profile.Save();
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
            return Keys.Select(key => new KeyValuePair<string, object>(key.Name, key.ObjectValue)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
