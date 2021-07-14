using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class StarredPage : ContentPage
    {
        //Page Variables
        private static int vPreviousScrollItem = 0;

        public StarredPage()
        {
            InitializeComponent();
            Page_Loaded();
        }

        private async void Page_Loaded()
        {
            try
            {
                //Check the set account
                if (!CheckAccount())
                {
                    await CleanupPageResources();
                    App.NavigateToPage(new SettingsPage(), true, false);
                    return;
                }

                //Register page events
                RegisterPageEvents();

                //Show page status
                ProgressDisableUI("Preparing starred page...", true);

                //Adjust the scrolling direction
                ChangeListViewDirection(Convert.ToInt32(AppSettingLoad("ListViewDirection")));

                //Adjust the list view style
                ChangeListViewStyle(Convert.ToInt32(AppSettingLoad("ListViewStyle")));

                //Adjust the swiping direction
                SwipeBarAdjust();

                //Bind list to ListView
                listview_Items.ItemsSource = List_StarredItems;

                //Load all the items
                await LoadItems();
            }
            catch { }
        }

        //Register page events
        private void RegisterPageEvents()
        {
            try
            {
                //Register popup and load events
                EventProgressDisableUI += new DelegateProgressDisableUI(ProgressDisableUI);
                EventProgressEnableUI += new DelegateProgressEnableUI(ProgressEnableUI);
                EventHideShowHeader += new DelegateHideShowHeader(HideShowHeader);
                EventHideProgressionStatus += new DelegateHideProgressionStatus(HideProgressionStatus);
                EventUpdateTotalItemsCount += new DelegateUpdateTotalItemsCount(UpdateTotalItemsCount);
                EventChangeListViewDirection += new DelegateChangeListViewDirection(ChangeListViewDirection);
                EventChangeListViewStyle += new DelegateChangeListViewStyle(ChangeListViewStyle);

                //Register ListView events
                listview_Items.ItemTapped += EventsListView.listview_Items_Tapped;

                //Register ListView scroll viewer
                listview_Items.ItemAppearing += ScrollViewer_ItemAppearing;
                listview_Items.ItemDisappearing += ScrollViewer_ItemDisappearing;

                //Register item status count events
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += button_StatusCurrentItem_Tapped;
                button_StatusCurrentItem.GestureRecognizers.Add(tapGestureRecognizer);

                //Monitor user touch swipe
                if (!(bool)AppSettingLoad("DisableSwipeActions"))
                {
                    SwipeBarAdjust();
                }
            }
            catch { }
        }

        //Disable page events
        private void DisablePageEvents()
        {
            try
            {
                //Register popup and load events
                EventProgressDisableUI -= new DelegateProgressDisableUI(ProgressDisableUI);
                EventProgressEnableUI -= new DelegateProgressEnableUI(ProgressEnableUI);
                EventHideShowHeader -= new DelegateHideShowHeader(HideShowHeader);
                EventHideProgressionStatus -= new DelegateHideProgressionStatus(HideProgressionStatus);
                EventUpdateTotalItemsCount -= new DelegateUpdateTotalItemsCount(UpdateTotalItemsCount);
                EventChangeListViewDirection -= new DelegateChangeListViewDirection(ChangeListViewDirection);
                EventChangeListViewStyle -= new DelegateChangeListViewStyle(ChangeListViewStyle);

                //Register ListView events
                listview_Items.ItemTapped -= EventsListView.listview_Items_Tapped;

                //Register ListView scroll viewer
                listview_Items.ItemAppearing -= ScrollViewer_ItemAppearing;
                listview_Items.ItemDisappearing -= ScrollViewer_ItemDisappearing;

                //Monitor user touch swipe
                grid_SwipeBar.GestureRecognizers.Clear();
            }
            catch { }
        }

        //Cleanup page resources
        async Task CleanupPageResources()
        {
            try
            {
                Debug.WriteLine("Clearing page resources...");

                DisablePageEvents();

                //listview_Items.ItemContainerStyle = null;
                listview_Items.ItemsSource = null;
                //listview_Items.ItemsPanel = null;
                //listview_Items.Items.Clear();

                await ClearObservableCollection(List_Feeds);
                await ClearObservableCollection(List_FeedSelect);
                await ClearObservableCollection(List_NewsItems);
                await ClearObservableCollection(List_SearchItems);
                await ClearObservableCollection(List_StarredItems);
            }
            catch { }
        }

        //Update Api Start
        async Task LoadItems()
        {
            try
            {
                //Clear all items and reset load count
                await ClearObservableCollection(List_StarredItems);

                //Update the loading information
                txt_AppInfo.Text = "Loading items";
                txt_NewsScrollInfo.Text = "Your starred items will be shown here shortly...";
                txt_NewsScrollInfo.IsVisible = true;

                //Load items from api/database
                AppVariables.LoadNews = false;
                AppVariables.LoadStarred = true;
                AppVariables.LoadSearch = false;
                AppVariables.LoadFeeds = true;
                int Result = await ApiUpdate.PageApiUpdate();

                //Check the api update result
                if (Result == 2)
                {
                    await CleanupPageResources();
                    App.NavigateToPage(new SettingsPage(), true, false);
                    return;
                }

                //Set all items to list
                List<TableItems> LoadTableItems = await vSQLConnection.Table<TableItems>().ToListAsync();

                //Load items into the list
                await ProcessItemLoad.DatabaseToList(null, LoadTableItems, AppVariables.CurrentItemsLoaded, AppVariables.ItemsToLoadMax, false, false);

                //Update the total items count
                await UpdateTotalItemsCount(null, LoadTableItems, false, true);
            }
            catch { }
        }

        //Hide the progression status
        private void HideProgressionStatus()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    txt_NewsScrollInfo.IsVisible = false;
                }
                catch { }
            });
        }

        //Item status scroll events
        private async void button_StatusCurrentItem_Tapped(object sender, EventArgs e)
        {
            try
            {
                bool Scrolled = await EventsItemStatus.ListViewScroller(listview_Items, AppVariables.CurrentViewItemsCount, vPreviousScrollItem);
                if (Scrolled)
                {
                    vPreviousScrollItem = AppVariables.CurrentViewItemsCount;
                }
            }
            catch { }
        }

        //Progressbar/UI Status
        void ProgressDisableUI(string ProgressMsg, bool DisableInterface)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    AppVariables.BusyApplication = true;

                    //Enable progressbar
                    label_StatusApplication.Text = ProgressMsg;
                    grid_StatusApplication.IsVisible = true;

                    if (DisableInterface)
                    {
                        //Disable content
                        button_StatusCurrentItem.IsEnabled = false;
                        button_StatusCurrentItem.Opacity = 0.30;

                        //Disable buttons
                        iconApi.IsEnabled = false;
                        iconApi.Opacity = 0.30;
                        iconNews.IsEnabled = false;
                        iconNews.Opacity = 0.30;
                        iconSearch.IsEnabled = false;
                        iconSearch.Opacity = 0.30;
                        iconSettings.IsEnabled = false;
                        iconSettings.Opacity = 0.30;
                        iconRefresh.IsEnabled = false;
                        iconRefresh.Opacity = 0.30;
                    }
                }
                catch { AppVariables.BusyApplication = true; }
            });
        }

        void ProgressEnableUI()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    //Disable progressbar
                    grid_StatusApplication.IsVisible = false;
                    label_StatusApplication.Text = string.Empty;

                    //Enable content
                    button_StatusCurrentItem.IsEnabled = true;
                    button_StatusCurrentItem.Opacity = 1;

                    //Enable buttons
                    iconApi.IsEnabled = true;
                    iconApi.Opacity = 1;
                    iconNews.IsEnabled = true;
                    iconNews.Opacity = 1;
                    iconSearch.IsEnabled = true;
                    iconSearch.Opacity = 1;
                    iconSettings.IsEnabled = true;
                    iconSettings.Opacity = 1;
                    iconRefresh.IsEnabled = true;
                    iconRefresh.Opacity = 1;

                    AppVariables.BusyApplication = false;
                }
                catch { AppVariables.BusyApplication = false; }
            });
        }

        //User Interface - Buttons
        async void iconSearch_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.NavigateToPage(new SearchPage(), true, false);
            }
            catch { }
        }

        async void iconNews_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.NavigateToPage(new NewsPage(), true, false);
            }
            catch { }
        }

        async void iconRefresh_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                await RefreshItems();
            }
            catch { }
        }

        private async Task RefreshItems()
        {
            try
            {
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Refresh starred items");
                messageAnswers.Add("Cancel");

                string messageResult = await MessagePopup.Popup("Refresh starred items", "Do you want to refresh starred items and scroll to the top?", messageAnswers);
                if (messageResult == "Refresh starred items")
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateStarred = true;
                    ApiMessageError = string.Empty;

                    //Load all the items
                    await LoadItems();
                }
            }
            catch { }
        }

        async void iconApi_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.NavigateToPage(new ApiPage(), true, false);
            }
            catch { }
        }

        private void iconPersonalize_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                PersonalizePopup.Popup();
            }
            catch { }
        }

        async void iconSettings_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.NavigateToPage(new SettingsPage(), true, false);
            }
            catch { }
        }
    }
}