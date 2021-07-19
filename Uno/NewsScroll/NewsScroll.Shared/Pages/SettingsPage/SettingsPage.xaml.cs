using ArnoldVinkCode;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static NewsScroll.Api.Api;
using static NewsScroll.Database.Database;
using static NewsScroll.Lists.Lists;
using static NewsScroll.Startup.Startup;

namespace NewsScroll
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        //Page variables
        private string vPreviousAccount = string.Empty;

        //Application Navigation
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Loaded += async delegate
            {
                try
                {
                    //Load and set the settings
                    SettingsLoad();
                    SettingsSave();

                    //Hide the switch fullscreen mode
                    if (!AVFunctions.DevMobile()) { button_SwitchScreenMode.Visibility = Visibility.Visible; }

                    //Store the previous username
                    vPreviousAccount = AppVariables.ApplicationSettings["ApiAccount"].ToString();

                    //Load and set database size
                    await UpdateSizeInformation();
                }
                catch { }
            };
        }

        async void iconHelp_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                App.vApplicationFrame.Navigate(typeof(HelpPage));
                //FixApp.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }

        //Check if the account has changed
        async Task CheckAccountChange()
        {
            try
            {
                if (vPreviousAccount != string.Empty && vPreviousAccount != AppVariables.ApplicationSettings["ApiAccount"].ToString())
                {
                    //Clear the database
                    await ClearDatabase();
                }
            }
            catch { }
        }

        //No account prompt
        private async Task NoAccountPrompt()
        {
            try
            {
                int MessageBoxResult = await MessagePopup.Popup("No account is set", "Please set your account email and password to start using News Scroll.", "Enter account", "Register account", "Exit application", "", "", false);
                if (MessageBoxResult == 1)
                {
                    //Focus on the text box to open keyboard
                    setting_ApiAccount.IsEnabled = false;
                    setting_ApiAccount.IsEnabled = true;
                    setting_ApiAccount.Focus(FocusState.Programmatic);
                    return;
                }
                if (MessageBoxResult == 2)
                {
                    btn_RegisterAccount_Click(null, null);
                    return;
                }
                else if (MessageBoxResult == 3)
                {
                    Application.Current.Exit();
                    return;
                }
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
                //Check username and password
                if (!string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiAccount"].ToString()) && !string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiPassword"].ToString()))
                {
                    HideShowMenu(true);

                    //Check if account has changed
                    await CheckAccountChange();

                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    App.vApplicationFrame.Navigate(typeof(NewsPage));
                    //FixApp.vApplicationFrame.BackStack.Clear();
                    return;
                }
                else
                {
                    HideShowMenu(true);
                    await NoAccountPrompt();
                }
            }
            catch { }
        }

        private void button_SwitchScreenMode_Tap(object sender, RoutedEventArgs e)
        {
            try { SwitchScreenMode(); } catch { }
        }

        async void iconStar_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check username and password
                if (!string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiAccount"].ToString()) && !string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiPassword"].ToString()))
                {
                    HideShowMenu(true);

                    //Check if account has changed
                    await CheckAccountChange();

                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    App.vApplicationFrame.Navigate(typeof(StarredPage));
                    //FixApp.vApplicationFrame.BackStack.Clear();
                    return;
                }
                else
                {
                    HideShowMenu(true);
                    await NoAccountPrompt();
                }
            }
            catch { }
        }

        async void iconSearch_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check username and password
                if (!string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiAccount"].ToString()) && !string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiPassword"].ToString()))
                {
                    HideShowMenu(true);

                    //Check if account has changed
                    await CheckAccountChange();

                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    App.vApplicationFrame.Navigate(typeof(SearchPage));
                    //FixApp.vApplicationFrame.BackStack.Clear();
                    return;
                }
                else
                {
                    HideShowMenu(true);
                    await NoAccountPrompt();
                }
            }
            catch { }
        }

        async void iconApi_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check username and password
                if (!string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiAccount"].ToString()) && !string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiPassword"].ToString()))
                {
                    HideShowMenu(true);

                    //Check if account has changed
                    await CheckAccountChange();

                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    App.vApplicationFrame.Navigate(typeof(ApiPage));
                    //FixApp.vApplicationFrame.BackStack.Clear();
                    return;
                }
                else
                {
                    HideShowMenu(true);
                    await NoAccountPrompt();
                }
            }
            catch { }
        }

        //Open Project Website
        async void ProjectWebsite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AVFunctions.DevMobile()) { await Launcher.LaunchUriAsync(new Uri("https://m.arnoldvink.com/?p=projects")); }
                else { await Launcher.LaunchUriAsync(new Uri("https://projects.arnoldvink.com")); }
            }
            catch { }
        }

        //Open register account
        async void btn_RegisterAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    await Launcher.LaunchUriAsync(new Uri("https://theoldreader.com/users/sign_up"));
                }
                else
                {
                    await MessagePopup.Popup("No internet connection", "You can't register an account when there is no internet connection available.", "Ok", "", "", "", "", false);
                }
            }
            catch { }
        }

        //Open WiFi Settings
        async void btn_OpenWiFiSettings_Click(object sender, RoutedEventArgs e)
        {
            try { await Launcher.LaunchUriAsync(new Uri("ms-settings:network-wifi")); } catch { }
        }

        //Clear Database Click
        async void ClearStoredItems_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int MessageBoxResult = await MessagePopup.Popup("Clear database", "Do you want to clear all your stored offline feeds and items? All the feeds and items will need to be downloaded again.", "Clear database", "", "", "", "", true);
                if (MessageBoxResult == 1)
                {
                    await ClearDatabase();
                }
            }
            catch { }
        }

        //Clear Database Thread
        async Task ClearDatabase()
        {
            await ProgressDisableUI("Clearing stored items...");
            try
            {
                //Reset the online status
                OnlineUpdateFeeds = true;
                OnlineUpdateNews = true;
                OnlineUpdateStarred = true;
                ApiMessageError = string.Empty;

                //Reset the last update setting
                AppVariables.ApplicationSettings["LastItemsUpdate"] = "Never";

                await ClearObservableCollection(List_Feeds);
                await ClearObservableCollection(List_FeedSelect);
                await ClearObservableCollection(List_NewsItems);
                await ClearObservableCollection(List_SearchItems);
                await ClearObservableCollection(List_StarredItems);

                await SQLConnection.DeleteAllAsync<TableFeeds>();
                await SQLConnection.DropTableAsync<TableFeeds>();
                await SQLConnection.CreateTableAsync<TableFeeds>();

                await SQLConnection.DeleteAllAsync<TableOffline>();
                await SQLConnection.DropTableAsync<TableOffline>();
                await SQLConnection.CreateTableAsync<TableOffline>();

                await SQLConnection.DeleteAllAsync<TableItems>();
                await SQLConnection.DropTableAsync<TableItems>();
                await SQLConnection.CreateTableAsync<TableItems>();

                await SQLConnection.DeleteAllAsync<TableSearchHistory>();
                await SQLConnection.DropTableAsync<TableSearchHistory>();
                await SQLConnection.CreateTableAsync<TableSearchHistory>();

                //Delete all feed icons from local storage
                foreach (IStorageItem LocalFile in await ApplicationData.Current.LocalFolder.GetItemsAsync())
                {
                    try
                    {
                        if (LocalFile.Name.EndsWith(".png"))
                        {
                            await LocalFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }
                    }
                    catch { }
                }

                //Load and set database size
                await UpdateSizeInformation();
            }
            catch { }
            await ProgressEnableUI();
        }

        //Load and set database size
        async Task UpdateSizeInformation()
        {
            try
            {
                string DatabaseSize = await GetDatabaseSize();
                int TotalItems = await SQLConnection.Table<TableItems>().CountAsync();
                int TotalFeeds = await SQLConnection.Table<TableFeeds>().CountAsync();

                txt_OfflineStoredSize.Text = "Offline stored database is " + DatabaseSize + "\nin size and contains a total of " + TotalItems + "\nstored items and has " + TotalFeeds + " feeds in it.";
            }
            catch { }
        }

        //Progressbar/UI Status
        async Task ProgressDisableUI(string ProgressMsg)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    AppVariables.BusyApplication = true;

                    //Enable progressbar
                    textblock_StatusApplication.Text = ProgressMsg;
                    grid_StatusApplication.Visibility = Visibility.Visible;

                    //Disable Content
                    page_Content.IsHitTestVisible = false;
                    page_Content.Opacity = 0.30;

                    //Disable UI Buttons
                    iconApi.IsHitTestVisible = false;
                    iconApi.Opacity = 0.30;
                    iconHelp.IsHitTestVisible = false;
                    iconHelp.Opacity = 0.30;
                    iconNews.IsHitTestVisible = false;
                    iconNews.Opacity = 0.30;
                    iconStar.IsHitTestVisible = false;
                    iconStar.Opacity = 0.30;
                    iconSearch.IsHitTestVisible = false;
                    iconSearch.Opacity = 0.30;
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
                    textblock_StatusApplication.Text = string.Empty;
                    grid_StatusApplication.Visibility = Visibility.Collapsed;

                    //Enable Content
                    page_Content.IsHitTestVisible = true;
                    page_Content.Opacity = 1;

                    //Enable UI Buttons
                    iconApi.IsHitTestVisible = true;
                    iconApi.Opacity = 1;
                    iconHelp.IsHitTestVisible = true;
                    iconHelp.Opacity = 1;
                    iconNews.IsHitTestVisible = true;
                    iconNews.Opacity = 1;
                    iconStar.IsHitTestVisible = true;
                    iconStar.Opacity = 1;
                    iconSearch.IsHitTestVisible = true;
                    iconSearch.Opacity = 1;

                    AppVariables.BusyApplication = false;
                }
                catch { AppVariables.BusyApplication = false; }
            });
        }
    }
}