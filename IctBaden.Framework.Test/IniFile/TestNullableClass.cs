using System.Collections.Generic;

namespace IctBaden.Framework.Test.IniFile;

public class TestNullableClass
{
    public bool? Boolean1 { get; set; }
    public bool? BooleanTrue { get; set; }
    public int? Integer { get; set; }
    public float? Numeric1 { get; set; }
    public double? Numeric2 { get; set; }
    public string? Text { get; set; }
    public List<string>? TextList { get; set; }
    public string[]? TextArray { get; set; }
    public List<int>? IntList { get; set; }
}
