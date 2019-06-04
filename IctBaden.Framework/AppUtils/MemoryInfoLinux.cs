namespace IctBaden.Framework.AppUtils
{
    public class MemoryInfoLinux : IMemoryInfo
    {
        public ulong GetSystemRamInstalled()
        {
            throw new System.NotImplementedException();
        }

        public float GetSystemRamUsage()
        {
            throw new System.NotImplementedException();
        }

        public ulong GetSystemTotalVirtualMemory()
        {
            throw new System.NotImplementedException();
        }

        public float GetSystemVirtualMemoryUsage()
        {
            throw new System.NotImplementedException();
        }
    }
}