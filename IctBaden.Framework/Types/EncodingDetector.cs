using System;

namespace IctBaden.Framework.Types
{
    public static class EncodingDetector
    {
        public static bool IsUnicode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            if ((text.Length % 2) != 0)
                return false;
            var limit = Math.Min(10, text.Length / 2);
            for (var ix = 1; ix < limit; ix += 2)
            {
                if (text[ix] != '\0')
                    return false;
            }
            return true;
        }
    }
}
