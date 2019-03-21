using IctBaden.Framework.Types;

namespace IctBaden.Framework.IniFile
{
    public class ProfileObjectSerializer
    {
        /// <summary>
        /// Reads all properties from the given profile.
        /// Object's type name ti used as section name.
        /// </summary>
        /// <param name="obj">Object whos properties are to be read</param>
        /// <param name="profile">Profile containing data</param>
        public static void Load(object obj, Profile profile)
        {
            var type = obj.GetType();
            var iniSection = profile[type.Name];
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var value = iniSection.Get<object>(property.Name);
                if (value == null) continue;

                try
                {
                    value = UniversalConverter.ConvertToType(value, property.PropertyType);
                    property.SetValue(obj, value, null);
                }
                catch
                {
                    // ignore
                }
            }
        }

        /// <summary>
        /// Saves object properties to given profile.
        /// Object's type name ti used as section name.
        /// </summary>
        /// <param name="obj">Object whos properties are to be written</param>
        /// <param name="profile">Target profile</param>
        public static void Save(object obj, Profile profile)
        {
            var type = obj.GetType();
            var iniSection = profile[type.Name];
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(obj, null);
                if (value != null)
                {
                    iniSection.Set(property.Name, value);
                }
            }

            profile.Save();
        }

    }
}
