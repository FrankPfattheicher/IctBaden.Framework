using System;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace IctBaden.Framework.CharacterSet
{
  public class Ucs2CharacterSet
  {
    public static string EncodeText(string textToEncode)
    {
      var unicodeText = new StringBuilder();
      foreach (var uniChar in textToEncode)
      {
        unicodeText.AppendFormat("{0:X4}", (uint)uniChar);
      }
      return unicodeText.ToString();
    }

    public static string DecodeText(string textToDecode)
    {
      try
      {
        var decoded = new StringBuilder();
        for (var cx = 0; cx < textToDecode.Length; cx += 4)
        {
          int utf16;
          if (!int.TryParse(textToDecode.Substring(cx, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out utf16))
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
}

