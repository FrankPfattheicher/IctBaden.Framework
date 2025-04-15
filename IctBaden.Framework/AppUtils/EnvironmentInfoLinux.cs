namespace IctBaden.Framework.AppUtils;

public class EnvironmentInfoLinux : IEnvironmentInfo
{
    public bool CanAccessDesktop()
    {
        return true;
    }
}