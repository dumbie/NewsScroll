using ArnoldVinkCode;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using static NewsScroll.Database.Database;

namespace NewsScroll.Startup
{
    class Startup
    {
        public static async Task ApplicationStartup()
        {
            try
            {
                //Check application settings
                SettingsPage.SettingsCheck();

                //Adjust the color theme
                AppAdjust.AdjustColorTheme();

                //Adjust the font sizes
                AppAdjust.AdjustFontSizes();

                //Adjust application user agent
                //AppAdjust.AdjustUserAgent();

                //Set Landscape Display
                if ((bool)AppVariables.ApplicationSettings["DisableLandscapeDisplay"])
                {
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                }
                else
                {
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape;
                }

                //Calculate the maximum preload items
                Size ScreenResolution = AVFunctions.DevScreenResolution();
                if (ScreenResolution.Width > ScreenResolution.Height) { AppVariables.ContentToScrollLoad = Convert.ToInt32(ScreenResolution.Width / 280) + 4; }
                else { AppVariables.ContentToScrollLoad = Convert.ToInt32(ScreenResolution.Height / 280) + 4; }
                System.Diagnostics.Debug.WriteLine("Preload items has been set to: " + AppVariables.ContentToScrollLoad);

                //Connect to the database
                if (!DatabaseConnect())
                {
                    MessageDialog MessageDialog = new MessageDialog("Your database will be cleared, please restart the application to continue.", "Failed to connect to the database");
                    await MessageDialog.ShowAsync();

                    await DatabaseReset();
                    Application.Current.Exit();
                    return;
                }

                //Create the database tables
                await DatabaseCreate();

                //Register application timers
                AppTimers.TimersRegister();

                //Register application events
                Events.Events.EventsRegister();

                //Check if all items need to load
                if ((bool)AppVariables.ApplicationSettings["LoadAllItems"]) { AppVariables.ItemsToScrollLoad = 100000; }

                //Prevent application lock screen
                try { AppVariables.DisplayRequest.RequestActive(); } catch { }
            }
            catch { }
        }

        //Switch between screen modes
        public static void SwitchScreenMode()
        {
            try
            {
                if (!AVFunctions.DevMobile())
                {
                    System.Diagnostics.Debug.WriteLine("Switching between screen modes.");
                    if (AppVariables.ApplicationView.IsFullScreenMode)
                    {
                        try { AppVariables.ApplicationView.ExitFullScreenMode(); } catch { }
                    }
                    else
                    {
                        try { AppVariables.ApplicationView.TryEnterFullScreenMode(); } catch { }
                    }
                }
            }
            catch { }
        }
    }
}