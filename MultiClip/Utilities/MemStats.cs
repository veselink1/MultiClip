using System.ComponentModel;
using MultiClip.Native;

namespace MultiClip.Utilities
{
    public static class MemStats
    {
        public static long GetTotalPhysicalMemory()
        {
            var memStat = NativeMethods.MEMORYSTATUSEX.Create();
            if (!NativeMethods.GlobalMemoryStatusEx(ref memStat))
            {
                throw new Win32Exception();
            }
            return (long)memStat.ullTotalPhys;
        }
    }
}
