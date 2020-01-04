using System;
using IctBaden.Framework.AppUtils;

namespace IctBaden.Framework.UserInterface
{
    // ReSharper disable once UnusedType.Global
    public class Display
    {
        private static readonly IDisplayScaling DisplayScaling;

        static Display()
        {
            switch (SystemInfo.Platform)
            {
                case Platform.Windows:
                    DisplayScaling = new DisplayScalingWindows();
                    break;
                case Platform.Linux:
                    DisplayScaling = new DisplayScalingLinux();
                    break;
                default:
                    throw new PlatformNotSupportedException("DisplayScaling");
            }
        }

        public static float GetScalingFactor()
        {
            return DisplayScaling.GetScalingFactor();
        }
        
    }
    
}