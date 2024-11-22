using System.Diagnostics.CodeAnalysis;

namespace IctBaden.Framework.UserInterface;

[SuppressMessage("Design", "MA0026:Fix TODO comment")]
public class DisplayScalingLinux : IDisplayScaling
{
    public float GetScalingFactor()
    {
        //TODO
        return 1.0f;
    }
}