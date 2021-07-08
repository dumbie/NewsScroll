using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static NewsScroll.Api.Api;
using static NewsScroll.Database.Database;

namespace NewsScroll.AppEvents
{
    public class AppEvents
    {
        public delegate void DelegateProgressDisableUI(string ProgressMsg, bool DisableInterface);
        public static DelegateProgressDisableUI EventProgressDisableUI = null;

        public delegate void DelegateProgressEnableUI();
        public static DelegateProgressEnableUI EventProgressEnableUI = null;

        public delegate void DelegateHideShowHeader(bool ForceClose);
        public static DelegateHideShowHeader EventHideShowHeader = null;

        public delegate void DelegateHideProgressionStatus();
        public static DelegateHideProgressionStatus EventHideProgressionStatus = null;

        public delegate Task DelegateRefreshPageItems(bool Confirm);
        public static DelegateRefreshPageItems EventRefreshPageItems = null;

        public delegate Task DelegateUpdateTotalItemsCount(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, bool Silent, bool EnableUI);
        public static DelegateUpdateTotalItemsCount EventUpdateTotalItemsCount = null;

        public delegate void DelegateChangeListViewDirection(int Direction);
        public static DelegateChangeListViewDirection EventChangeListViewDirection = null;

        public delegate void DelegateChangeListViewStyle(int Style);
        public static DelegateChangeListViewStyle EventChangeListViewStyle = null;

        //Register Application Events
        public static void EventsRegister()
        {
            try
            {
                Debug.WriteLine("Registering application events...");

                //Create event to check internet connection
                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            }
            catch { }
        }

        private static async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            try
            {
                bool currentOnlineStatus = NetworkInterface.GetIsNetworkAvailable();
                Debug.WriteLine("Connectivity changed, internet available: " + currentOnlineStatus);

                //Check if internet connection has changed
                if (currentOnlineStatus && !AppVariables.PreviousOnlineStatus)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("Internet now available", "It seems like you have an internet connection available, you can now refresh the feeds and items, your offline starred and read items will now be synced.", messageAnswers);
                    await SyncOfflineChanges(false, true);
                }

                AppVariables.PreviousOnlineStatus = currentOnlineStatus;
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
                Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            }
            catch { }
        }
    }
}