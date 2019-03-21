using System;
using System.Runtime.InteropServices;

namespace IctBaden.Framework.AppUtils
{
  [AttributeUsage(AttributeTargets.Assembly, Inherited = false), ComVisible(true)]
  public sealed class AssemblyReleaseAttribute : Attribute
  {
    public string Date { get; set; }
  }
}