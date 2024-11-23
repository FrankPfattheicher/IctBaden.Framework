using System.Drawing;

namespace IctBaden.Framework.UserInterface;

internal interface IDisplayInfo
{
    int GetMonitorCount();
    float GetScalingFactor(int monitor = 1);
    
    Rectangle GetVirtualScreen();
}