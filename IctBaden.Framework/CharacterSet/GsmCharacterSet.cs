using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.CharacterSet
{
    public static class GsmCharacterSet
    {
        // ` is not a conversion, just a untranslatable letter
        private const string GsmTable =
            "@£$¥èéùìòÇ\nØø\rÅå" +
            "Δ_ΦΓΛΩΠΨΣΘΞ`ÆæßÉ" +
            " !\"#¤%&'()*+,-./" +
            "0123456789:;<=>?" +
            "¡ABCDEFGHIJKLMNO" +
            "PQRSTUVWXYZÄÖÑÜ§" +
            "¿abcdefghijklmno" +
            "pqrstuvwxyzäöñüà";

        private const string ExtendedTable =
            "````````````````" +
            "````^```````````" +
            "````````{}`````\\" +
            "````````````[~]`" +
            "|```````````````" +
            "````````````````" +
            "`````€``````````" +
            "````````````````";

        public static byte[] GetBytes(string text)
        {
            var data = new List<byte>();
            foreach (var cPlainText in text)
            {
                int intGsmTable = GsmTable.IndexOf(cPlainText);
                if (intGsmTable != -1)
                {
                    data.Add((byte)intGsmTable);
                    continue;
                }
                int intExtendedTable = ExtendedTable.IndexOf(cPlainText);
                if (intExtendedTable != -1)
                {
                    data.Add(27);
                    data.Add((byte)intExtendedTable);
                }
            }
            return data.ToArray();
        }

        public static string GetString(byte[] data)
        {
            var text = string.Empty;
            var esc = false;
            foreach (var dByte in data)
            {
                if (dByte == 27)
                {
                    esc = true;
                    continue;
                }
                text += (esc) ? ExtendedTable[dByte] : GsmTable[dByte];
            }
            return text;
        }

    }
}
