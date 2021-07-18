using ArnoldVinkCode;
using ArnoldVinkMessageBox;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static NewsScroll.Api.Api;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class ApiPage : Page
    {
        public ApiPage()
        {
            this.InitializeComponent();
        }

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

                    //Bind list to ListView
                    ListView_Items.ItemsSource = List_Feeds;

                    //Set the autosuggestbox inputscope
                    TextBox SuggestTextBox = AVFunctions.FindVisualChild<TextBox>(txtbox_AddFeed);
                    InputScope inputScope = new InputScope();
                    InputScopeName inputScopeName = new InputScopeName() { NameValue = InputScopeNameValue.Url };
                    inputScope.Names.Add(inputScopeName);
                    SuggestTextBox.InputScope = inputScope;

                    //Load all the feeds
                    await LoadFeeds();
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
                EventHideProgressionStatus += new DelegateHideProgressionStatus(HideProgressionStatus);
                EventUpdateTotalItemsCount += new DelegateUpdateTotalItemsCount(UpdateTotalItemsCount);

                //Register ListView events
                ListView_Items.RightTapped += UpdateFeedIcon;

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
                //Register load events
                EventProgressDisableUI -= new DelegateProgressDisableUI(ProgressDisableUI);
                EventProgressEnableUI -= new DelegateProgressEnableUI(ProgressEnableUI);
                EventHideProgressionStatus -= new DelegateHideProgressionStatus(HideProgressionStatus);
                EventUpdateTotalItemsCount -= new DelegateUpdateTotalItemsCount(UpdateTotalItemsCount);

                //Register ListView events
                ListView_Items.RightTapped -= UpdateFeedIcon;

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

        //User Interface - Buttons
        void HideShowMenu(bool ForceClose)
        {
            try
            {
                int MenuTargetSize = Convert.ToInt32(grid_PopupMenu.Tag);
                int MenuCurrentSize = Convert.ToInt32(grid_PopupMenu.Height);
                if (ForceClose || MenuCurrentSize == MenuTargetSize) { grid_PopupMenu.Height = 0; }
                else { grid_PopupMenu.Height = MenuTargetSize; }
            }
            catch { }
        }
        void iconMenu_Tap(object sender, RoutedEventArgs e) { try { HideShowMenu(false); } catch { } }

        async void iconNews_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.vApplicationFrame.Navigate(typeof(NewsPage));
                //FixApp.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        async void iconStar_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.vApplicationFrame.Navigate(typeof(StarredPage));
                //FixApp.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        async void iconSearch_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.vApplicationFrame.Navigate(typeof(SearchPage));
                //FixApp.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        async void iconSettings_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                HideShowMenu(true);

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
            try { await RefreshFeeds(); } catch { }
        }

        private async Task RefreshFeeds()
        {
            try
            {
                HideShowMenu(true);

                int MsgBoxResult = await AVMessageBox.Popup("Refresh feeds", "Do you want to refresh the feeds and scroll to the top?", "Refresh feeds", "", "", "", "", true);
                if (MsgBoxResult == 1)
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    ApiMessageError = string.Empty;

                    //Load all the feeds
                    await LoadFeeds();
                }
            }
            catch { }
        }

        //Update the feed icon
        private async void UpdateFeedIcon(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                ListView SendListView = sender as ListView;
                Feeds SelectedItem = ((e.OriginalSource as FrameworkElement).DataContext) as Feeds;
                if (SelectedItem != null)
                {
                    int MsgBoxResult = await AVMessageBox.Popup("Change the feed icon", "Would you like to set a custom feed icon for " + SelectedItem.feed_title + "?", "Set custom icon", "Reset the icon", "", "", "", true);
                    if (MsgBoxResult == 1)
                    {
                        System.Diagnostics.Debug.WriteLine("Changing icon for feed: " + SelectedItem.feed_id + " / " + SelectedItem.feed_title);

                        FileOpenPicker FileOpenPicker = new FileOpenPicker();
                        FileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
                        FileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                        FileOpenPicker.FileTypeFilter.Add(".png");

                        //Save and replace feed icon locally
                        StorageFile StorageFile = await FileOpenPicker.PickSingleFileAsync();
                        if (StorageFile != null)
                        {
                            await StorageFile.CopyAndReplaceAsync(await ApplicationData.Current.LocalFolder.CreateFileAsync(SelectedItem.feed_id + ".png", CreationCollisionOption.ReplaceExisting));

                            //Load the feed icon
                            BitmapImage FeedIcon = null;
                            if (SelectedItem.feed_id.StartsWith("user/")) { FeedIcon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconUser-Dark.png", false); } else { FeedIcon = await AVImage.LoadBitmapImage("ms-appdata:///local/" + SelectedItem.feed_id + ".png", false); }
                            if (FeedIcon == null) { FeedIcon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconRSS-Dark.png", false); }

                            SelectedItem.feed_icon = FeedIcon;
                        }
                    }
                    else if (MsgBoxResult == 2)
                    {
                        //Delete the feed icon
                        IStorageItem LocalFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync(SelectedItem.feed_id + ".png");
                        if (LocalFile != null) { try { await LocalFile.DeleteAsync(StorageDeleteOption.PermanentDelete); } catch { } }

                        //Load default feed icon
                        SelectedItem.feed_icon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconRSS-Dark.png", false);

                        //Reset the online status
                        OnlineUpdateFeeds = true;
                        ApiMessageError = string.Empty;

                        await AVMessageBox.Popup("Feed icon reset", "The feed icon has been reset and will be refreshed on the next online feed update, you can refresh the feeds by clicking on the refresh icon above.", "Ok", "", "", "", "", false);
                    }
                }
            }
            catch { }
        }

        //Add feed to The Old Reader
        async void btn_AddFeed_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            try
            {
                //Hide the keyboard
                InputPane.GetForCurrentView().TryHide();

                //Add feed to the api
                await AddFeedToApi();
            }
            catch { }
        }

        //Add feed to the api
        private async Task AddFeedToApi()
        {
            try
            {
                //Check if user is logged in
                if (!CheckLogin())
                {
                    await AVMessageBox.Popup("Not logged in", "Adding a feed can only be done when you are logged in.", "Ok", "", "", "", "", false);
                    return;
                }

                //Check for internet connection
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    await AVMessageBox.Popup("No internet connection", "Adding a feed can only be done when there is an internet connection available.", "Ok", "", "", "", "", false);
                    return;
                }

                //Remove ending / characters from the url
                txtbox_AddFeed.Text = AVFunctions.StringRemoveEnd(txtbox_AddFeed.Text, "/");

                //Check if there is an url entered
                if (string.IsNullOrWhiteSpace(txtbox_AddFeed.Text))
                {
                    //Focus on the text box to open keyboard
                    txtbox_AddFeed.IsEnabled = false;
                    txtbox_AddFeed.IsEnabled = true;
                    txtbox_AddFeed.Focus(FocusState.Programmatic);
                    return;
                }

                //Validate the url entered
                if (!Regex.IsMatch(txtbox_AddFeed.Text, @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$"))
                {
                    await AVMessageBox.Popup("Invalid feed link", "The entered feed link is invalid or does not contain a feed, please check your link and try again.", "Ok", "", "", "", "", false);

                    //Focus on the text box to open keyboard
                    txtbox_AddFeed.IsEnabled = false;
                    txtbox_AddFeed.IsEnabled = true;
                    txtbox_AddFeed.Focus(FocusState.Programmatic);
                    return;
                }

                await ProgressDisableUI("Adding feed: " + txtbox_AddFeed.Text, true);

                if (await AddFeed(txtbox_AddFeed.Text))
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateNews = true;
                    OnlineUpdateStarred = true;
                    ApiMessageError = string.Empty;

                    //Reset the last update setting
                    AppVariables.ApplicationSettings["LastItemsUpdate"] = "Never";

                    //Reset the textbox entry
                    txtbox_AddFeed.Text = string.Empty;

                    //Load all the feeds
                    await LoadFeeds();
                }
                else
                {
                    //Focus on the text box to open keyboard
                    txtbox_AddFeed.IsEnabled = false;
                    txtbox_AddFeed.IsEnabled = true;
                    txtbox_AddFeed.Focus(FocusState.Programmatic);
                }

                await ProgressEnableUI();
            }
            catch { }
        }

        //Load feeds from Api
        async Task LoadFeeds()
        {
            try
            {
                //Clear all items
                await ClearObservableCollection(List_Feeds);

                //Update the loading information
                txt_AppInfo.Text = "Loading feeds";
                txt_NewsScrollInfo.Text = "Your feeds will be shown here shortly...";
                txt_NewsScrollInfo.Visibility = Visibility.Visible;

                //Load feeds from api/database
                AppVariables.LoadNews = false;
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

                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                //Set all items to list
                List<TableFeeds> LoadTableFeeds = await SQLConnection.Table<TableFeeds>().OrderBy(x => x.feed_folder).ToListAsync();

                //Load items into the list
                await ProcessItemLoad.DatabaseToList(LoadTableFeeds, null, AppVariables.CurrentItemsLoaded, AppVariables.ItemsToScrollLoad, false, false);

                //Update the total items count
                await UpdateTotalItemsCount(LoadTableFeeds, null, false, true);
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
                            txt_AppInfo.Text = ApiMessageError + AppVariables.CurrentTotalItemsCount + " feeds";
                            txt_NewsScrollInfo.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            txt_AppInfo.Text = ApiMessageError + "No feeds";
                            txt_NewsScrollInfo.Text = "It seems like you don't have any feeds added to your account, please add some feeds to start loading your feeds and click on the refresh button above.";
                            txt_NewsScrollInfo.Visibility = Visibility.Visible;
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }

        //Ignore feeds in database
        async void btn_IgnoreFeeds_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check for selected items
                if (ListView_Items.SelectedItems.Count == 0)
                {
                    await AVMessageBox.Popup("No feeds selected", "Please select some feeds that you want to un/ignore first.", "Ok", "", "", "", "", false);
                    return;
                }
                else
                {
                    try
                    {
                        //Wait for busy application
                        await ApiUpdate.WaitForBusyApplication();

                        await ProgressDisableUI("Un/ignoring selected feeds...", true);

                        //Wait for busy database
                        await ApiUpdate.WaitForBusyDatabase();

                        List<TableFeeds> TableEditFeeds = await SQLConnection.Table<TableFeeds>().ToListAsync();

                        foreach (Feeds SelectedItem in ListView_Items.SelectedItems)
                        {
                            TableFeeds TableResult = TableEditFeeds.Where(x => x.feed_id == SelectedItem.feed_id).FirstOrDefault();
                            if (SelectedItem.feed_ignore_status == Visibility.Visible)
                            {
                                TableResult.feed_ignore_status = false;
                                SelectedItem.feed_ignore_status = Visibility.Collapsed;
                            }
                            else
                            {
                                TableResult.feed_ignore_status = true;
                                SelectedItem.feed_ignore_status = Visibility.Visible;
                            }
                        }

                        //Update the items in database
                        await SQLConnection.UpdateAllAsync(TableEditFeeds);

                        //Reset the list selection
                        ListView_Items.SelectedIndex = -1;
                        await AVMessageBox.Popup("Feeds have been un/ignored", "Their items will be hidden or shown again on the next news item refresh.", "Ok", "", "", "", "", false);
                    }
                    catch
                    {
                        await AVMessageBox.Popup("Failed to un/ignore feeds", "Please try to un/ignored the feeds again.", "Ok", "", "", "", "", false);
                    }

                    await ProgressEnableUI();
                }
            }
            catch { }
        }

        //Delete feeds from The Old Reader/Database
        async void btn_DeleteFeeds_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check for internet connection
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    await AVMessageBox.Popup("No internet connection", "Deleting a feed can only be done when there is an internet connection available.", "Ok", "", "", "", "", false);
                    return;
                }

                //Check for selected items
                if (ListView_Items.SelectedItems.Count == 0)
                {
                    await AVMessageBox.Popup("No feeds selected", "Please select some feeds that you want to delete first.", "Ok", "", "", "", "", false);
                    return;
                }
                else
                {
                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    await ProgressDisableUI("Deleting the selected feeds...", true);

                    try
                    {
                        foreach (Feeds SelectedItem in ListView_Items.SelectedItems) { await DeleteFeed(SelectedItem.feed_id); }
                        await AVMessageBox.Popup("Feeds have been deleted", "The feeds and it's items will disappear on the next refresh.", "Ok", "", "", "", "", false);
                    }
                    catch
                    {
                        await AVMessageBox.Popup("Failed to delete feeds", "Please check your account settings, internet connection and try again.", "Ok", "", "", "", "", false);
                    }

                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    ApiMessageError = string.Empty;

                    //Load all the feeds
                    await LoadFeeds();

                    await ProgressEnableUI();
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
                        //Disable Manage
                        StackPanel_Manage.IsHitTestVisible = false;
                        StackPanel_Manage.Opacity = 0.30;

                        //Disable the ListView
                        ListView_Items.IsHitTestVisible = false;
                        ListView_Items.Opacity = 0.30;

                        //Disable UI Buttons
                        iconNews.IsHitTestVisible = false;
                        iconNews.Opacity = 0.30;
                        iconStar.IsHitTestVisible = false;
                        iconStar.Opacity = 0.30;
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
                    textblock_StatusApplication.Text = string.Empty;

                    //Enable Manage
                    StackPanel_Manage.IsHitTestVisible = true;
                    StackPanel_Manage.Opacity = 1;

                    //Enable the ListView
                    ListView_Items.IsHitTestVisible = true;
                    ListView_Items.Opacity = 1;

                    //Enable UI Buttons
                    iconNews.IsHitTestVisible = true;
                    iconNews.Opacity = 1;
                    iconStar.IsHitTestVisible = true;
                    iconStar.Opacity = 1;
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
    }
}