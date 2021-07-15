using ArnoldVinkCode;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.AppTimers;
using static NewsScroll.Cleanup;
using static NewsScroll.Database.Database;

namespace NewsScroll.AppStartup
{
    class AppStartup
    {
        public static async Task ApplicationStart()
        {
            try
            {
                Debug.WriteLine("NewsScroll startup checks.");

                //Check application settings
                await SettingsPage.SettingsCheck();

                //Create image cache folder
                AVFiles.Directory_Create(@"Cache", false, true);

                //Cleanup image download cache
                CleanImageDownloadCache();

                //Adjust the color theme
                AppAdjust.AdjustColorTheme();

                //Adjust the font sizes
                AppAdjust.AdjustFontSizes();

                //Connect to the database
                if (!DatabaseConnect())
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    string messageResult = await MessagePopup.Popup("Failed to connect to the database", "Your database will be cleared, please restart the application to continue.", messageAnswers);

                    await DatabaseReset();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    return;
                }

                //Create the database tables
                await DatabaseCreate();

                //Register application timers
                TimersRegister();

                //Register Application Events
                EventsRegister();

                //Prevent application lock screen
                DeviceDisplay.KeepScreenOn = true;
            }
            catch { }
        }
    }
}