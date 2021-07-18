using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        //Search Variables
        public static Feeds vSearchFeed = null;
        public static string vSearchTerm = string.Empty;
        public static string vSearchFeedTitle = string.Empty;

        //Page Variables
        private static int vPreviousScrollItem = 0;

        //Application Navigation
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Loaded += async delegate
            {
                try
                {
                    //Register page events
                    RegisterPageEvents();

                    //Adjust the scrolling direction
                    await AdjustItemsScrollingDirection(Convert.ToInt32(AppVariables.ApplicationSettings["ItemScrollDirection"]));

                    //Adjust the list view style
                    ChangeListViewStyle(Convert.ToInt32(AppVariables.ApplicationSettings["ListViewStyle"]));

                    //Adjust the swiping direction
                    SwipeBarAdjust();

                    //Bind list to ListView
                    ListView_Items.ItemsSource = List_SearchItems;
                    combobox_FeedSelection.ItemsSource = List_FeedSelect;

                    //Load the search history
                    await LoadSearchHistory();

                    //Load feeds into selector
                    await LoadSelectionFeeds(false, true);

                    //Focus on the text box to open keyboard
                    txtbox_Search.IsEnabled = false;
                    txtbox_Search.IsEnabled = true;
                    txtbox_Search.Focus(FocusState.Programmatic);
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
                System.Diagnostics.Debug.WriteLine("Clearing page resources...");

                DisablePageEvents();
                vSearchFeed = null;

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

        //Search items from database
        async Task LoadItems()
        {
            try
            {
                //Clear all items and reset load count
                await ClearObservableCollection(List_SearchItems);

                //Get the currently selected feed
                vSearchFeed = combobox_FeedSelection.SelectedItem as Feeds;
                if (vSearchFeed.feed_title != null) { vSearchFeedTitle = vSearchFeed.feed_title; }
                if (vSearchFeed.feed_folder_title != null) { vSearchFeedTitle = vSearchFeed.feed_folder_title; }

                //Update the items count placer
                txt_AppInfo.Text = "Searching items";
                txt_NewsScrollInfo.Inlines.Clear();
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = "Your search results for " });
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = vSearchTerm, Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]) });
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = " in " });
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = vSearchFeedTitle, Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]) });
                txt_NewsScrollInfo.Inlines.Add(new Run() { Text = " will be shown here shortly..." });
                txt_NewsScrollInfo.Visibility = Visibility.Visible;

                await EventProgressDisableUI("Searching for: " + vSearchTerm, true);
                System.Diagnostics.Debug.WriteLine("Searching for: " + vSearchTerm);

                //Add search history to database
                await AddSearchHistory(vSearchTerm);

                //Load items from api/database
                AppVariables.LoadNews = false;
                AppVariables.LoadStarred = false;
                AppVariables.LoadSearch = true;
                AppVariables.LoadFeeds = false;
                int Result = await ApiUpdate.PageApiUpdate();

                //Check the api update result
                if (Result == 2)
                {
                    await CleanupPageResources();
                    App.vApplicationFrame.Navigate(typeof(SettingsPage));
                    //FixApp.vApplicationFrame.BackStack.Clear();
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
                            txt_AppInfo.Text = AppVariables.CurrentTotalItemsCount + " results";
                            txt_NewsScrollInfo.Visibility = Visibility.Collapsed;

                            button_StatusCurrentItem.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            txt_AppInfo.Text = "No results";
                            txt_NewsScrollInfo.Inlines.Clear();
                            txt_NewsScrollInfo.Inlines.Add(new Run() { Text = "No search results could be found for " });
                            txt_NewsScrollInfo.Inlines.Add(new Run() { Text = vSearchTerm, Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]) });
                            txt_NewsScrollInfo.Inlines.Add(new Run() { Text = " in " });
                            txt_NewsScrollInfo.Inlines.Add(new Run() { Text = vSearchFeedTitle, Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]) });
                            txt_NewsScrollInfo.Visibility = Visibility.Visible;

                            button_StatusCurrentItem.Visibility = Visibility.Collapsed;

                            //Focus on the text box to open keyboard
                            txtbox_Search.IsEnabled = false;
                            txtbox_Search.IsEnabled = true;
                            txtbox_Search.Focus(FocusState.Programmatic);
                        }

                        //Update the current item count
                        int HeaderTargetSize = Convert.ToInt32(stackpanel_Header.Tag);
                        int HeaderCurrentSize = Convert.ToInt32(stackpanel_Header.Height);
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
                int CurrentOffSetId = (virtualizingStackPanel.Orientation == Orientation.Horizontal) ? (int)virtualizingStackPanel.HorizontalOffset : (int)virtualizingStackPanel.VerticalOffset;

                //Update the current item count
                textblock_StatusCurrentItem.Tag = (CurrentOffSetId + 1).ToString();
                int HeaderTargetSize = Convert.ToInt32(stackpanel_Header.Tag);
                int HeaderCurrentSize = Convert.ToInt32(stackpanel_Header.Height);
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
                        //Disable Search
                        txtbox_Search.IsHitTestVisible = false;
                        txtbox_Search.Opacity = 0.30;

                        //Disable content
                        button_StatusCurrentItem.IsHitTestVisible = false;
                        button_StatusCurrentItem.Opacity = 0.30;

                        //Disable UI Buttons
                        iconApi.IsHitTestVisible = false;
                        iconApi.Opacity = 0.30;
                        iconNews.IsHitTestVisible = false;
                        iconNews.Opacity = 0.30;
                        iconStar.IsHitTestVisible = false;
                        iconStar.Opacity = 0.30;
                        iconSettings.IsHitTestVisible = false;
                        iconSettings.Opacity = 0.30;
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

                    //Enable Search
                    txtbox_Search.IsHitTestVisible = true;
                    txtbox_Search.Opacity = 1;

                    //Enable content
                    button_StatusCurrentItem.IsHitTestVisible = true;
                    button_StatusCurrentItem.Opacity = 1;

                    //Enable UI Buttons
                    iconApi.IsHitTestVisible = true;
                    iconApi.Opacity = 1;
                    iconNews.IsHitTestVisible = true;
                    iconNews.Opacity = 1;
                    iconStar.IsHitTestVisible = true;
                    iconStar.Opacity = 1;
                    iconSettings.IsHitTestVisible = true;
                    iconSettings.Opacity = 1;

                    AppVariables.BusyApplication = false;
                }
                catch { AppVariables.BusyApplication = false; }
            });
        }

        //User Interface - Buttons
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

        async void iconNews_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.vApplicationFrame.Navigate(typeof(NewsPage));
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
                //FixApp.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        async void txtbox_Search_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            try
            {
                //Set search or selection string
                if (e.ChosenSuggestion != null) { vSearchTerm = e.ChosenSuggestion.ToString(); } else { vSearchTerm = txtbox_Search.Text; }
                vSearchTerm = AVFunctions.StringRemoveEnd(vSearchTerm, " ");
                txtbox_Search.Text = vSearchTerm;

                //Check searchbox input
                if (string.IsNullOrWhiteSpace(vSearchTerm))
                {
                    if (!List_SearchItems.Any())
                    {
                        txt_NewsScrollInfo.Text = "Please select a feed to search in and enter a search term to look for above...";
                        txt_NewsScrollInfo.Visibility = Visibility.Visible;
                    }

                    //Focus on the text box to open keyboard
                    txtbox_Search.IsEnabled = false;
                    txtbox_Search.IsEnabled = true;
                    txtbox_Search.Focus(FocusState.Programmatic);
                    return;
                }

                //Initialize the search
                txtbox_Search.IsEnabled = false;

                //Hide the suggestion list
                txtbox_Search.IsSuggestionListOpen = false;

                //Hide the keyboard
                InputPane.GetForCurrentView().TryHide();

                txtbox_Search.IsEnabled = true;

                //Search for the items
                await LoadItems();
            }
            catch { }
        }

        //Monitor the application size
        private double PreviousLayoutWidth = 0;
        private double PreviousLayoutHeight = 0;
        private async void OnLayoutUpdated(object sender, object e)
        {
            try
            {
                Rect ScreenResolution = AVFunctions.AppWindowResolution();
                double NewLayoutWidth = ScreenResolution.Width;
                double NewLayoutHeight = ScreenResolution.Height;
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

        //Show suggestion list when focused
        private void txtbox_Search_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                AutoSuggestBox autosender = sender as AutoSuggestBox;
                autosender.IsSuggestionListOpen = true;
            }
            catch { }
        }
    }
}