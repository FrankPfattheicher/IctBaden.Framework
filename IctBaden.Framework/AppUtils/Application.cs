using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.AppUtils;

/// <summary>
/// Provides information about an application
/// </summary>
// ReSharper disable once UnusedType.Global
[Obsolete("Use ApplicationInfo instead.")]
public static class Application
{
    /// <summary>
    /// With netcore 3 introduces single file publish,
    /// the assemblies are unpacked in a temp directory.
    /// Assembly.Location returns this temp path :-(
    /// This is a first try to ALWAYS return the applications directory.
    /// Feedback welcome !!!
    /// </summary>
    /// <returns></returns>
    [Obsolete("Use ApplicationInfo.ApplicationDirectory instead.")]
    public static string GetApplicationDirectory()
    {
        var path = AppContext.BaseDirectory;

        using (var processModule = Process.GetCurrentProcess().MainModule)
        {
            if (processModule != null 
                && !string.Equals(processModule.ModuleName, "dotnet", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(processModule.ModuleName, "dotnet.exe", StringComparison.OrdinalIgnoreCase))
            {
                // started as app.exe (netcore 3.1) or published single file
                path = Path.GetDirectoryName(processModule.FileName);
                if (path != null && Directory.Exists(path))
                {
                    return path;
                }
            }
        }
        // started as dotnet app.dll
        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            path = Path.GetDirectoryName(assembly.Location);
            if (path != null && Directory.Exists(path))
            {
                return path;
            }
        }

        if (path != null && Directory.Exists(path))
        {
            return path;
        }
        // fallback
        return Environment.CurrentDirectory;
    }
        
}