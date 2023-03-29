using System;
using System.IO;
using System.Linq;
using System.Text;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.FileSystem
{
    public static class FileEncoding
    {
        private const int MaxDetectionLength = 10*1024;
        public static Encoding DetectTextFileEncoding(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return Encoding.Default;
            }

            var bom = new byte[4];
            byte[] data;
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                if (file.Length == 0)
                {
                    return Encoding.Default;
                }

                var read = file.Read(bom, 0, 4);
                if (read == 4)
                {
                    // Analyze the BOM
                    if (bom[0] == 0x2B && bom[1] == 0x2F && bom[2] == 0x76)
#pragma warning disable SYSLIB0001
                        return Encoding.UTF7;
#pragma warning restore SYSLIB0001
                    if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                        return Encoding.UTF8;
                    if (bom[0] == 0xFF && bom[1] == 0xFE)
                        return Encoding.Unicode; // UTF-16LE
                    if (bom[0] == 0xFE && bom[1] == 0xFF)
                        return Encoding.BigEndianUnicode; // UTF-16BE
                    if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
                        return Encoding.UTF32;
                }

                file.Seek(0, SeekOrigin.Begin);
                data = new byte[Math.Min(file.Length, MaxDetectionLength)];
                read = file.Read(data, 0, data.Length);
                if(read == 0)
                {
                    return Encoding.Default;
                }
            }

            // Analyze content
            var text = Encoding.UTF8.GetString(data);
            var enc = Encoding.UTF8.GetBytes(text);
            //if (text.Contains("\uFFFD"))

            if(data.Zip(enc, (b1, b2) => b1 == b2).All(eq => eq))
            {
                return Encoding.UTF8;
            }

            text = Encoding.GetEncoding(1252).GetString(data);
            enc = Encoding.GetEncoding(1252).GetBytes(text);
            if (data.Zip(enc, (b1, b2) => b1 == b2).All(eq => eq))
            {
                return Encoding.GetEncoding(1252);
            }

            return Encoding.ASCII;
        }
    }
}