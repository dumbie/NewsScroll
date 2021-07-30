using System.Collections.Generic;
using System.Threading.Tasks;
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
    }
}