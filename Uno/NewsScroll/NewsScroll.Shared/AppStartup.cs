﻿using ArnoldVinkCode;
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

                //Adjust screen rotation
                AppAdjust.AdjustScreenRotation();

                //Monitor screen rotation
                DisplayInformation.GetForCurrentView().OrientationChanged += AppAdjust.AdjustListviewRotationEvent;

                //Update internet access
                AppVariables.UpdateInternetAccess();

                //Calculate the maximum scroll load items
                Size ScreenResolution = AVFunctions.DevScreenResolution();
                if (ScreenResolution.Width > ScreenResolution.Height)
                {
                    AppVariables.ContentToScrollLoad = Convert.ToInt32(ScreenResolution.Width / 280) + 4;
                }
                else
                {
                    AppVariables.ContentToScrollLoad = Convert.ToInt32(ScreenResolution.Height / 280) + 4;
                }
                System.Diagnostics.Debug.WriteLine("Scroll load items have been set to: " + AppVariables.ContentToScrollLoad);

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

                //Prevent application lock screen
                try { AppVariables.DisplayRequest.RequestActive(); } catch { }
            }
            catch { }
        }
    }
}