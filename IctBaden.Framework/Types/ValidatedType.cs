using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.Types
{
    public class ValidatedType
    {
        /// <summary>
        /// Kurzer Name des Typs
        /// </summary>
        public readonly string ShortName;

        /// <summary>
        /// Vollständiger Name des Typs
        /// </summary>
        public readonly string FullName;

        /// <summary>
        /// Vollständige Typinformation
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// Gibt an, ob ein Objekt des Typs instanziiert werden kann
        /// </summary>
        public readonly bool CanInstantiate;

        /// <summary>
        /// Zeigt an, ob der angegebene Typ gültig ist
        /// </summary>
        public readonly bool IsValid;

        /// <summary>
        /// Letzte Meldung eines Ausnahmefehlers
        /// </summary>
        public readonly string Message;

        public ValidatedType(string shortName)
        {
            ShortName = shortName;
            try
            {
                Type = GetTypeByShortName(shortName);
                if (Type != null)
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

        private static Type GetTypeByShortName(string name)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    var type = types.FirstOrDefault(t => t.Name == name);
                    if (type != null) return type;
                }
                catch
                {
                    // ignore
                }
            }
            return null;
        }

        public object CreateInstance()
        {
            return ObjectGenerator.Generate(Type);
        }
    }
}