using System.IO;
using System.Reflection;
using System.Text;

namespace IctBaden.Framework.Resource
{
    public class ResourceLoader
    {
        public static string LoadString(string resourceName)
        {
            return LoadString(Assembly.GetExecutingAssembly(), resourceName);
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