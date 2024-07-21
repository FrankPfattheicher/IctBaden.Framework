using System;
using System.Collections.Generic;

namespace IctBaden.Framework.Test.IniFile;

public class TestClass
{
    public bool Boolean1 { get; set; }
    public bool BooleanTrue { get; set; }
    public int Integer { get; set; }
    public float Numeric1 { get; set; }
    public double Numeric2 { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> TextList { get; set; } = new();
    public string[] TextArray { get; set; } = Array.Empty<string>();
    public List<int> IntList { get; set; } = new();
    
    public TestSubClass Section1 { get; set; } = new();
    public TestSubClass Section2 { get; set; } = new();

}