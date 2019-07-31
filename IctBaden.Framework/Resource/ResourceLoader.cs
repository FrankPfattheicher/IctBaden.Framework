using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.Resource
{
    public class ResourceLoader
    {
        /// <summary>
        /// Loads resource string by name from
        /// any assembly of current AppDomain
        /// </summary>
        /// <param name="resourceName">Name of the resource. Only base name required, case insensitive.</param>
        /// <returns></returns>
        public static string LoadString(string resourceName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);
            foreach (var assembly in assemblies)
            {
                var assemblyResourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(rn => rn.EndsWith(resourceName, StringComparison.InvariantCultureIgnoreCase));
                if (assemblyResourceName != null)
                {
                    return LoadString(assembly, assemblyResourceName);
                }
            }
            return null;
        }

        public static string LoadString(Assembly assembly, string resourceName)
        {
            string result;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                // Assembly resources are stored in default (Windows) encoding as the files are.
                using (var reader = new StreamReader(stream, Encoding.Default))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }
    }
}
