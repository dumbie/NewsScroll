using System;
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

        private static async void vTimer_CheckInternet_Function()
        {
            try
            {
                //Update internet access
                AppVariables.UpdateInternetAccess();

                //Check if media needs to load
                if (!AppVariables.InternetAccess && !(bool)AppVariables.ApplicationSettings["DisplayImagesOffline"])
                {
                    AppVariables.LoadMedia = false;
                }
                else
                {
                    AppVariables.LoadMedia = true;
                }

                //Check if internet connection has changed
                if (AppVariables.InternetAccess && !AppVariables.PreviousInternetAccess)
                {
                    await new MessagePopup().OpenPopup("Internet now available", "It seems like you have an internet connection available, you can now refresh the feeds and items, your offline starred and read items will now be synced.", "Ok", "", "", "", "", false);
                    await SyncOfflineChanges(false, true);
                }
            }
            catch { }
        }
    }
}