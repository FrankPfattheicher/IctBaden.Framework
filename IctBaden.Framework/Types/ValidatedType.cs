using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.Framework.Types;

public class ValidatedType
{
    /// <summary>
    /// Kurzer Name des Typs
    /// </summary>
    public string ShortName { get; private set; }

    /// <summary>
    /// Vollständiger Name des Typs
    /// </summary>
    public string FullName { get; private set; }

    /// <summary>
    /// Vollständige Typinformation
    /// </summary>
    public Type? Type { get; private set; }

    /// <summary>
    /// Gibt an, ob ein Objekt des Typs instanziiert werden kann
    /// </summary>
    public bool CanInstantiate { get; private set; }

    /// <summary>
    /// Zeigt an, ob der angegebene Typ gültig ist
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Letzte Meldung eines Ausnahmefehlers
    /// </summary>
    public string Message { get; private set; }

    public ValidatedType(string shortName)
    {
        ShortName = shortName;
        FullName = "";
        Message = "";
            
            
        try
        {
            Type = GetTypeByShortName(shortName);
            if (Type is { FullName: { } })
            {
                FullName = Type.FullName;

                var inst = ObjectGenerator.Generate(Type);
                CanInstantiate = (inst != null);
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            Debug.WriteLine(ex.Message);
            Message = ex.Message;
        }
        catch (MissingMethodException ex)
        {
            Debug.WriteLine(ex.Message);
            Message = ex.Message;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Message = ex.Message;
            throw;
        }

        IsValid = (Type != null);
    }

    private static Type? GetTypeByShortName(string name)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                var type = types.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
                if (type != null) return type;
            }
            catch
            {
                // ignore
            }
        }
        return null;
    }

    public object? CreateInstance() =>
        Type != null 
            ? ObjectGenerator.Generate(Type) 
            : null;
}