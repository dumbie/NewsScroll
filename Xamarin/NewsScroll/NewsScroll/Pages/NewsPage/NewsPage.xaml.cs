using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.AppVariables;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class NewsPage : ContentPage
    {
        //Page Variables
        private int vPreviousScrollItem = 0;

        public NewsPage()
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

                //Adjust the scrolling direction
                ChangeListViewDirection(Convert.ToInt32(AppSettingLoad("ListViewDirection")));

                //Adjust the list view style
                ChangeListViewStyle(Convert.ToInt32(AppSettingLoad("ListViewStyle")));

                //Adjust the swiping direction
                SwipeBarAdjust();

                //Show page status
                ProgressDisableUI("Preparing news page...", true);

                //Bind list to ListView
                listview_Items.ItemsSource = List_NewsItems;
                combobox_FeedSelection.ItemsSource = List_FeedSelect;

                //Load all the items
                await LoadItems(true, false);
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
                EventUpdateTotalItemsCount += new DelegateUpdateTotalItemsCount(UpdateSelectionFeeds);
                EventChangeListViewDirection += new DelegateChangeListViewDirection(ChangeListViewDirection);
                EventChangeListViewStyle += new DelegateChangeListViewStyle(ChangeListViewStyle);
                EventRefreshPageItems += new DelegateRefreshPageItems(RefreshItems);

                //Register ListView events
                listview_Items.ItemTapped += EventsListView.listview_Items_Tapped;

                //Register ListView scroll viewer
                listview_Items.ItemAppearing += ScrollViewer_ItemAppearing;
                listview_Items.ItemDisappearing += ScrollViewer_ItemDisappearing;

                //Register combo box events
                combobox_FeedSelection.SelectedIndexChanged += combobox_FeedSelection_SelectionChanged;

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
                EventUpdateTotalItemsCount -= new DelegateUpdateTotalItemsCount(UpdateSelectionFeeds);
                EventChangeListViewDirection -= new DelegateChangeListViewDirection(ChangeListViewDirection);
                EventChangeListViewStyle -= new DelegateChangeListViewStyle(ChangeListViewStyle);
                EventRefreshPageItems -= new DelegateRefreshPageItems(RefreshItems);

                //Register ListView events
                listview_Items.ItemTapped -= EventsListView.listview_Items_Tapped;

                //Register ListView scroll viewer
                listview_Items.ItemAppearing -= ScrollViewer_ItemAppearing;
                listview_Items.ItemDisappearing -= ScrollViewer_ItemDisappearing;

                //Register combo box events
                combobox_FeedSelection.SelectedIndexChanged -= combobox_FeedSelection_SelectionChanged;

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
                vNewsFeed = null;

                listview_Items.ItemsSource = null;
                combobox_FeedSelection.ItemsSource = null;

                await ClearObservableCollection(List_Feeds);
                await ClearObservableCollection(List_FeedSelect);
                await ClearObservableCollection(List_NewsItems);
                await ClearObservableCollection(List_SearchItems);
                await ClearObservableCollection(List_StarredItems);
            }
            catch { }
        }

        private async void combobox_FeedSelection_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                //Load selected feed items
                Picker ComboBoxSender = sender as Picker;
                Feeds SelectedItem = ComboBoxSender.SelectedItem as Feeds;
                if (SelectedItem != null)
                {
                    string CheckKind = "feed";
                    if (SelectedItem.feed_folder_status) { CheckKind = "folder"; }
                    else if (SelectedItem.feed_collection_status) { CheckKind = "collection"; }

                    ProgressDisableUI("Checking " + CheckKind + " items...", true);
                    Debug.WriteLine("Checking " + CheckKind + " items...");

                    //Set the loading feed or folder
                    vNewsFeed = SelectedItem;

                    //Reset the previous scroll item
                    vPreviousScrollItem = 0;

                    //Load all the items
                    await LoadItems(false, false);

                    ProgressEnableUI();
                }
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
                        //Disable feed selection
                        combobox_FeedSelection.IsEnabled = false;
                        combobox_FeedSelection.Opacity = 0.30;

                        //Disable content
                        button_StatusCurrentItem.IsEnabled = false;
                        button_StatusCurrentItem.Opacity = 0.30;

                        //Disable buttons
                        iconApi.IsEnabled = false;
                        iconApi.Opacity = 0.30;
                        iconStar.IsEnabled = false;
                        iconStar.Opacity = 0.30;
                        iconSearch.IsEnabled = false;
                        iconSearch.Opacity = 0.30;
                        iconSettings.IsEnabled = false;
                        iconSettings.Opacity = 0.30;
                        iconRefresh.IsEnabled = false;
                        iconRefresh.Opacity = 0.30;
                        iconReadAll.IsEnabled = false;
                        iconReadAll.Opacity = 0.30;
                    }
                }
                catch { }
                AppVariables.BusyApplication = true;
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

                    //Enable feed selection
                    combobox_FeedSelection.IsEnabled = true;
                    combobox_FeedSelection.Opacity = 1;

                    //Enable content
                    button_StatusCurrentItem.IsEnabled = true;
                    button_StatusCurrentItem.Opacity = 1;

                    //Enable buttons
                    iconApi.IsEnabled = true;
                    iconApi.Opacity = 1;
                    iconStar.IsEnabled = true;
                    iconStar.Opacity = 1;
                    iconSearch.IsEnabled = true;
                    iconSearch.Opacity = 1;
                    iconSettings.IsEnabled = true;
                    iconSettings.Opacity = 1;
                    iconRefresh.IsEnabled = true;
                    iconRefresh.Opacity = 1;
                    iconReadAll.IsEnabled = true;
                    iconReadAll.Opacity = 1;
                }
                catch { }
                AppVariables.BusyApplication = false;
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

        async void iconStar_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.NavigateToPage(new StarredPage(), true, false);
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

        private async void iconSettings_Tap(object sender, EventArgs e)
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

        async void iconRefresh_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                await RefreshItems(true);
            }
            catch { }
        }

        private async Task RefreshItems(bool Confirm)
        {
            try
            {
                string messageResult = string.Empty;
                if (Confirm)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Refresh news items");
                    messageAnswers.Add("Cancel");

                    messageResult = await MessagePopup.Popup("Refresh news items", "Do you want to refresh all the news items and scroll to the top?", messageAnswers);
                }

                if (messageResult == "Refresh news items")
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateNews = true;
                    ApiMessageError = string.Empty;

                    //Load all the items
                    await LoadItems(true, false);
                }
                else if (!List_NewsItems.Any() && !(bool)AppSettingLoad("DisplayReadMarkedItems"))
                {
                    Feeds TempFeed = new Feeds();
                    TempFeed.feed_id = "1";

                    //Change the selection feed
                    ChangeSelectionFeed(TempFeed, false);

                    //Load all the items
                    await LoadItems(false, true);
                }
            }
            catch { }
        }

        async void iconReadAll_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                bool MarkedAllRead = await MarkReadAll(List_NewsItems, true);

                //Ask if user wants to refresh the items
                if (MarkedAllRead) { await RefreshItems(true); }
            }
            catch { }
        }
    }
}