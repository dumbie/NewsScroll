using System.Collections.Generic;
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

        public delegate Task DelegateAdjustItemsScrollingDirection(int Direction);
        public static DelegateAdjustItemsScrollingDirection EventAdjustItemsScrollingDirection = null;

        public delegate void DelegateChangeListViewStyle(int Style);
        public static DelegateChangeListViewStyle EventChangeListViewStyle = null;

        //Register Application Events
        public static void EventsRegister()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Registering application events...");

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

                //Check if media needs to load
                if (!CurrentOnlineStatus && !(bool)AppVariables.ApplicationSettings["DisplayImagesOffline"])
                {
                    AppVariables.LoadMedia = false;
                }
                else
                {
                    AppVariables.LoadMedia = true;
                }

                //Check if internet connection has changed
                if (CurrentOnlineStatus && !AppVariables.PreviousOnlineStatus)
                {
                    await MessagePopup.Popup("Internet now available", "It seems like you have an internet connection available, you can now refresh the feeds and items, your offline starred and read items will now be synced.", "Ok", "", "", "", "", false);
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
                System.Diagnostics.Debug.WriteLine("Disabling application events...");

                //Disable event to check internet connection
                NetworkInformation.NetworkStatusChanged -= CheckInternetConnection;
            }
            catch { }
        }
    }
}