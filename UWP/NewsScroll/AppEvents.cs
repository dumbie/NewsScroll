using ArnoldVinkMessageBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using static NewsScroll.Api.Api;
using static NewsScroll.Database.Database;

namespace NewsScroll.Events
{
    public class Events
    {
        public delegate Task DelegateProgressDisableUI(string ProgressMsg, bool DisableInterface);
        public static DelegateProgressDisableUI EventProgressDisableUI = null;

        public delegate Task DelegateProgressEnableUI();
        public static DelegateProgressEnableUI EventProgressEnableUI = null;

        public delegate Task DelegateHideShowHeader(bool ForceClose);
        public static DelegateHideShowHeader EventHideShowHeader = null;

        public delegate Task DelegateHideProgressionStatus();
        public static DelegateHideProgressionStatus EventHideProgressionStatus = null;

        public delegate Task DelegateRefreshPageItems(bool Confirm);
        public static DelegateRefreshPageItems EventRefreshPageItems = null;

        public delegate Task DelegateUpdateTotalItemsCount(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, bool Silent, bool EnableUI);
        public static DelegateUpdateTotalItemsCount EventUpdateTotalItemsCount = null;

        public delegate Task DelegateAdjustItemsScrollingDirection(Int32 Direction);
        public static DelegateAdjustItemsScrollingDirection EventAdjustItemsScrollingDirection = null;

        public delegate void DelegateChangeListViewStyle(Int32 Style);
        public static DelegateChangeListViewStyle EventChangeListViewStyle = null;

        //Register Application Events
        public static void EventsRegister()
        {
            try
            {
                Debug.WriteLine("Registering application events...");

                //Create event to check internet connection
                NetworkInformation.NetworkStatusChanged += CheckInternetConnection;
            }
            catch { }
        }

        private static async void CheckInternetConnection(object sender)
        {
            try
            {
                bool CurrentOnlineStatus = NetworkInterface.GetIsNetworkAvailable();

                //Check if internet connection has changed
                if (CurrentOnlineStatus && !AppVariables.PreviousOnlineStatus)
                {
                    await AVMessageBox.Popup("Internet now available", "It seems like you have an internet connection available, you can now refresh the feeds and items, your offline starred and read items will now be synced.", "Ok", "", "", "", "", false);
                    await SyncOfflineChanges(false, true);
                }

                AppVariables.PreviousOnlineStatus = CurrentOnlineStatus;
            }
            catch { }
        }

        //Disable Application Events
        public static void EventsDisable()
        {
            try
            {
                Debug.WriteLine("Disabling application events...");

                //Disable event to check internet connection
                NetworkInformation.NetworkStatusChanged -= CheckInternetConnection;
            }
            catch { }
        }
    }
}