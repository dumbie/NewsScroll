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
using static NewsScroll.Database.Database;
using static NewsScroll.Lists.Lists;
using static NewsScroll.Process;

namespace NewsScroll
{
    public partial class NewsPage : ContentPage
    {
        //Page Variables
        //0 = All items
        //1 = All read items
        //2 = All unread items
        public static Feeds vCurrentLoadingFeedFolder = null;
        private static int vPreviousScrollItem = 0;

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
                    App.NavigateToPage(new SettingsPage(), true);
                    return;
                }

                //Register page events
                RegisterPageEvents();

                //Show page status
                ProgressDisableUI("Preparing news page...", true);

                //Bind list to ListView
                ListView_Items.ItemsSource = List_NewsItems;
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
                //EventAdjustItemsScrollingDirection += new DelegateAdjustItemsScrollingDirection(AdjustItemsScrollingDirection);
                //EventChangeListViewStyle += new DelegateChangeListViewStyle(ChangeListViewStyle);
                EventRefreshPageItems += new DelegateRefreshPageItems(RefreshItems);

                //Register ListView events
                ListView_Items.ItemTapped += EventsListView.ListView_Items_Tapped;

                //Register ListView scroll viewer
                ListView_Items.ItemAppearing += ScrollViewer_ItemAppearing;
                ListView_Items.ItemDisappearing += ScrollViewer_ItemDisappearing;

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
                //EventAdjustItemsScrollingDirection -= new DelegateAdjustItemsScrollingDirection(AdjustItemsScrollingDirection);
                //EventChangeListViewStyle -= new DelegateChangeListViewStyle(ChangeListViewStyle);
                EventRefreshPageItems -= new DelegateRefreshPageItems(RefreshItems);

                //Register ListView events
                ListView_Items.ItemTapped -= EventsListView.ListView_Items_Tapped;

                //Register ListView scroll viewer
                ListView_Items.ItemAppearing -= ScrollViewer_ItemAppearing;
                ListView_Items.ItemDisappearing -= ScrollViewer_ItemDisappearing;

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
                vCurrentLoadingFeedFolder = null;

                ListView_Items.ItemTemplate = null;
                ListView_Items.ItemsSource = null;
                ListView_Items.ItemTemplate = null;
                ListView_Items.ItemsSource = null;

                //combobox_FeedSelection.ItemTemplate = null;
                combobox_FeedSelection.ItemsSource = null;
                //combobox_FeedSelection.ItemTemplate = null;
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
                    vCurrentLoadingFeedFolder = SelectedItem;

                    //Reset the previous scroll item
                    vPreviousScrollItem = 0;

                    //Load all the items
                    await LoadItems(false, false);

                    ProgressEnableUI();
                }
            }
            catch { }
        }

        //Update Api Start
        async Task LoadItems(bool LoadSelectFeeds, bool UpdateSelectFeeds)
        {
            try
            {
                //Clear all items and reset load count
                await ClearObservableCollection(List_NewsItems);

                //Get the currently selected feed
                string SelectedFeedTitle = "All feed items";
                if (!(bool)AppSettingLoad("DisplayReadMarkedItems")) { SelectedFeedTitle = "All unread items"; }
                if (vCurrentLoadingFeedFolder != null)
                {
                    if (vCurrentLoadingFeedFolder.feed_title != null) { SelectedFeedTitle = vCurrentLoadingFeedFolder.feed_title; }
                    if (vCurrentLoadingFeedFolder.feed_folder_title != null) { SelectedFeedTitle = vCurrentLoadingFeedFolder.feed_folder_title; }
                }

                //Update the loading information
                txt_AppInfo.Text = "Loading items";
                Application.Current.Resources.TryGetValue("ApplicationAccentLightColor", out object ColorAccent);
                FormattedString fs = new FormattedString();
                fs.Spans.Add(new Span { Text = "Your news items from " });
                fs.Spans.Add(new Span { Text = SelectedFeedTitle, TextColor = (Color)ColorAccent });
                fs.Spans.Add(new Span { Text = " will be shown here shortly..." });
                txt_NewsScrollInfo.FormattedText = fs;
                txt_NewsScrollInfo.IsVisible = true;

                //Check the loading feed
                if (LoadSelectFeeds)
                {
                    if ((bool)AppSettingLoad("DisplayReadMarkedItems"))
                    {
                        Feeds TempFeed = new Feeds();
                        TempFeed.feed_id = "0";
                        vCurrentLoadingFeedFolder = TempFeed;
                        vPreviousScrollItem = 0;
                    }
                    else
                    {
                        Feeds TempFeed = new Feeds();
                        TempFeed.feed_id = "2";
                        vCurrentLoadingFeedFolder = TempFeed;
                        vPreviousScrollItem = 0;
                    }
                }

                //Load items from api/database
                AppVariables.LoadNews = true;
                AppVariables.LoadStarred = false;
                AppVariables.LoadSearch = false;
                AppVariables.LoadFeeds = true;
                int Result = await ApiUpdate.PageApiUpdate();

                //Check the api update result
                if (Result == 2)
                {
                    await CleanupPageResources();
                    App.NavigateToPage(new SettingsPage(), true);
                    return;
                }

                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                //Set all items to list
                List<TableFeeds> LoadTableFeeds = await vSQLConnection.Table<TableFeeds>().OrderBy(x => x.feed_folder).ToListAsync();
                List<TableItems> LoadTableItems = await vSQLConnection.Table<TableItems>().ToListAsync();

                //Load items into the list
                await ProcessItemLoad.DatabaseToList(LoadTableFeeds, LoadTableItems, AppVariables.CurrentItemsLoaded, AppVariables.ItemsToScrollLoad, false, false);

                //Load feeds into selector
                if (LoadSelectFeeds)
                {
                    await LoadSelectionFeeds(LoadTableFeeds, LoadTableItems, false, true);
                }

                //Update feeds in selector
                if (UpdateSelectFeeds)
                {
                    await UpdateSelectionFeeds(LoadTableFeeds, LoadTableItems, false, true);
                }

                //Change the selection feed
                ChangeSelectionFeed(vCurrentLoadingFeedFolder, false);

                //Update the total item count
                UpdateTotalItemsCount();

                //Enable the interface manually
                if (!LoadSelectFeeds && !UpdateSelectFeeds) { ProgressEnableUI(); }
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

        //Listview item appearing
        private void ScrollViewer_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            try
            {
                //Update the current item count
                int CurrentOffSetId = e.ItemIndex;
                AppVariables.CurrentViewItemsCount = CurrentOffSetId;

                if (stackpanel_Header.IsVisible || AppVariables.CurrentTotalItemsCount == 0)
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount.ToString();
                }
                else
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount + "/" + AppVariables.CurrentTotalItemsCount;
                }

                //Update the shown item content
                ItemUpdateImages(e.Item, false);
            }
            catch { }
        }

        //Listview item disappearing
        private void ScrollViewer_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
            try
            {
                //Update the current item count
                int CurrentOffSetId = e.ItemIndex;
                AppVariables.CurrentViewItemsCount = CurrentOffSetId;

                if (stackpanel_Header.IsVisible || AppVariables.CurrentTotalItemsCount == 0)
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount.ToString();
                }
                else
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount + "/" + AppVariables.CurrentTotalItemsCount;
                }

                //Update the shown item content
                ItemUpdateImages(e.Item, true);
            }
            catch { }
        }

        //Item status scroll events
        private async void button_StatusCurrentItem_Tapped(object sender, EventArgs e)
        {
            try
            {
                bool Scrolled = await EventsItemStatus.ListViewScroller(ListView_Items, AppVariables.CurrentViewItemsCount, vPreviousScrollItem);
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
                //App.vApplicationFrame.Navigate(typeof(SearchPage));
                //App.vApplicationFrame.BackStack.Clear();
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
                //App.vApplicationFrame.Navigate(typeof(ApiPage));
                //App.vApplicationFrame.BackStack.Clear();
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
                //App.vApplicationFrame.Navigate(typeof(StarredPage));
                //App.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        private async void iconPersonalize_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                //PersonalizePopup personalizePopup = new PersonalizePopup();
                //await personalizePopup.OpenPopup();
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
                App.NavigateToPage(new SettingsPage(), true);
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

                    messageResult = await AVMessageBox.Popup("Refresh news items", "Do you want to refresh all the news items and scroll to the top?", messageAnswers);
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