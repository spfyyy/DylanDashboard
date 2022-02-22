using System.Diagnostics;

namespace DylanDashboard
{
    public static class Utils
    {
        public static Process? StartProcess(ProcessStartInfo processInfo)
        {
            try
            {
                return Process.Start(processInfo);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
