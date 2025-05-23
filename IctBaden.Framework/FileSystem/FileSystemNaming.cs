﻿using System;
using System.Collections.Generic;
using System.IO;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.FileSystem;

public static class FileSystemNaming
{
    public static string GetValidFileName(string text)
    {
        var validFileName = text;
        var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var c in invalidChars)
        {
            validFileName = validFileName.Replace(c.ToString(), string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        return validFileName;
    }

    public static string ResolvePlaceholders(string text, IDictionary<string, string> values)
    {
        var resolved = text;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var pair in values)
        {
            resolved = resolved.Replace("%" + pair.Key + "%", pair.Value, StringComparison.OrdinalIgnoreCase);
        }
        return resolved;
    }

    /// <summary>
    /// See Win32 function PathAddBackslash 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string PathAddBackslash(string path)
    {
        // They're always one character but EndsWith is shorter than
        // array style access to last path character. Change this
        // if performance are a (measured) issue.
        var separator1 = Path.DirectorySeparatorChar.ToString();
        var separator2 = Path.AltDirectorySeparatorChar.ToString();

        // Trailing white spaces are always ignored but folders may have
        // leading spaces. It's unusual but it may happen. If it's an issue
        // then just replace TrimEnd() with Trim(). Tnx Paul Groke to point this out.
        path = path.TrimEnd();

        // Argument is always a directory name then if there is one
        // of allowed separators then I have nothing to do.
        if (path.EndsWith(separator1, StringComparison.OrdinalIgnoreCase) || path.EndsWith(separator2, StringComparison.OrdinalIgnoreCase))
            return path;

        // If there is the "alt" separator then I add a trailing one.
        // Note that URI format (file://drive:\path\filename.ext) is
        // not supported in most .NET I/O functions then we don't support it
        // here too. If you have to then simply revert this check:
        // if (path.Contains(separator1))
        //     return path + separator1;
        //
        // return path + separator2;
        if (path.Contains(separator2, StringComparison.OrdinalIgnoreCase))
            return path + separator2;

        // If there is not an "alt" separator I add a "normal" one.
        // It means path may be with normal one or it has not any separator
        // (for example if it's just a directory name). In this case I
        // default to normal as users expect.
        return path + separator1;
    }

    public static string GetRelativePath(string rootPath, string destinationPath)
    {
        rootPath = PathAddBackslash(rootPath);
        var fullPath = new Uri(destinationPath, UriKind.Absolute);
        var relRoot = new Uri(rootPath, UriKind.Absolute);

        var relPath = relRoot.MakeRelativeUri(fullPath).ToString();
        return relPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar); 
    }
}