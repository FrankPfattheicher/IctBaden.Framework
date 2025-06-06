using System;
using System.IO;
using System.Runtime.InteropServices;

namespace IctBaden.Framework.AppUtils;

/// <summary>
/// Provides information about the used framework
/// </summary>
public static class FrameworkInfo
{
    public static string FrameworkVersion => Environment.Version.ToString();
    public static string RuntimeDirectory => RuntimeEnvironment.GetRuntimeDirectory();
    public static string ClrVersion => RuntimeEnvironment.GetSystemVersion();

    public static bool IsSelfHosted { get; }

    [Obsolete("Classic framework no longer supported")]
    public static bool IsNetCore => true;

        
    static FrameworkInfo()
    {
        var runtimeDirectory = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "."));
        IsSelfHosted = string.Equals(runtimeDirectory, ApplicationInfo.ApplicationDirectory, StringComparison.OrdinalIgnoreCase);
    }
        
}