using System;
using Windows.UI.Xaml;

namespace NewsScroll
{
    partial class AppTimers
    {
        //Application Timers
        public static DispatcherTimer vDispatcherTimer_CheckInternet = new DispatcherTimer();
        public static DispatcherTimer vDispatcherTimer_MemoryCleanup = new DispatcherTimer();

        //Register Application Timers
        public static void TimersRegister()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Registering application timers...");

                //Create timer to check internet connection
                vDispatcherTimer_CheckInternet.Interval = TimeSpan.FromSeconds(2);
                vDispatcherTimer_CheckInternet.Tick += delegate { vTimer_CheckInternet_Function(); };
                vDispatcherTimer_CheckInternet.Start();

                //Create timer to cleanup the memory left behind
                vDispatcherTimer_MemoryCleanup.Interval = TimeSpan.FromSeconds(5);
                vDispatcherTimer_MemoryCleanup.Tick += delegate { vTimer_MemoryCleanup_Function(); };
                vDispatcherTimer_MemoryCleanup.Start();
            }
            catch { }
        }

        //Disable Application Timers
        public static void TimersDisable()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Disabling application timers...");

                vDispatcherTimer_CheckInternet.Stop();
                vDispatcherTimer_MemoryCleanup.Stop();
            }
            catch { }
        }
    }
}