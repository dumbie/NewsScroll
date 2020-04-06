using ArnoldVinkCode;
using ArnoldVinkMessageBox;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
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
        }

        //Page Variables
        //0 = All items
        //1 = All read items
        //2 = All unread items
        public static Feeds vCurrentLoadingFeedFolder = null;
        private static Int32 vPreviousScrollItem = 0;

        //Application Navigation
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Loaded += async delegate
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

                    ////Enable combobox searching
                    //if (AVFunctions.DevOsVersion() >= 14393)
                    //{
                    //    combobox_FeedSelection.IsTextSearchEnabled = true;
                    //    Debug.WriteLine("Enabling combobox text searching.");
                    //}

                    //Load all the items
                    await LoadItems(true, false);
                }
                catch { }
            };
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
                Debug.WriteLine("Clearing page resources...");

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
                    Debug.WriteLine("Checking " + CheckKind + " items...");

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
                string SelectedFeedTitle = "All feed items";
                if (!(bool)AppVariables.ApplicationSettings["DisplayReadMarkedItems"]) { SelectedFeedTitle = "All unread items"; }
                if (vCurrentLoadingFeedFolder != null)
                {
                    if (vCurrentLoadingFeedFolder.feed_title != null) { SelectedFeedTitle = vCurrentLoadingFeedFolder.feed_title; }
                    if (vCurrentLoadingFeedFolder.feed_folder_title != null) { SelectedFeedTitle = vCurrentLoadingFeedFolder.feed_folder_title; }
                }

                //Update the loading information
                txt_AppInfo.Text = "Loading items";
                txt_NewsScrollInfo.Inlines.Clear();
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = "Your news items from " });
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = SelectedFeedTitle, Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]) });
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
                Int32 Result = await ApiUpdate.PageApiUpdate();

                //Check the api update result
                if (Result == 2)
                {
                    await CleanupPageResources();
                    App.vApplicationFrame.Navigate(typeof(SettingsPage));
                    App.vApplicationFrame.BackStack.Clear();
                    return;
                }

                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

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

        //Monitor and handle the scroll viewer
        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                //Get current scroll item
                VirtualizingStackPanel virtualizingStackPanel = AVFunctions.FindVisualChild<VirtualizingStackPanel>(ListView_Items);
                Int32 CurrentOffSetId = (virtualizingStackPanel.Orientation == Orientation.Horizontal) ? (Int32)virtualizingStackPanel.HorizontalOffset : (Int32)virtualizingStackPanel.VerticalOffset;

                //Update the current item count
                textblock_StatusCurrentItem.Tag = (CurrentOffSetId + 1).ToString();
                Int32 HeaderTargetSize = Convert.ToInt32(stackpanel_Header.Tag);
                Int32 HeaderCurrentSize = Convert.ToInt32(stackpanel_Header.Height);
                if (HeaderCurrentSize == HeaderTargetSize || AppVariables.CurrentTotalItemsCount == 0)
                {
                    textblock_StatusCurrentItem.Text = textblock_StatusCurrentItem.Tag.ToString();
                }
                else
                {
                    textblock_StatusCurrentItem.Text = textblock_StatusCurrentItem.Tag.ToString() + "/" + AppVariables.CurrentTotalItemsCount;
                }

                //Update the shown item content
                await EventsScrollViewer.ScrollViewerUpdateContent(ListView_Items, CurrentOffSetId);

                //Check if new items need to be loaded
                await EventsScrollViewer.ScrollViewerAddItems(ListView_Items, CurrentOffSetId);
            }
            catch { }
        }

        //Item status scroll events
        private async void button_StatusCurrentItem_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                bool Scrolled = await EventsItemStatus.ListViewScroller(ListView_Items, Convert.ToInt32(textblock_StatusCurrentItem.Tag), vPreviousScrollItem);
                if (Scrolled) { vPreviousScrollItem = Convert.ToInt32(textblock_StatusCurrentItem.Tag); }
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
                    textblock_StatusApplication.Text = String.Empty;

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
                App.vApplicationFrame.BackStack.Clear();
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
                App.vApplicationFrame.BackStack.Clear();
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
                App.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        private async void iconPersonalize_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);
                PersonalizePopup personalizePopup = new PersonalizePopup();
                await personalizePopup.OpenPopup();
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
                App.vApplicationFrame.BackStack.Clear();
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
                Int32 MsgBoxResult = 1;
                if (Confirm) { MsgBoxResult = await AVMessageBox.Popup("Refresh news items", "Do you want to refresh all the news items and scroll to the top?", "Refresh news items", "", "", "", "", true); }
                if (MsgBoxResult == 1)
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateNews = true;
                    ApiMessageError = String.Empty;

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

        //Monitor the application size
        private Double PreviousLayoutWidth = 0;
        private Double PreviousLayoutHeight = 0;
        private async void OnLayoutUpdated(object sender, object e)
        {
            try
            {
                Rect ScreenResolution = AVFunctions.AppWindowResolution();
                Double NewLayoutWidth = ScreenResolution.Width;
                Double NewLayoutHeight = ScreenResolution.Height;
                if (NewLayoutWidth != PreviousLayoutWidth || NewLayoutHeight != PreviousLayoutHeight)
                {
                    PreviousLayoutWidth = NewLayoutWidth;
                    PreviousLayoutHeight = NewLayoutHeight;

                    //Adjust the scrolling direction
                    await AdjustItemsScrollingDirection(Convert.ToInt32(AppVariables.ApplicationSettings["ItemScrollDirection"]));
                }
            }
            catch { }
        }
    }
}