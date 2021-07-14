using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class ApiPage : ContentPage
    {
        public ApiPage()
        {
            InitializeComponent();
            Page_Loaded();
        }

        //Application Navigation
        private async void Page_Loaded()
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
                listview_Items.ItemsSource = List_Feeds;

                ////Set the autosuggestbox inputscope
                //TextBox SuggestTextBox = AVFunctions.FindVisualChild<TextBox>(txtbox_AddFeed);
                //InputScope inputScope = new InputScope();
                //InputScopeName inputScopeName = new InputScopeName() { NameValue = InputScopeNameValue.Url };
                //inputScope.Names.Add(inputScopeName);
                //SuggestTextBox.InputScope = inputScope;

                //Load all the feeds
                await LoadFeeds();
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
                EventHideProgressionStatus += new DelegateHideProgressionStatus(HideProgressionStatus);
                EventUpdateTotalItemsCount += new DelegateUpdateTotalItemsCount(UpdateTotalItemsCount);
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

                listview_Items.ItemsSource = null;

                await ClearObservableCollection(List_Feeds);
                await ClearObservableCollection(List_FeedSelect);
                await ClearObservableCollection(List_NewsItems);
                await ClearObservableCollection(List_SearchItems);
                await ClearObservableCollection(List_StarredItems);
            }
            catch { }
        }

        //User Interface - Buttons
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

        async void iconRefresh_Tap(object sender, EventArgs e)
        {
            try
            {
                await RefreshFeeds();
            }
            catch { }
        }

        private async Task RefreshFeeds()
        {
            try
            {
                HideShowMenu(true);

                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Refresh feeds");
                messageAnswers.Add("Cancel");

                string messageResult = await MessagePopup.Popup("Refresh feeds", "Do you want to refresh the feeds and scroll to the top?", messageAnswers);
                if (messageResult == "Refresh feeds")
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

        //Set the feed folder
        private void btn_SetFolder_Clicked(object sender, EventArgs e)
        {
            try
            {
                //https://theoldreader.com/reader/api/0/subscription/edit
                //s=feed/FeedId
                //a=user/-/label/FolderTitle
            }
            catch { }
        }

        //Set the feed icon
        private async void btn_SetIcon_Clicked(object sender, EventArgs e)
        {
            try
            {
                Feeds SelectedItem = (Feeds)listview_Items.SelectedItem;
                if (SelectedItem != null)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Set custom icon");
                    messageAnswers.Add("Reset the icon");
                    messageAnswers.Add("Cancel");

                    string messageResult = await MessagePopup.Popup("Change the feed icon", "Would you like to set a custom feed icon for " + SelectedItem.feed_title + "?", messageAnswers);
                    if (messageResult == "Set custom icon")
                    {
                        Debug.WriteLine("Changing icon for feed: " + SelectedItem.feed_id + " / " + SelectedItem.feed_title);

                        PickOptions pickOptions = new PickOptions();
                        pickOptions.FileTypes = FilePickerFileType.Png;

                        FileResult pickResult = await FilePicker.PickAsync(pickOptions);
                        if (pickResult != null)
                        {
                            //Load feed icon
                            Stream imageStream = await pickResult.OpenReadAsync();

                            //Update feed icon
                            imageStream.Position = 0;
                            SelectedItem.feed_icon = ImageSource.FromStream(() => imageStream);

                            //Save feed icon
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                imageStream.Position = 0;
                                await imageStream.CopyToAsync(memoryStream);
                                byte[] imageBytes = memoryStream.ToArray();
                                AVFiles.File_SaveBytes(SelectedItem.feed_id + ".png", imageBytes, true, true);
                            }
                        }
                    }
                    else if (messageResult == "Reset the icon")
                    {
                        //Delete the feed icon
                        AVFiles.File_Delete(SelectedItem.feed_id + ".png", true);

                        //Load default feed icon
                        SelectedItem.feed_icon = ImageSource.FromResource("NewsScroll.Assets.iconRSS-Dark.png");

                        //Reset the online status
                        OnlineUpdateFeeds = true;
                        ApiMessageError = string.Empty;

                        List<string> messageAnswersReset = new List<string>();
                        messageAnswersReset.Add("Ok");

                        await MessagePopup.Popup("Feed icon reset", "The feed icon has been reset and will be refreshed on the next online feed update, you can refresh the feeds by clicking on the refresh icon above.", messageAnswersReset);
                    }
                }
            }
            catch { }
        }

        //Add feed to The Old Reader
        private async void button_AddFeed_Clicked(object sender, EventArgs e)
        {
            try
            {
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
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("Not logged in", "Adding a feed can only be done when you are logged in.", messageAnswers);
                    return;
                }

                //Check for internet connection
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("No internet connection", "Adding a feed can only be done when there is an internet connection available.", messageAnswers);
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
                    txtbox_AddFeed.Focus();
                    return;
                }

                //Validate the url entered
                if (!Regex.IsMatch(txtbox_AddFeed.Text, @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$"))
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("Invalid feed link", "The entered feed link is invalid or does not contain a feed, please check your link and try again.", messageAnswers);

                    //Focus on the text box to open keyboard
                    txtbox_AddFeed.IsEnabled = false;
                    txtbox_AddFeed.IsEnabled = true;
                    txtbox_AddFeed.Focus();
                    return;
                }

                ProgressDisableUI("Adding feed: " + txtbox_AddFeed.Text, true);

                if (await AddFeed(txtbox_AddFeed.Text))
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateNews = true;
                    OnlineUpdateStarred = true;
                    ApiMessageError = string.Empty;

                    //Reset the last update setting
                    await AppSettingSave("LastItemsUpdate", "Never");

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
                    txtbox_AddFeed.Focus();
                }

                ProgressEnableUI();
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
                txt_NewsScrollInfo.IsVisible = true;

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
                    App.NavigateToPage(new SettingsPage(), true, false);
                    return;
                }

                //Set all items to list
                List<TableFeeds> LoadTableFeeds = await vSQLConnection.Table<TableFeeds>().OrderBy(x => x.feed_folder).ToListAsync();

                //Load items into the list
                await ProcessItemLoad.DatabaseToList(LoadTableFeeds, null, AppVariables.CurrentItemsLoaded, AppVariables.ItemsToLoadMax, false, false);

                //Update the total items count
                await UpdateTotalItemsCount(LoadTableFeeds, null, false, true);
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

        //Update the total items count
        private async Task UpdateTotalItemsCount(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, bool Silent, bool EnableUI)
        {
            try
            {
                //Set the total item count
                AppVariables.CurrentTotalItemsCount = await ProcessItemLoad.DatabaseToCount(LoadTableFeeds, LoadTableItems, Silent, EnableUI);
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        //Check the total item count
                        if (AppVariables.CurrentTotalItemsCount > 0)
                        {
                            txt_AppInfo.Text = ApiMessageError + AppVariables.CurrentTotalItemsCount + " feeds";
                            txt_NewsScrollInfo.IsVisible = false;
                        }
                        else
                        {
                            txt_AppInfo.Text = ApiMessageError + "No feeds";
                            txt_NewsScrollInfo.Text = "It seems like you don't have any feeds added to your account, please add some feeds to start loading your feeds and click on the refresh button above.";
                            txt_NewsScrollInfo.IsVisible = true;
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }

        //Ignore feeds in database
        async void btn_IgnoreFeeds_Click(object sender, EventArgs e)
        {
            try
            {
                //Check for selected items
                if (listview_Items.SelectedItem == null)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("No feeds selected", "Please select some feeds that you want to un/ignore first.", messageAnswers);
                    return;
                }
                else
                {
                    try
                    {
                        //Wait for busy application
                        await ApiUpdate.WaitForBusyApplication();

                        ProgressDisableUI("Un/ignoring selected feeds...", true);

                        List<TableFeeds> TableEditFeeds = await vSQLConnection.Table<TableFeeds>().ToListAsync();

                        Feeds SelectedItem = (Feeds)listview_Items.SelectedItem;
                        TableFeeds TableResult = TableEditFeeds.Where(x => x.feed_id == SelectedItem.feed_id).FirstOrDefault();
                        if (SelectedItem.feed_ignore_status == true)
                        {
                            TableResult.feed_ignore_status = false;
                            SelectedItem.feed_ignore_status = false;
                        }
                        else
                        {
                            TableResult.feed_ignore_status = true;
                            SelectedItem.feed_ignore_status = true;
                        }

                        //Update the items in database
                        await vSQLConnection.UpdateAllAsync(TableEditFeeds);

                        //Reset the list selection
                        listview_Items.SelectedItem = -1;

                        List<string> messageAnswers = new List<string>();
                        messageAnswers.Add("Ok");

                        await MessagePopup.Popup("Feeds have been un/ignored", "Their items will be hidden or shown again on the next news item refresh.", messageAnswers);
                    }
                    catch
                    {
                        List<string> messageAnswers = new List<string>();
                        messageAnswers.Add("Ok");

                        await MessagePopup.Popup("Failed to un/ignore feeds", "Please try to un/ignored the feeds again.", messageAnswers);
                    }

                    ProgressEnableUI();
                }
            }
            catch { }
        }

        //Delete feeds from The Old Reader/Database
        async void btn_DeleteFeeds_Click(object sender, EventArgs e)
        {
            try
            {
                //Check for internet connection
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("No internet connection", "Deleting a feed can only be done when there is an internet connection available.", messageAnswers);
                    return;
                }

                //Check for selected items
                if (listview_Items.SelectedItem == null)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("No feeds selected", "Please select some feeds that you want to delete first.", messageAnswers);
                    return;
                }
                else
                {
                    //Wait for busy application
                    await ApiUpdate.WaitForBusyApplication();

                    ProgressDisableUI("Deleting the selected feeds...", true);

                    try
                    {
                        Feeds SelectedItem = (Feeds)listview_Items.SelectedItem;
                        await DeleteFeed(SelectedItem.feed_id);

                        List<string> messageAnswers = new List<string>();
                        messageAnswers.Add("Ok");

                        await MessagePopup.Popup("Feeds have been deleted", "The feeds and it's items will disappear on the next refresh.", messageAnswers);
                    }
                    catch
                    {
                        List<string> messageAnswers = new List<string>();
                        messageAnswers.Add("Ok");

                        await MessagePopup.Popup("Failed to delete feeds", "Please check your account settings, internet connection and try again.", messageAnswers);
                    }

                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    ApiMessageError = string.Empty;

                    //Load all the feeds
                    await LoadFeeds();

                    ProgressEnableUI();
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
                        //Disable Manage
                        stackpanel_Manage.IsEnabled = false;
                        stackpanel_Manage.Opacity = 0.30;

                        //Disable the ListView
                        listview_Items.IsEnabled = false;
                        listview_Items.Opacity = 0.30;

                        //Disable UI Buttons
                        iconNews.IsEnabled = false;
                        iconNews.Opacity = 0.30;
                        iconStar.IsEnabled = false;
                        iconStar.Opacity = 0.30;
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

                    //Enable Manage
                    stackpanel_Manage.IsEnabled = true;
                    stackpanel_Manage.Opacity = 1;

                    //Enable the ListView
                    listview_Items.IsEnabled = true;
                    listview_Items.Opacity = 1;

                    //Enable UI Buttons
                    iconNews.IsEnabled = true;
                    iconNews.Opacity = 1;
                    iconStar.IsEnabled = true;
                    iconStar.Opacity = 1;
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
    }
}