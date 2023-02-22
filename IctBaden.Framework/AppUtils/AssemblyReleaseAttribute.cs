using System;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.AppUtils
{
  [AttributeUsage(AttributeTargets.Assembly), ComVisible(true)]
  public sealed class AssemblyReleaseAttribute : Attribute
  {
    public string? Date { get; set; }
  }
}