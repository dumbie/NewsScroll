using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;

namespace NewsScroll
{
    public partial class ItemPopup : ContentPage
    {
        public ItemPopup(Items bindingItem)
        {
            try
            {
                InitializeComponent();
                this.BindingContext = this;
                OpenPopup(bindingItem);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed initializing itempopup: " + ex.Message);
            }
        }

        //Open the popup
        public async void OpenPopup(Items Source)
        {
            try
            {
                //Adjust the swiping direction
                SwipeBarAdjust();

                //Check if the header is hidden
                if (AppVariables.HeaderHidden) { HideShowHeader(true); }

                //Update the status message
                ProgressDisableUI("Loading the item...");

                //Set item source
                vItemViewerItem = Source;
                txt_AppInfo.Text = ApiMessageError + Source.feed_title;

                //Load feed icon
                if (vItemViewerItem.feed_id.StartsWith("user/"))
                {
                    image_feed_icon.Source = ImageSource.FromResource("NewsScroll.Assets.iconUser-Dark.png");
                }
                else
                {
                    image_feed_icon.Source = AVFiles.File_LoadImage(vItemViewerItem.feed_id + ".png", true);
                }
                if (image_feed_icon.Source == null)
                {
                    image_feed_icon.Source = ImageSource.FromResource("NewsScroll.Assets.iconRSS-Dark.png");
                }

                //Check if internet is available
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    iconItem.IsVisible = true;
                    iconBrowser.IsVisible = true;
                    button_LoadFullItem.IsVisible = true;
                    button_OpenInBrowser.IsVisible = true;
                }
                else
                {
                    iconItem.IsVisible = false;
                    iconBrowser.IsVisible = false;
                    button_LoadFullItem.IsVisible = false;
                    button_OpenInBrowser.IsVisible = false;
                }

                //Update the star status
                if (Source.item_star_status == false)
                {
                    iconStar.Source = ImageSource.FromResource("NewsScroll.Assets.iconStarAdd.png");
                }
                else
                {
                    iconStar.Source = ImageSource.FromResource("NewsScroll.Assets.iconStarRemove.png");
                }
                iconStar.IsVisible = true;

                //Load item into the viewer
                await LoadItem(string.Empty);

                //Register page events
                RegisterPageEvents();

                //Update the status message
                ProgressEnableUI();
            }
            catch
            {
                Debug.WriteLine("Failed loading item.");
                await ClosePopup();
            }
        }

        //Load item into the viewer
        private async Task LoadItem(string item_content_custom)
        {
            try
            {
                //Load the full item
                TableItems LoadTable = await vSQLConnection.Table<TableItems>().Where(x => x.item_id == vItemViewerItem.item_id).FirstOrDefaultAsync();
                if (LoadTable != null)
                {
                    //Set the date time string
                    DateTime convertedDate = DateTime.SpecifyKind(LoadTable.item_datetime, DateTimeKind.Utc).ToLocalTime();
                    string DateAuthorString = convertedDate.ToString(AppVariables.CultureInfoLocal.DateTimeFormat.LongDatePattern, AppVariables.CultureInfoLocal.DateTimeFormat) + ", " + convertedDate.ToString(AppVariables.CultureInfoLocal.DateTimeFormat.ShortTimePattern, AppVariables.CultureInfoLocal.DateTimeFormat);

                    //Add the author to date time
                    if (!string.IsNullOrWhiteSpace(LoadTable.item_author)) { DateAuthorString += " by " + LoadTable.item_author; }
                    tb_ItemDateString.Text = DateAuthorString;

                    //fix
                    ////Enable or disable text selection
                    //if ((bool)AppSettingLoad("ItemTextSelection"))
                    //{
                    //    tb_ItemTitle.IsTextSelectionEnabled = true;
                    //    tb_ItemDateString.IsTextSelectionEnabled = true;
                    //    item_content.IsTextSelectionEnabled = true;
                    //}
                    //else
                    //{
                    //    tb_ItemTitle.IsTextSelectionEnabled = false;
                    //    tb_ItemDateString.IsTextSelectionEnabled = false;
                    //    item_content.IsTextSelectionEnabled = false;
                    //}

                    //Load the item content
                    bool SetHtmlToRichLabel = false;
                    if (!string.IsNullOrWhiteSpace(item_content_custom))
                    {
                        SetHtmlToRichLabel = HtmlToStackLayout(item_content, item_content_custom, string.Empty);
                    }
                    else if (!string.IsNullOrWhiteSpace(LoadTable.item_content_full))
                    {
                        SetHtmlToRichLabel = HtmlToStackLayout(item_content, LoadTable.item_content_full, string.Empty);
                    }

                    //Check if html to xaml has failed
                    if (!SetHtmlToRichLabel || !item_content.Children.Any())
                    {
                        //Load summary text
                        Label labelSummary = new Label();
                        labelSummary.Text = AVFunctions.StringCut(LoadTable.item_content, AppVariables.MaximumItemTextLength, "...");

                        //Add paragraph to rich text block
                        item_content.Children.Clear();
                        item_content.Children.Add(labelSummary);
                    }

                    //Check if item content contains preview image
                    CheckItemContentContainsPreviewImage(LoadTable);
                }
            }
            catch { }
        }

        //Check if there are images and the preview image is included
        private void CheckItemContentContainsPreviewImage(TableItems loadTable)
        {
            try
            {
                //Check the preview image
                string itemImageLink = loadTable.item_image;
                if (string.IsNullOrWhiteSpace(itemImageLink))
                {
                    item_image.image_link = null;
                    item_image.IsVisible = false;
                    return;
                }

                //Check if there are images or the preview image is included
                IEnumerable<View> imageContainers = item_content.Children.Where(x => x.GetType() == typeof(ImageContainer));
                int itemImageCount = imageContainers.Count();
                bool foundPreviewImage = false;
                foreach (ImageContainer imageContainer in imageContainers)
                {
                    try
                    {
                        string CompareBitmapLink = Regex.Replace(imageContainer.image_link, @"^(?:http(?:s)?://)?(?:www(?:[0-9]+)?\.)?", string.Empty, RegexOptions.IgnoreCase).ToLower();
                        string CompareItemImageLink = Regex.Replace(itemImageLink, @"^(?:http(?:s)?://)?(?:www(?:[0-9]+)?\.)?", string.Empty, RegexOptions.IgnoreCase).ToLower();
                        //Debug.WriteLine("Comparing image: " + CompareBitmapLink + " vs " + CompareItemImageLink);
                        if (CompareBitmapLink == CompareItemImageLink)
                        {
                            foundPreviewImage = true;
                            break;
                        }
                    }
                    catch { }
                }

                //Update the preview image based on result
                if (itemImageCount == 0 || !foundPreviewImage)
                {
                    Debug.WriteLine("No media found in rich text block, showing item image: " + itemImageLink);
                    item_image.image_link = itemImageLink;
                    item_image.IsVisible = true;
                }
                else
                {
                    item_image.image_link = null;
                    item_image.IsVisible = false;
                }
            }
            catch
            {
                item_image.image_link = null;
                item_image.IsVisible = false;
            }
        }

        //Close the popup
        public async Task ClosePopup()
        {
            try
            {
                Debug.WriteLine("Closing the item popup...");

                //Cleanup xaml resources
                //item_image.Source = null;
                item_content.Children.Clear();

                //Disable page events
                DisablePageEvents();

                //Close the popup
                await Application.Current.MainPage.Navigation.PopModalAsync(false);
            }
            catch { }
        }

        //Register page events
        private void RegisterPageEvents()
        {
            try
            {
                //Monitor user touch swipe
                if (!(bool)AppSettingLoad("DisableSwipeActions"))
                {
                    SwipeBarAdjust();
                }

                //Hide show header
                EventHideShowHeader += new DelegateHideShowHeader(HideShowHeader);
            }
            catch { }
        }

        //Disable page events
        private void DisablePageEvents()
        {
            try
            {
                //Monitor user touch swipe
                grid_SwipeBar.GestureRecognizers.Clear();

                //Hide show header
                EventHideShowHeader -= new DelegateHideShowHeader(HideShowHeader);
            }
            catch { }
        }

        //Progressbar/UI Status
        void ProgressDisableUI(string ProgressMsg)
        {
            try
            {
                AppVariables.BusyApplication = true;

                //Enable progressbar
                label_StatusApplication.Text = ProgressMsg;
                grid_StatusApplication.IsVisible = true;

                //Disable content
                item_image.IsEnabled = false;
                item_image.Opacity = 0.30;
                item_content.IsEnabled = false;
                item_content.Opacity = 0.30;

                //Disable buttons
                iconStar.IsEnabled = false;
                iconStar.Opacity = 0.30;
                iconItem.IsEnabled = false;
                iconItem.Opacity = 0.30;
                iconBrowser.IsEnabled = false;
                iconBrowser.Opacity = 0.30;
                iconShare.IsEnabled = false;
                iconShare.Opacity = 0.30;
                iconBack.Opacity = 0.30;
                iconBack.IsEnabled = false;
                button_LoadFullItem.Opacity = 0.30;
                button_LoadFullItem.IsEnabled = false;
                button_OpenInBrowser.Opacity = 0.30;
                button_OpenInBrowser.IsEnabled = false;
                button_GoBackPage.Opacity = 0.30;
                button_GoBackPage.IsEnabled = false;
            }
            catch { AppVariables.BusyApplication = true; }
        }

        void ProgressEnableUI()
        {
            try
            {
                //Disable progressbar
                grid_StatusApplication.IsVisible = false;
                label_StatusApplication.Text = string.Empty;

                //Enable content
                item_image.IsEnabled = true;
                item_image.Opacity = 1;
                item_content.IsEnabled = true;
                item_content.Opacity = 1;

                //Enable buttons
                iconStar.IsEnabled = true;
                iconStar.Opacity = 1;
                iconItem.IsEnabled = true;
                iconItem.Opacity = 1;
                iconBrowser.IsEnabled = true;
                iconBrowser.Opacity = 1;
                iconShare.IsEnabled = true;
                iconShare.Opacity = 1;
                iconBack.Opacity = 1;
                iconBack.IsEnabled = true;
                button_LoadFullItem.Opacity = 1;
                button_LoadFullItem.IsEnabled = true;
                button_OpenInBrowser.Opacity = 1;
                button_OpenInBrowser.IsEnabled = true;
                button_GoBackPage.Opacity = 1;
                button_GoBackPage.IsEnabled = true;

                AppVariables.BusyApplication = false;
            }
            catch { AppVariables.BusyApplication = false; }
        }

        //User Interface - Buttons
        async void iconBack_Tap(object sender, EventArgs e)
        {
            try
            {
                await ClosePopup();
            }
            catch { }
        }

        async void iconShare_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                await ShareItem(vItemViewerItem);
            }
            catch { }
        }

        async void iconStar_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Update the status bar
                if (vItemViewerItem.item_star_status == false) { ProgressDisableUI("Starring the item..."); }
                else { ProgressDisableUI("Unstarring the item..."); }

                //Get and set the current page name
                string CurrentPageName = App.Current.MainPage.ToString();

                if (CurrentPageName.EndsWith("StarredPage"))
                {
                    bool MarkedAsStar = await MarkItemAsStarPrompt(vItemViewerItem, true, true, false, false, true);

                    //Update the header and selection feeds
                    if (MarkedAsStar) { await EventUpdateTotalItemsCount(null, null, false, true); }
                }
                else
                {
                    await MarkItemAsStarPrompt(vItemViewerItem, true, false, false, false, true);
                }

                //Update the star status
                if (vItemViewerItem.item_star_status == false)
                {
                    iconStar.Source = ImageSource.FromResource("NewsScroll.Assets.iconStarAdd.png");
                }
                else
                {
                    iconStar.Source = ImageSource.FromResource("NewsScroll.Assets.iconStarRemove.png");
                }

                ProgressEnableUI();
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

        async void iconBrowserMenu_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                await OpenBrowser(null, true);
            }
            catch { }
        }

        async void iconBrowserItem_Tap(object sender, EventArgs e)
        {
            try
            {
                await OpenBrowser(null, true);
            }
            catch { }
        }

        //Open item in browser
        private async Task OpenBrowser(Uri TargetUri, bool closePopup)
        {
            try
            {
                //Check internet connection
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("No internet connection", "You currently don't have an internet connection available to open this item or link in your webbrowser.", messageAnswers);
                    return;
                }

                //Close the popup
                if (closePopup)
                {
                    await ClosePopup();
                }

                //Open item in webbrowser
                if (TargetUri != null)
                {
                    await Browser.OpenAsync(TargetUri, BrowserLaunchMode.SystemPreferred);
                }
                else
                {
                    await Browser.OpenAsync(new Uri(vItemViewerItem.item_link), BrowserLaunchMode.SystemPreferred);
                }
            }
            catch { }
        }

        //Download and load the full item
        private async void LoadFullItem_Tap(object sender, EventArgs e)
        {
            try
            {
                await LoadFullItem();
            }
            catch { }
        }

        private async void iconItem_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                await LoadFullItem();
            }
            catch { }
        }

        private async Task LoadFullItem()
        {
            try
            {
                if (button_LoadFullItem.Text == "Load the original item")
                {
                    ProgressDisableUI("Loading the original item...");

                    //Load the original item content
                    string OriginalContent = vItemViewerItem.item_content_full;

                    //Load item into the viewer
                    await LoadItem(OriginalContent);

                    //Update the button text
                    button_LoadFullItem.Text = "Load the full item";
                    iconItem.Source = ImageSource.FromResource("NewsScroll.Assets.iconItemFull.png");

                    //Scroll to the top
                    await Task.Delay(10);
                    await scrollviewer_NewsItem.ScrollToAsync(0, 0, false);
                }
                else
                {
                    //Check if internet is available
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        ProgressDisableUI("Loading the full item...");

                        int RetryCount = 0;
                        await DownloadFullItemContent(RetryCount);
                    }
                    else
                    {
                        List<string> messageAnswers = new List<string>();
                        messageAnswers.Add("Ok");

                        Debug.WriteLine("No network available to fully load this item.");
                        await MessagePopup.Popup("No internet connection", "You currently don't have an internet connection available to fully load this item.", messageAnswers);
                    }
                }

                ProgressEnableUI();
            }
            catch { }
        }

        private async Task DownloadFullItemContent(int RetryCount)
        {
            try
            {
                //Download the full item content
                string FullContent = await WebParser(vItemViewerItem.item_link, false, true);
                if (!string.IsNullOrWhiteSpace(FullContent))
                {
                    //Load item into the viewer
                    await LoadItem(FullContent);

                    //Update the button text
                    button_LoadFullItem.Text = "Load the original item";
                    iconItem.Source = ImageSource.FromResource("NewsScroll.Assets.iconItemSumm.png");

                    //Scroll to the top
                    await Task.Delay(10);
                    await scrollviewer_NewsItem.ScrollToAsync(0, 0, false);
                }
                else
                {
                    if (RetryCount == 3)
                    {
                        //Check if internet is available
                        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                        {
                            Debug.WriteLine("There is currently no full item content available.");

                            List<string> messageAnswers = new List<string>();
                            messageAnswers.Add("Open in browser");
                            messageAnswers.Add("Cancel");

                            string messageResult = await MessagePopup.Popup("No item content available", "There is currently no full item content available, would you like to open the item in the browser?", messageAnswers);
                            if (messageResult == "Open in browser")
                            {
                                await OpenBrowser(null, true);
                            }
                        }
                        else
                        {
                            List<string> messageAnswers = new List<string>();
                            messageAnswers.Add("Ok");

                            Debug.WriteLine("There is currently no full item content available. (No Internet)");
                            await MessagePopup.Popup("No item content available", "There is currently no full item content available but it might also be your internet connection, please check your internet connection and try again.", messageAnswers);
                        }
                    }
                    else
                    {
                        RetryCount++;
                        Debug.WriteLine("Retrying to download full item content: " + RetryCount);
                        await DownloadFullItemContent(RetryCount);
                    }
                }
            }
            catch { }
        }
    }
}