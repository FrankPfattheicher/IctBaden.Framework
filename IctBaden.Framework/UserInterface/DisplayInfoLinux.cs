using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace IctBaden.Framework.UserInterface;

[SuppressMessage("Design", "MA0026:Fix TODO comment")]
internal class DisplayInfoLinux : IDisplayInfo
{
    public int GetMonitorCount()
    {
        return 1;
    }

    public float GetScalingFactor(int monitor = 1)
    {
        //TODO
        return 1.0f;
    }

    public Rectangle GetVirtualScreen()
    {
        return new Rectangle(0, 0, 0, 0);
    }
}