using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace NewsScroll
{
    class AppTimers
    {
        //Application Timers
        public static DispatcherTimer vDispatcherTimer_MemoryCleanup = new DispatcherTimer();

        //Register Application Timers
        public static void TimersRegister()
        {
            try
            {
                Debug.WriteLine("Registering application timers...");

                //Create timer to cleanup the memory left behind
                vDispatcherTimer_MemoryCleanup.Interval = TimeSpan.FromSeconds(5);
                vDispatcherTimer_MemoryCleanup.Tick += delegate { try { GC.Collect(); } catch { } };
                vDispatcherTimer_MemoryCleanup.Start();
            }
            catch { }
        }

        //Disable Application Timers
        public static void TimersDisable()
        {
            try
            {
                Debug.WriteLine("Disabling application timers...");

                vDispatcherTimer_MemoryCleanup.Stop();
            }
            catch { }
        }
    }
}