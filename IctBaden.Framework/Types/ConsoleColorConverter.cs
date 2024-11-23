using System;
using System.Drawing;

namespace IctBaden.Framework.Types;

public static class ConsoleColorConverter
{
    public static bool TryGetConsoleColor(Color color, out ConsoleColor consoleColor)
    {
        foreach (var property in typeof(Color).GetProperties())
        {
            var value = property.GetValue(color, index: null) as Color?;
            if (color.ToArgb() != value?.ToArgb()) continue;

            var index = Array.IndexOf(Enum.GetNames(typeof(ConsoleColor)), property.Name);
            if (index == -1) continue;

            var cc = (ConsoleColor?)Enum.GetValues(typeof(ConsoleColor)).GetValue(index);
            if (cc != null)
            {
                consoleColor = (ConsoleColor)cc;
                return true;
            }
        }
        consoleColor = default;
        return false;
    }
}