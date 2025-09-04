using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.AppUtils;

/// <summary>
/// Provides information about an application
/// </summary>
// ReSharper disable once UnusedType.Global
public static class ApplicationInfo
{
    /// <summary>
    /// With netcore 3 introduced single file publish,
    /// the assemblies are unpacked in a temp directory.
    /// Assembly.Location returns this temp path :-(
    /// This is a first try to ALWAYS return the applications directory.
    /// Feedback welcome !!!
    /// </summary>
    /// <returns></returns>
    public static string ApplicationDirectory
    {
        get
        {
            var path = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            if (IsRunningInUnitTest)
            {
                return path;
            }

            using var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                var moduleName = processModule.ModuleName?.ToLower(CultureInfo.InvariantCulture);
                if (!string.Equals(moduleName, "dotnet", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(moduleName, "dotnet.exe", StringComparison.OrdinalIgnoreCase))
                {
                    // started as app.exe (netcore 3.1) or published single file
                    path = Path.GetDirectoryName(processModule.FileName);
                    if (path != null && Directory.Exists(path))
                    {
                        return path;
                    }
                }
            }

            // started as "dotnet <assembly>"
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                path = Path.GetDirectoryName(assembly.Location);
                if (path != null && Directory.Exists(path))
                {
                    return path;
                }
            }

            path = Environment.ProcessPath != null 
                ? Path.GetDirectoryName(Environment.ProcessPath)
                : null;
            if (path != null && Directory.Exists(path))
            {
                return path;
            }

            path = AppContext.BaseDirectory;
            return Directory.Exists(path)
                ? path
                : Environment.CurrentDirectory; // fallback
        }
    }

    /// <summary>
    /// Full path to the application executable.
    /// </summary>
    public static string ApplicationExeName
    {
        get
        {
            var fullExePath = Environment.GetCommandLineArgs().FirstOrDefault() 
                              ?? Process.GetCurrentProcess().MainModule?.FileName;
            return fullExePath ?? string.Empty;
        }
    }

    public static bool IsRunningInUnitTest
    {
        get
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null) return false;

            var moduleName = assembly.GetName().Name?.ToLower(CultureInfo.InvariantCulture) ?? string.Empty;
            return moduleName.Contains("testrunner", StringComparison.OrdinalIgnoreCase) ||
                   moduleName.Contains("testhost", StringComparison.OrdinalIgnoreCase);
        }
    }
    
    public static string ReleaseDate => File.GetCreationTime(ApplicationExeName).Date.ToString("d", new CultureInfo("de"));
    public static string ProductVersion => FileVersionInfo.GetVersionInfo(ApplicationExeName).ProductVersion ?? "<unknown>";

}