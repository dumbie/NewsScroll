using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
            }
            catch { }
        }

        //Disable Application Events
        public static void EventsDisable()
        {
            try
            {
                Debug.WriteLine("Disabling application events...");
            }
            catch { }
        }
    }
}