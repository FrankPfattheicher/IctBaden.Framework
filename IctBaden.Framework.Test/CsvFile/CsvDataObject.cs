// ReSharper disable UnusedMember.Global

using System;

namespace IctBaden.Framework.Test.CsvFile
{
    public class CsvDataObject
    {
        public bool PropertyBool { get; set; } = true;
        public int PropertyInt { get; set; } = 4;
        public long PropertyLong { get; set; } = 4000000;
        public string PropertyString { get; set; } = "test";
        public float PropertyFloat { get; set; } = 12.34f;
        public double PropertyDouble { get; set; } = 56.789;
        public DateTime PropertyDateTime { get; set; } = DateTime.Now;
        public DateTimeOffset PropertyDateTimeOffset { get; set; } = DateTimeOffset.UtcNow;
    }
}