using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static NewsScroll.Api.Api;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class NewsPage : Page
    {
        public NewsPage()
        {
            this.InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        //Page Variables
        //0 = All items
        //1 = Read items
        //2 = Unread items
        public static Feeds vCurrentLoadingFeedFolder = null;
        private static int vPreviousScrollItem = 0;

        //Application Navigation
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check the set account
                if (!CheckAccount())
                {
                    await CleanupPageResources();
                    return;
                }

                //Register page events
                RegisterPageEvents();

                //Show page status
                await ProgressDisableUI("Preparing news page...", true);

                //Adjust the scrolling direction
                await AdjustItemsScrollingDirection(Convert.ToInt32(AppVariables.ApplicationSettings["ItemScrollDirection"]));

                //Adjust the list view style
                ChangeListViewStyle(Convert.ToInt32(AppVariables.ApplicationSettings["ListViewStyle"]));

                //Adjust the swiping direction
                SwipeBarAdjust();

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
                EventAdjustItemsScrollingDirection += new DelegateAdjustItemsScrollingDirection(AdjustItemsScrollingDirection);
                EventChangeListViewStyle += new DelegateChangeListViewStyle(ChangeListViewStyle);
                EventRefreshPageItems += new DelegateRefreshPageItems(RefreshItems);

                //Register ListView events
                ListView_Items.Tapped += EventsListView.ListView_Items_Tapped;
                ListView_Items.RightTapped += EventsListView.ListView_Items_RightTapped;

                //Register ListView scroll viewer
                ScrollViewer ListViewScrollViewer = AVFunctions.FindVisualChild<ScrollViewer>(ListView_Items);
                ListViewScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                ListViewScrollViewer.VerticalSnapPointsType = SnapPointsType.None;
                ListViewScrollViewer.HorizontalSnapPointsType = SnapPointsType.None;

                //Register combo box events
                combobox_FeedSelection.SelectionChanged += combobox_FeedSelection_SelectionChanged;

                //Monitor user touch swipe
                if (!(bool)AppVariables.ApplicationSettings["DisableSwipeActions"])
                {
                    grid_SwipeBar.ManipulationMode = ManipulationModes.TranslateX;
                    grid_SwipeBar.ManipulationStarted += Page_ManipulationStarted;
                    grid_SwipeBar.ManipulationDelta += Page_ManipulationDelta;
                    grid_SwipeBar.ManipulationCompleted += Page_ManipulationCompleted;
                }

                //Monitor key presses
                grid_Main.PreviewKeyUp += Page_PreviewKeyUp; //DesktopOnly
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
                EventAdjustItemsScrollingDirection -= new DelegateAdjustItemsScrollingDirection(AdjustItemsScrollingDirection);
                EventChangeListViewStyle -= new DelegateChangeListViewStyle(ChangeListViewStyle);
                EventRefreshPageItems -= new DelegateRefreshPageItems(RefreshItems);

                //Register ListView events
                ListView_Items.Tapped -= EventsListView.ListView_Items_Tapped;
                ListView_Items.RightTapped -= EventsListView.ListView_Items_RightTapped;

                //Register ListView scroll viewer
                ScrollViewer ListViewScrollViewer = AVFunctions.FindVisualChild<ScrollViewer>(ListView_Items);
                ListViewScrollViewer.ViewChanged -= ScrollViewer_ViewChanged;

                //Register combo box events
                combobox_FeedSelection.SelectionChanged -= combobox_FeedSelection_SelectionChanged;

                //Monitor user touch swipe
                grid_Main.ManipulationStarted -= Page_ManipulationStarted;
                grid_Main.ManipulationDelta -= Page_ManipulationDelta;
                grid_Main.ManipulationCompleted -= Page_ManipulationCompleted;

                //Monitor key presses
                grid_Main.PreviewKeyUp -= Page_PreviewKeyUp; //DesktopOnly
            }
            catch { }
        }

        //Cleanup page resources
        async Task CleanupPageResources()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Clearing page resources...");

                DisablePageEvents();
                vCurrentLoadingFeedFolder = null;

                ListView_Items.ItemContainerStyle = null;
                ListView_Items.ItemsSource = null;
                ListView_Items.ItemsPanel = null;
                ListView_Items.Items.Clear();

                combobox_FeedSelection.ItemContainerStyle = null;
                combobox_FeedSelection.ItemsSource = null;
                combobox_FeedSelection.ItemsPanel = null;
                combobox_FeedSelection.Items.Clear();

                await ClearObservableCollection(List_Feeds);
                await ClearObservableCollection(List_FeedSelect);
                await ClearObservableCollection(List_NewsItems);
                await ClearObservableCollection(List_SearchItems);
                await ClearObservableCollection(List_StarredItems);
            }
            catch { }
        }

        private async void combobox_FeedSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Load selected feed items
                ComboBox ComboBoxSender = sender as ComboBox;
                Feeds SelectedItem = ComboBoxSender.SelectedItem as Feeds;
                if (SelectedItem != null)
                {
                    string CheckKind = "feed";
                    if (SelectedItem.feed_folder_status) { CheckKind = "folder"; }
                    else if (SelectedItem.feed_collection_status) { CheckKind = "collection"; }

                    await ProgressDisableUI("Checking " + CheckKind + " items...", true);
                    System.Diagnostics.Debug.WriteLine("Checking " + CheckKind + " items...");

                    //Set the loading feed or folder
                    vCurrentLoadingFeedFolder = SelectedItem;

                    //Reset the previous scroll item
                    vPreviousScrollItem = 0;

                    //Load all the items
                    await LoadItems(false, false);

                    await ProgressEnableUI();
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
                string SelectedFeedTitle = "All items";
                if (!(bool)AppVariables.ApplicationSettings["DisplayReadMarkedItems"]) { SelectedFeedTitle = "Unread items"; }
                if (vCurrentLoadingFeedFolder != null)
                {
                    if (vCurrentLoadingFeedFolder.feed_title != null) { SelectedFeedTitle = vCurrentLoadingFeedFolder.feed_title; }
                    if (vCurrentLoadingFeedFolder.feed_folder_title != null) { SelectedFeedTitle = vCurrentLoadingFeedFolder.feed_folder_title; }
                }

                //Update the loading information
                txt_AppInfo.Text = "Loading items";
                txt_NewsScrollInfo.Inlines.Clear();
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = "Your news items from " });
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = SelectedFeedTitle, Foreground = new SolidColorBrush((Color)Application.Current.Resources["ApplicationAccentLightColor"]) });
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = " will be shown here shortly..." });
                txt_NewsScrollInfo.Visibility = Visibility.Visible;

                //Check the loading feed
                if (LoadSelectFeeds)
                {
                    if ((bool)AppVariables.ApplicationSettings["DisplayReadMarkedItems"])
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
                    App.vApplicationFrame.Navigate(typeof(SettingsPage));
                    //FixApp.vApplicationFrame.BackStack.Clear();
                    return;
                }

                //Set all items to list
                List<TableFeeds> LoadTableFeeds = await SQLConnection.Table<TableFeeds>().OrderBy(x => x.feed_folder).ToListAsync();
                List<TableItems> LoadTableItems = await SQLConnection.Table<TableItems>().ToListAsync();

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
                if (!LoadSelectFeeds && !UpdateSelectFeeds) { await ProgressEnableUI(); }
            }
            catch { }
        }

        //Hide the progression status
        private async Task HideProgressionStatus()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    txt_NewsScrollInfo.Visibility = Visibility.Collapsed;
                }
                catch { }
            });
        }

        //Item status scroll events
        private async void button_StatusCurrentItem_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                bool Scrolled = await EventsItemStatus.ListViewScroller(ListView_Items, Convert.ToInt32(AppVariables.CurrentShownItemCount), vPreviousScrollItem);
                if (Scrolled)
                {
                    vPreviousScrollItem = Convert.ToInt32(AppVariables.CurrentShownItemCount);
                }
            }
            catch { }
        }

        //Progressbar/UI Status
        async Task ProgressDisableUI(string ProgressMsg, bool DisableInterface)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    AppVariables.BusyApplication = true;

                    //Enable progressbar
                    textblock_StatusApplication.Text = ProgressMsg;
                    grid_StatusApplication.Visibility = Visibility.Visible;

                    if (DisableInterface)
                    {
                        //Disable feed selection
                        combobox_FeedSelection.IsHitTestVisible = false;
                        combobox_FeedSelection.Opacity = 0.30;

                        //Disable content
                        button_StatusCurrentItem.IsHitTestVisible = false;
                        button_StatusCurrentItem.Opacity = 0.30;

                        //Disable buttons
                        iconApi.IsHitTestVisible = false;
                        iconApi.Opacity = 0.30;
                        iconStar.IsHitTestVisible = false;
                        iconStar.Opacity = 0.30;
                        iconSearch.IsHitTestVisible = false;
                        iconSearch.Opacity = 0.30;
                        iconSettings.IsHitTestVisible = false;
                        iconSettings.Opacity = 0.30;
                        iconRefresh.IsHitTestVisible = false;
                        iconRefresh.Opacity = 0.30;
                        iconReadAll.IsHitTestVisible = false;
                        iconReadAll.Opacity = 0.30;
                    }
                }
                catch { AppVariables.BusyApplication = true; }
            });
        }

        async Task ProgressEnableUI()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    //Disable progressbar
                    grid_StatusApplication.Visibility = Visibility.Collapsed;
                    textblock_StatusApplication.Text = string.Empty;

                    //Enable feed selection
                    combobox_FeedSelection.IsHitTestVisible = true;
                    combobox_FeedSelection.Opacity = 1;

                    //Enable content
                    button_StatusCurrentItem.IsHitTestVisible = true;
                    button_StatusCurrentItem.Opacity = 1;

                    //Enable buttons
                    iconApi.IsHitTestVisible = true;
                    iconApi.Opacity = 1;
                    iconStar.IsHitTestVisible = true;
                    iconStar.Opacity = 1;
                    iconSearch.IsHitTestVisible = true;
                    iconSearch.Opacity = 1;
                    iconSettings.IsHitTestVisible = true;
                    iconSettings.Opacity = 1;
                    iconRefresh.IsHitTestVisible = true;
                    iconRefresh.Opacity = 1;
                    iconReadAll.IsHitTestVisible = true;
                    iconReadAll.Opacity = 1;

                    AppVariables.BusyApplication = false;
                }
                catch { AppVariables.BusyApplication = false; }
            });
        }

        //User Interface - Buttons
        async void iconSearch_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.vApplicationFrame.Navigate(typeof(SearchPage));
                //FixApp.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        async void iconApi_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.vApplicationFrame.Navigate(typeof(ApiPage));
                //FixApp.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        async void iconStar_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.vApplicationFrame.Navigate(typeof(StarredPage));
                //FixApp.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        private async void iconPersonalize_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);
                PersonalizePopup personalizePopup = new PersonalizePopup();
                await personalizePopup.Popup();
            }
            catch { }
        }

        async void iconSettings_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.vApplicationFrame.Navigate(typeof(SettingsPage));
                //FixApp.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        async void iconRefresh_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);
                await RefreshItems(true);
            }
            catch { }
        }

        private async Task RefreshItems(bool Confirm)
        {
            try
            {
                int MsgBoxResult = 1;
                if (Confirm) { MsgBoxResult = await new MessagePopup().OpenPopup("Refresh news items", "Do you want to refresh all the news items and scroll to the top?", "Refresh news items", "", "", "", "", true); }
                if (MsgBoxResult == 1)
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateNews = true;
                    ApiMessageError = string.Empty;

                    //Load all the items
                    await LoadItems(true, false);
                }
                else if (!List_NewsItems.Any() && !(bool)AppVariables.ApplicationSettings["DisplayReadMarkedItems"])
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

        async void iconReadAll_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);
                bool MarkedAllRead = await MarkReadAll(List_NewsItems, true);

                //Ask if user wants to refresh the items
                if (MarkedAllRead) { await RefreshItems(true); }
            }
            catch { }
        }
    }
}