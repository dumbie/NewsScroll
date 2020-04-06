﻿using ArnoldVinkCode;
using ArnoldVinkMessageBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using static NewsScroll.Api.Api;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class StarredPage : Page
    {
        public StarredPage()
        {
            this.InitializeComponent();
        }

        //Page Variables
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
                    await ProgressDisableUI("Preparing starred page...", true);

                    //Adjust the scrolling direction
                    await AdjustItemsScrollingDirection(Convert.ToInt32(AppVariables.ApplicationSettings["ItemScrollDirection"]));

                    //Adjust the list view style
                    ChangeListViewStyle(Convert.ToInt32(AppVariables.ApplicationSettings["ListViewStyle"]));

                    //Adjust the swiping direction
                    SwipeBarAdjust();

                    //Bind list to ListView
                    ListView_Items.ItemsSource = List_StarredItems;

                    //Load all the items
                    await LoadItems();
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
                EventUpdateTotalItemsCount += new DelegateUpdateTotalItemsCount(UpdateTotalItemsCount);
                EventAdjustItemsScrollingDirection += new DelegateAdjustItemsScrollingDirection(AdjustItemsScrollingDirection);
                EventChangeListViewStyle += new DelegateChangeListViewStyle(ChangeListViewStyle);

                //Register ListView events
                ListView_Items.Tapped += EventsListView.ListView_Items_Tapped;
                ListView_Items.RightTapped += EventsListView.ListView_Items_RightTapped;

                //Register ListView scroll viewer
                ScrollViewer ListViewScrollViewer = AVFunctions.FindVisualChild<ScrollViewer>(ListView_Items);
                ListViewScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                ListViewScrollViewer.VerticalSnapPointsType = SnapPointsType.None;
                ListViewScrollViewer.HorizontalSnapPointsType = SnapPointsType.None;

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
                EventUpdateTotalItemsCount -= new DelegateUpdateTotalItemsCount(UpdateTotalItemsCount);
                EventAdjustItemsScrollingDirection -= new DelegateAdjustItemsScrollingDirection(AdjustItemsScrollingDirection);
                EventChangeListViewStyle -= new DelegateChangeListViewStyle(ChangeListViewStyle);

                //Register ListView events
                ListView_Items.Tapped -= EventsListView.ListView_Items_Tapped;
                ListView_Items.RightTapped -= EventsListView.ListView_Items_RightTapped;

                //Register ListView scroll viewer
                ScrollViewer ListViewScrollViewer = AVFunctions.FindVisualChild<ScrollViewer>(ListView_Items);
                ListViewScrollViewer.ViewChanged -= ScrollViewer_ViewChanged;

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

                ListView_Items.ItemContainerStyle = null;
                ListView_Items.ItemsSource = null;
                ListView_Items.ItemsPanel = null;
                ListView_Items.Items.Clear();

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
                txt_NewsScrollInfo.Visibility = Visibility.Visible;

                //Load items from api/database
                AppVariables.LoadNews = false;
                AppVariables.LoadStarred = true;
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
                List<TableItems> LoadTableItems = await SQLConnection.Table<TableItems>().ToListAsync();

                //Load items into the list
                await ProcessItemLoad.DatabaseToList(null, LoadTableItems, AppVariables.CurrentItemsLoaded, AppVariables.ItemsToScrollLoad, false, false);

                //Update the total items count
                await UpdateTotalItemsCount(null, LoadTableItems, false, true);
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

        //Update the total items count
        private async Task UpdateTotalItemsCount(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, bool Silent, bool EnableUI)
        {
            try
            {
                //Set the total item count
                AppVariables.CurrentTotalItemsCount = await ProcessItemLoad.DatabaseToCount(LoadTableFeeds, LoadTableItems, Silent, EnableUI);
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        //Check the total item count
                        if (AppVariables.CurrentTotalItemsCount > 0)
                        {
                            txt_AppInfo.Text = ApiMessageError + AppVariables.CurrentTotalItemsCount + " items";
                            txt_NewsScrollInfo.Visibility = Visibility.Collapsed;

                            button_StatusCurrentItem.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            txt_AppInfo.Text = ApiMessageError + "No items";
                            txt_NewsScrollInfo.Text = "It seems like you don't have any starred items added to your account, please add some starred items to start loading your items and click on the refresh button above.";
                            txt_NewsScrollInfo.Visibility = Visibility.Visible;

                            button_StatusCurrentItem.Visibility = Visibility.Collapsed;
                        }

                        //Update the current item count
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
                    }
                    catch { }
                });
            }
            catch { }
        }

        //Monitor and handle the scroll viewer
        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                //Get current scroll item
                VirtualizingStackPanel virtualizingStackPanel = AVFunctions.FindVisualChild<VirtualizingStackPanel>(ListView_Items);
                Int32 CurrentOffSetId = (virtualizingStackPanel.Orientation == Orientation.Horizontal) ? (Int32)virtualizingStackPanel.HorizontalOffset : (Int32)virtualizingStackPanel.VerticalOffset;

                //Update the current item status text
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
                        //Disable content
                        button_StatusCurrentItem.IsHitTestVisible = false;
                        button_StatusCurrentItem.Opacity = 0.30;

                        //Disable buttons
                        iconApi.IsHitTestVisible = false;
                        iconApi.Opacity = 0.30;
                        iconNews.IsHitTestVisible = false;
                        iconNews.Opacity = 0.30;
                        iconSearch.IsHitTestVisible = false;
                        iconSearch.Opacity = 0.30;
                        iconSettings.IsHitTestVisible = false;
                        iconSettings.Opacity = 0.30;
                        iconRefresh.IsHitTestVisible = false;
                        iconRefresh.Opacity = 0.30;
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

                    //Enable content
                    button_StatusCurrentItem.IsHitTestVisible = true;
                    button_StatusCurrentItem.Opacity = 1;

                    //Enable buttons
                    iconApi.IsHitTestVisible = true;
                    iconApi.Opacity = 1;
                    iconNews.IsHitTestVisible = true;
                    iconNews.Opacity = 1;
                    iconSearch.IsHitTestVisible = true;
                    iconSearch.Opacity = 1;
                    iconSettings.IsHitTestVisible = true;
                    iconSettings.Opacity = 1;
                    iconRefresh.IsHitTestVisible = true;
                    iconRefresh.Opacity = 1;

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

        async void iconNews_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.vApplicationFrame.Navigate(typeof(NewsPage));
                App.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        async void iconRefresh_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);
                await RefreshItems();
            }
            catch { }
        }

        private async Task RefreshItems()
        {
            try
            {
                Int32 MsgBoxResult = await AVMessageBox.Popup("Refresh starred items", "Do you want to refresh starred items and scroll to the top?", "Refresh starred items", "", "", "", true);
                if (MsgBoxResult == 1)
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateStarred = true;
                    ApiMessageError = String.Empty;

                    //Load all the items
                    await LoadItems();
                }
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