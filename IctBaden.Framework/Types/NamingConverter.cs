using System;
using System.Globalization;
using System.Linq;
using System.Text;
// ReSharper disable UnusedMember.Global
// ReSharper disable ReplaceSubstringWithRangeIndexer

namespace IctBaden.Framework.Types;

/// <summary>
/// Naming converter supporting
/// camelCase  or LowerCamelCase
/// PascalCase or UpperCamelCase
/// kebab-case
/// snake_case
/// UPPER_SNAKE_CASE or SCREAMING_SNAKE_CASE
/// </summary>
public static class NamingConverter
{
    public static string PascalToKebabCase(string str) => PascalToKebabCase(str, '-');
    public static string PascalToSnakeCase(string str) => PascalToKebabCase(str, '_');
    public static string PascalToUpperSnakeCase(string str) => PascalToKebabCase(str, '_').ToUpperInvariant();
    
    public static string KebabToPascalCase(string str) => ToPascalCase(str, '-');

    public static string SnakeToPascalCase(string str) => ToPascalCase(str, '_');

    public static string ToCamelCase(string str) => string
        .Concat(str.Substring(0, 1)
            .ToLower(CultureInfo.InvariantCulture), str.AsSpan(1));

    public static string ToPascalCase(string str) => string
        .Concat(str.Substring(0, 1)
            .ToUpper(CultureInfo.InvariantCulture), str.AsSpan(1));

    /// <summary>
    /// Converts a string to Pascal case identifier
    /// </summary>
    /// <param name="text">Text to convert</param>
    public static string ToPascalIdentifier(string text)
    {
        // Replace all non-letter and non-digits with an underscore and lowercase the rest.
        var parts = string.Join("", text.Select(c => char.IsLetterOrDigit(c) ? c.ToString().ToLower(CultureInfo.InvariantCulture) : "_").ToArray());

        // Split the resulting string by underscore
        // Select first character, uppercase it and concatenate with the rest of the string
        var arr = parts
            .Split(['_'], StringSplitOptions.RemoveEmptyEntries)
            .Select(ToPascalCase);

        // Join the resulting collection
        parts = string.Join("", arr);

        return parts;
    }


    
    private static string PascalToKebabCase(string str, char delimiter)
    {
        if (string.IsNullOrEmpty(str))
            return string.Empty;

        var builder = new StringBuilder();
        builder.Append(char.ToLower(str.First(), CultureInfo.InvariantCulture));

        foreach (var c in str.Skip(1))
        {
            if (char.IsUpper(c))
            {
                builder.Append(delimiter);
                builder.Append(char.ToLower(c, CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    private static string ToPascalCase(string str, char delimiter)
    {
        if (string.IsNullOrEmpty(str))
            return string.Empty;

        var builder = new StringBuilder();
        builder.Append(char.ToUpper(str.First(), CultureInfo.InvariantCulture));

        var dash = false;
        foreach (var c in str.Skip(1))
        {
            if (c == delimiter)
            {
                dash = true;
            }
            else if (dash)
            {
                builder.Append(char.ToUpper(c, CultureInfo.InvariantCulture));
                dash = false;
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

}