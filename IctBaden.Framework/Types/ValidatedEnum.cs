// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IntroduceOptionalParameters.Global
namespace IctBaden.Framework.Types;

using System;
using System.Linq;

/// <summary>
/// Ermöglicht die einfache Konvertierung zwischen enum- / string- / und numerischer Darstellung
/// </summary>
// ReSharper disable once InconsistentNaming
public class ValidatedEnum<TEnum> where TEnum : struct
{
    /// <summary>
    /// Zeigt an, ob der angegebene Wert gültig ist
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Zeigt an, ob ein Wert vorhanden ist (nicht zwingend valid)
    /// </summary>
    public bool HasValue { get; private set; }

    /// <summary>
    /// enum-Wert
    /// </summary>
    public TEnum Enumeration { get; private set; }

    /// <summary>
    /// Numerischer Wert
    /// </summary>
    public long Numeric { get; private set; }

    /// <summary>
    /// String-Repräsentation
    /// </summary>
    public string? Text { get; private set; }


    /// <summary>
    /// Constructor with automatic type inference
    /// </summary>
    /// <param name="data"></param>
    public ValidatedEnum(object? data) : this(data, false)
    {
    }

    /// <summary>
    /// Constructor with automatic type inference and optional case handling
    /// </summary>
    /// <param name="data"></param>
    /// <param name="ignoreCase">Ignore case comparing enum values</param>
    public ValidatedEnum(object? data, bool ignoreCase)
    {
        HasValue = false;
            
        switch (data)
        {
            case null:
                IsValid = false;
                return;
            case int intData:
                data = (long)intData;
                break;
        }

        if (long.TryParse(data.ToString(), out var numeric))
        {
            Numeric = numeric;
            data = numeric;
        }

        if (data is long longData)
        {
            HasValue = true;
            Numeric = longData;
            Text = Enum.GetNames(typeof(TEnum)).FirstOrDefault(name => Convert.ToInt32(Enum.Parse(typeof(TEnum), name)) == Numeric);
            if (Text != null)
            {
                Enumeration = (TEnum)Enum.Parse(typeof(TEnum), Text);
                IsValid = true;
                return;
            }
        }

        HasValue = !string.IsNullOrEmpty(Text);
        Text = Enum.GetNames(typeof(TEnum))
            .FirstOrDefault(name => string.Equals(name, Text, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));

        if (Text != null && Enum.TryParse(Text, out TEnum enumeration))
        {
            Enumeration = enumeration;
            Numeric = Convert.ToInt32(enumeration);
            IsValid = true;
            return;
        }

        Text = data.ToString()?.Replace("|", ",");
            
        if (Enum.TryParse(Text, out enumeration))
        {
            Enumeration = enumeration;
            Numeric = Convert.ToInt32(enumeration);
            IsValid = true;
            HasValue = true;
            return;
        }


        IsValid = false;
    }

}