using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Essentials;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;

namespace NewsScroll
{
    partial class AppTimers
    {
        private static void vTimer_MemoryCleanup_Function()
        {
            try
            {
                GC.Collect();
            }
            catch { }
        }

        private static async void vTimer_InternetCheck_Function()
        {
            try
            {
                bool currentOnlineStatus = Connectivity.NetworkAccess == NetworkAccess.Internet;

                //Check if media needs to load
                AppVariables.LoadMedia = true;
                if (!currentOnlineStatus && !(bool)AppSettingLoad("DisplayImagesOffline"))
                {
                    AppVariables.LoadMedia = false;
                }

                //Check if internet connection has changed
                if (currentOnlineStatus && !AppVariables.PreviousOnlineStatus)
                {
                    Debug.WriteLine("Connectivity changed, internet available: " + currentOnlineStatus);
                    AppVariables.PreviousOnlineStatus = currentOnlineStatus;

                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("Internet now available", "It seems like you have an internet connection available, you can now refresh the feeds and items, your offline starred and read items will now be synced.", messageAnswers);
                    await SyncOfflineChanges(false, true);
                }
            }
            catch { }
        }
    }
}