using System.Diagnostics.CodeAnalysis;
using System.Drawing;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Framework.Tron;

[SuppressMessage("Design", "MA0069:Non-constant static fields should not be visible")]
[SuppressMessage("Usage", "CA2211:Nicht konstante Felder dürfen nicht sichtbar sein")]
public static class TraceColor
{
    public static Color Red = Color.FromArgb(255, 32, 32);
    public static Color LightRed = Color.FromArgb(255, 128, 128);
    public static Color DarkRed = Color.FromArgb(64, 0, 0);

    public static Color Green = Color.FromArgb(0, 128, 0);
    public static Color LightGreen = Color.FromArgb(0, 255, 0);
    public static Color DarkGreen = Color.FromArgb(0, 64, 0);

    public static Color Blue = Color.FromArgb(0, 0, 255);
    public static Color LightBlue = Color.FromArgb(128, 128, 255);
    public static Color DarkBlue = Color.FromArgb(0, 0, 128);

    public static Color Yellow = Color.FromArgb(255, 255, 0);

    public static Color Black = Color.FromArgb(0, 0, 0);
    public static Color White = Color.FromArgb(255, 255, 255);

    public static Color LightGray = Color.FromArgb(192, 192, 192);
    public static Color Gray = Color.FromArgb(128, 128, 128);
    public static Color DarkGray = Color.FromArgb(64, 64, 64);

    public static Color Text = Black;
    public static Color Info = Blue;
    public static Color Warning = Color.Orange;
    public static Color Error = Red;
    public static Color FatalError = Color.FromArgb(0x80, 255, 0, 0);
    public static Color Ok = Green;
}