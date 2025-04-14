using System;
using System.Text;
using System.Globalization;
using System.Diagnostics;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.CharacterSet;

public static class Ucs2CharacterSet
{
    public static string EncodeText(string textToEncode)
    {
        var unicodeText = new StringBuilder();
        foreach (var uniChar in textToEncode)
        {
            unicodeText.Append(CultureInfo.InvariantCulture, $"{(uint)uniChar:X4}");
        }
        return unicodeText.ToString();
    }

    public static string? DecodeText(string textToDecode)
    {
        try
        {
            var decoded = new StringBuilder();
            for (var cx = 0; cx < textToDecode.Length; cx += 4)
            {
                if (!int.TryParse(textToDecode.AsSpan(cx, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var utf16))
                    break;
                decoded.Append((char)utf16);
            }
            return (decoded.Length > 0) ? decoded.ToString() : textToDecode;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Trace.TraceError(ex.Message);
        }
        return null;
    }
}