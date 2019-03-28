using IctBaden.Framework.PropertyProvider;

namespace IctBaden.Framework.IniFile
{
    /// <summary>
    /// Deserializes classes from INI file.
    /// </summary>
    public class ProfileClassLoader
    {
        /// <summary>
        /// Loads all properties of the target object
        /// from the section with the name of the class.
        /// </summary>
        /// <param name="targetObject">Object </param>
        /// <param name="iniFile">Profile including the section</param>
        /// <returns></returns>
        public static void LoadClass(object targetObject, Profile iniFile)
        {
            var type = targetObject.GetType();
            LoadClass(targetObject, iniFile, type.Name);
        }

        /// <summary>
        /// Loads all properties of the target object
        /// from the section with the name of the class.
        /// </summary>
        /// <param name="targetObject">Object </param>
        /// <param name="iniFile">Profile including the section</param>
        /// <param name="sectionName">Name of the section to load properties from</param>
        /// <returns></returns>
        public static void LoadClass(object targetObject, Profile iniFile, string sectionName)
        {
            var iniSection = iniFile[sectionName];
            var targetClass = new ClassPropertyProvider(targetObject);
            targetClass.SetProperties(iniSection);
        }

    }
}
