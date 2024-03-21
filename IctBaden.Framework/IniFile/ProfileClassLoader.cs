using System;
using System.Globalization;
using System.Linq;
using IctBaden.Framework.PropertyProvider;
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Framework.IniFile;

/// <summary>
/// Deserializes classes from INI file.
/// </summary>
public class ProfileClassLoader
{
    private readonly IFormatProvider _provider = CultureInfo.CurrentCulture;
    private readonly bool _expandEnvironmentVariables;
        
    // ReSharper disable once UnusedMember.Global
    public ProfileClassLoader()
    {
    }
    public ProfileClassLoader(IFormatProvider provider)
    {
        _provider = provider;
    }
    public ProfileClassLoader(IFormatProvider provider, bool expandEnvironmentVariables)
    {
        _provider = provider;
        _expandEnvironmentVariables = expandEnvironmentVariables;
    }
        
    /// <summary>
    /// Loads all properties of the target object
    /// from the section with the name of the class.
    /// </summary>
    /// <param name="targetObject">Object </param>
    /// <param name="iniFile">Profile including the section</param>
    /// <returns></returns>
    public void LoadClass(object targetObject, Profile iniFile)
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
    public void LoadClass(object targetObject, Profile iniFile, string sectionName)
    {
        var iniSection = iniFile[sectionName];
        var targetClass = new ClassPropertyProvider(targetObject);
        targetClass.SetProperties(iniSection, _provider, _expandEnvironmentVariables);
        
        // load subtypes from named sections
        var subClasses = targetObject.GetType().GetProperties()
            .Where(sc => sc.PropertyType.IsClass)
            .Where(sc => sc.PropertyType.Namespace?.StartsWith("System") != true)
            .ToArray();

        try
        {
            foreach (var subClassProperty in subClasses)
            {
                var subType = subClassProperty.GetValue(targetObject);
                if (subType == null) continue;

                LoadClass(subType, iniFile, subClassProperty.Name);
            }
        }
        catch
        {
            // ignore
        }
    }

}