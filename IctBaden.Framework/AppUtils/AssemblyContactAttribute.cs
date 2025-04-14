using System;
using System.Runtime.InteropServices;

namespace IctBaden.Framework.AppUtils;

[AttributeUsage(AttributeTargets.Assembly), ComVisible(true)]
public sealed class AssemblyContactAttribute : Attribute
{
  public string? Address { get; set; }
  public string? City { get; set; }
  public string? Mail { get; set; }
  public string? Phone { get; set; }
  public string? Fax { get; set; }
  public string? Mobile { get; set; }
  public string? Url { get; set; }
}