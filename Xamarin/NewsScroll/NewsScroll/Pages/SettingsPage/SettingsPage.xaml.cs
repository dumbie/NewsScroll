using ArnoldVinkCode;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;
using static NewsScroll.Database.Database;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class SettingsPage : ContentPage
    {
        //Page variables
        private string vPreviousAccount = string.Empty;

        public SettingsPage()
        {
            InitializeComponent();
            Page_Loaded();
        }

        private async void Page_Loaded()
        {
            try
            {
                //Load and set the settings
                SettingsLoad();
                SettingsSave();

                //Store the previous username
                vPreviousAccount = AppSettingLoad("ApiAccount").ToString();

                //Load and set database size
                await UpdateSizeInformation();
            }
            catch { }
        }

        async void iconHelp_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                App.NavigateToPage(new HelpPage(), true, false);
            }
            catch { }
        }

        //Check if the account has changed
        async Task CheckAccountChange()
        {
            try
            {
                if (vPreviousAccount != string.Empty && vPreviousAccount != AppSettingLoad("ApiAccount").ToString())
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
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Enter account");
                messageAnswers.Add("Register account");
                messageAnswers.Add("Exit application");

                string messageResult = await MessagePopup.Popup("No account is set", "Please set your account email and password to start using News Scroll.", messageAnswers);
                if (messageResult == "Enter account")
                {
                    //Focus on the text box to open keyboard
                    setting_ApiAccount.IsEnabled = false;
                    setting_ApiAccount.IsEnabled = true;
                    setting_ApiAccount.Focus();
                    return;
                }
                if (messageResult == "Register account")
                {
                    btn_RegisterAccount_Click(null, null);
                    return;
                }
                else if (messageResult == "Exit application")
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    return;
                }
            }
            catch { }
        }

        //User Interface - Buttons
        async void iconNews_Tap(object sender, EventArgs e)
        {
            try
            {
                //Check username and password
                if (!string.IsNullOrWhiteSpace(AppSettingLoad("ApiAccount").ToString()) && !string.IsNullOrWhiteSpace(AppSettingLoad("ApiPassword").ToString()))
                {
                    HideShowMenu(true);

                    //Check if account has changed
                    await CheckAccountChange();

                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    App.NavigateToPage(new NewsPage(), true, false);
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

        async void iconStar_Tap(object sender, EventArgs e)
        {
            try
            {
                //Check username and password
                if (!string.IsNullOrWhiteSpace(AppSettingLoad("ApiAccount").ToString()) && !string.IsNullOrWhiteSpace(AppSettingLoad("ApiPassword").ToString()))
                {
                    HideShowMenu(true);

                    //Check if account has changed
                    await CheckAccountChange();

                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    App.NavigateToPage(new StarredPage(), true, false);
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

        async void iconSearch_Tap(object sender, EventArgs e)
        {
            try
            {
                //Check username and password
                if (!string.IsNullOrWhiteSpace(AppSettingLoad("ApiAccount").ToString()) && !string.IsNullOrWhiteSpace(AppSettingLoad("ApiPassword").ToString()))
                {
                    HideShowMenu(true);

                    //Check if account has changed
                    await CheckAccountChange();

                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    App.NavigateToPage(new SearchPage(), true, false);
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

        async void iconApi_Tap(object sender, EventArgs e)
        {
            try
            {
                //Check username and password
                if (!string.IsNullOrWhiteSpace(AppSettingLoad("ApiAccount").ToString()) && !string.IsNullOrWhiteSpace(AppSettingLoad("ApiPassword").ToString()))
                {
                    HideShowMenu(true);

                    //Check if account has changed
                    await CheckAccountChange();

                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    App.NavigateToPage(new ApiPage(), true, false);
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

        private void iconPersonalize_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                PersonalizePopup.Popup();
            }
            catch { }
        }

        //Open Project Website
        async void ProjectWebsite_Click(object sender, EventArgs e)
        {
            try
            {
                await Browser.OpenAsync(new Uri("https://projects.arnoldvink.com"), BrowserLaunchMode.SystemPreferred);
            }
            catch { }
        }

        //Open register account
        async void btn_RegisterAccount_Click(object sender, EventArgs e)
        {
            try
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    await Browser.OpenAsync(new Uri("https://theoldreader.com/users/sign_up"), BrowserLaunchMode.SystemPreferred);
                }
                else
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("No internet connection", "You can't register an account when there is no internet connection available.", messageAnswers);
                }
            }
            catch { }
        }

        //Clear Database Click
        async void ClearStoredItems_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Clear database");
                messageAnswers.Add("Cancel");

                string messageResult = await MessagePopup.Popup("Clear database", "Do you want to clear all your stored offline feeds and items? All the feeds and items will need to be downloaded again.", messageAnswers);
                if (messageResult == "Clear database")
                {
                    await ClearDatabase();
                }
            }
            catch { }
        }

        //Clear Database Thread
        async Task ClearDatabase()
        {
            ProgressDisableUI("Clearing stored items...");
            try
            {
                //Reset the online status
                OnlineUpdateFeeds = true;
                OnlineUpdateNews = true;
                OnlineUpdateStarred = true;
                ApiMessageError = string.Empty;

                //Reset the last update setting
                await AppSettingSave("LastItemsUpdate", "Never");

                await ClearObservableCollection(List_Feeds);
                await ClearObservableCollection(List_FeedSelect);
                await ClearObservableCollection(List_NewsItems);
                await ClearObservableCollection(List_SearchItems);
                await ClearObservableCollection(List_StarredItems);

                await vSQLConnection.DeleteAllAsync<TableFeeds>();
                await vSQLConnection.DropTableAsync<TableFeeds>();
                await vSQLConnection.CreateTableAsync<TableFeeds>();

                await vSQLConnection.DeleteAllAsync<TableOffline>();
                await vSQLConnection.DropTableAsync<TableOffline>();
                await vSQLConnection.CreateTableAsync<TableOffline>();

                await vSQLConnection.DeleteAllAsync<TableItems>();
                await vSQLConnection.DropTableAsync<TableItems>();
                await vSQLConnection.CreateTableAsync<TableItems>();

                await vSQLConnection.DeleteAllAsync<TableSearchHistory>();
                await vSQLConnection.DropTableAsync<TableSearchHistory>();
                await vSQLConnection.CreateTableAsync<TableSearchHistory>();

                //Delete all feed icons from local storage
                foreach (string localFile in AVFiles.Directory_ListFiles(string.Empty, true))
                {
                    try
                    {
                        if (localFile.EndsWith(".png"))
                        {
                            AVFiles.File_Delete(localFile, false);
                        }
                    }
                    catch { }
                }

                //Load and set database size
                await UpdateSizeInformation();
            }
            catch { }
            ProgressEnableUI();
        }

        //Load and set database size
        async Task UpdateSizeInformation()
        {
            try
            {
                string DatabaseSize = GetDatabaseSize();
                int TotalItems = await vSQLConnection.Table<TableItems>().CountAsync();
                int TotalFeeds = await vSQLConnection.Table<TableFeeds>().CountAsync();

                txt_OfflineStoredSize.Text = "Offline stored database is " + DatabaseSize + "\nin size and contains a total of " + TotalItems + "\nstored items and has " + TotalFeeds + " feeds in it.";
            }
            catch { }
        }

        //Progressbar/UI Status
        void ProgressDisableUI(string ProgressMsg)
        {
            Device.BeginInvokeOnMainThread(() =>
          {
              try
              {
                  AppVariables.BusyApplication = true;

                  //Enable progressbar
                  label_StatusApplication.Text = ProgressMsg;
                  grid_StatusApplication.IsVisible = true;

                  //Disable Content
                  page_Content.IsEnabled = false;
                  page_Content.Opacity = 0.30;

                  //Disable UI Buttons
                  iconApi.IsEnabled = false;
                  iconApi.Opacity = 0.30;
                  iconHelp.IsEnabled = false;
                  iconHelp.Opacity = 0.30;
                  iconNews.IsEnabled = false;
                  iconNews.Opacity = 0.30;
                  iconStar.IsEnabled = false;
                  iconStar.Opacity = 0.30;
                  iconSearch.IsEnabled = false;
                  iconSearch.Opacity = 0.30;
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
                  label_StatusApplication.Text = string.Empty;
                  grid_StatusApplication.IsVisible = false;

                  //Enable Content
                  page_Content.IsEnabled = true;
                  page_Content.Opacity = 1;

                  //Enable UI Buttons
                  iconApi.IsEnabled = true;
                  iconApi.Opacity = 1;
                  iconHelp.IsEnabled = true;
                  iconHelp.Opacity = 1;
                  iconNews.IsEnabled = true;
                  iconNews.Opacity = 1;
                  iconStar.IsEnabled = true;
                  iconStar.Opacity = 1;
                  iconSearch.IsEnabled = true;
                  iconSearch.Opacity = 1;

                  AppVariables.BusyApplication = false;
              }
              catch { AppVariables.BusyApplication = false; }
          });
        }
    }
}