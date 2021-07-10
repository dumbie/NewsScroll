using System.Diagnostics;
using System.Timers;

namespace NewsScroll
{
    partial class AppTimers
    {
        //Application Timers
        public static Timer vTimer_CheckInternet = new Timer();
        public static Timer vTimer_MemoryCleanup = new Timer();

        //Register Application Timers
        public static void TimersRegister()
        {
            try
            {
                Debug.WriteLine("Registering application timers...");

                //Create timer to cleanup the memory left behind
                vTimer_MemoryCleanup.Interval = 5000;
                vTimer_MemoryCleanup.Elapsed += delegate { vTimer_MemoryCleanup_Function(); };
                vTimer_MemoryCleanup.Start();

                //Create timer to check internet connection
                vTimer_CheckInternet.Interval = 3000;
                vTimer_CheckInternet.Elapsed += delegate { vTimer_InternetCheck_Function(); };
                vTimer_CheckInternet.Start();
            }
            catch { }
        }

        //Disable Application Timers
        public static void TimersDisable()
        {
            try
            {
                Debug.WriteLine("Disabling application timers...");

                vTimer_MemoryCleanup.Stop();
                vTimer_CheckInternet.Stop();
            }
            catch { }
        }
    }
}