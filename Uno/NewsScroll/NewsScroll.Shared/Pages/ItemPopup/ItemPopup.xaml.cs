using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static NewsScroll.Api.Api;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;

namespace NewsScroll
{
    public partial class ItemPopup : UserControl
    {
        //Itemviewer Variables
        private bool PopupIsOpen = false;

        public ItemPopup()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        //Open the popup
        public async Task OpenPopup(Items Source)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Opening item viewer...");

                if (PopupIsOpen)
                {
                    System.Diagnostics.Debug.WriteLine("The popup is already open...");
                    return;
                }

                //Open the popup
                popup_Main.IsOpen = true;
                PopupIsOpen = true;

                //Focus on the popup
                iconMenu.Focus(FocusState.Programmatic);

                //Adjust the swiping direction
                SwipeBarAdjust();

                //Check if the header is hidden
                if (AppVariables.HeaderHidden) { await HideShowHeader(true); }

                //Update the status message
                ProgressDisableUI("Loading the item...");

                //Set item source
                vCurrentWebSource = Source;
                txt_AppInfo.Text = ApiMessageError + vCurrentWebSource.feed_title;

                //Check if internet is available
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    iconItem.Visibility = Visibility.Visible;
                    iconBrowser.Visibility = Visibility.Visible;
                    button_LoadFullItem.Visibility = Visibility.Visible;
                    button_OpenInBrowser.Visibility = Visibility.Visible;
                }
                else
                {
                    iconItem.Visibility = Visibility.Collapsed;
                    iconBrowser.Visibility = Visibility.Collapsed;
                    button_LoadFullItem.Visibility = Visibility.Collapsed;
                    button_OpenInBrowser.Visibility = Visibility.Collapsed;
                }

                //Update the star status
                if (vCurrentWebSource.item_star_status == Visibility.Collapsed)
                {
                    ToolTipService.SetToolTip(iconStar, "Star item");
                    iconImageStar.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconStarAdd.png", false);
                }
                else
                {
                    ToolTipService.SetToolTip(iconStar, "Unstar item");
                    iconImageStar.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconStarRemove.png", false);
                }
                iconStar.Visibility = Visibility.Visible;

                //Load item into the viewer
                await LoadItem(string.Empty);

                //Register page events
                RegisterPageEvents();

                //Update the status message
                ProgressEnableUI();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed loading item.");
                ClosePopup();
            }
        }

        //Load item into the viewer
        private async Task LoadItem(string CustomItemContent)
        {
            try
            {
                //Load the full item
                TableItems LoadTable = await SQLConnection.Table<TableItems>().Where(x => x.item_id == vCurrentWebSource.item_id).FirstOrDefaultAsync();
                if (LoadTable != null)
                {
                    //Check if media needs to load
                    AppVariables.LoadMedia = true;
                    if (!NetworkInterface.GetIsNetworkAvailable() && !(bool)AppVariables.ApplicationSettings["DisplayImagesOffline"]) { AppVariables.LoadMedia = false; }

                    //Set the date time string
                    DateTime convertedDate = DateTime.SpecifyKind(LoadTable.item_datetime, DateTimeKind.Utc).ToLocalTime();
                    string DateAuthorString = convertedDate.ToString(AppVariables.CultureInfoFormat.LongDatePattern, AppVariables.CultureInfoFormat) + ", " + convertedDate.ToString(AppVariables.CultureInfoFormat.ShortTimePattern, AppVariables.CultureInfoFormat);

                    //Add the author to date time
                    if (!string.IsNullOrWhiteSpace(LoadTable.item_author)) { DateAuthorString += " by " + LoadTable.item_author; }
                    tb_ItemDateString.Text = DateAuthorString;

                    //Enable or disable text selection
                    if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                    {
                        tb_ItemTitle.IsTextSelectionEnabled = true;
                        tb_ItemDateString.IsTextSelectionEnabled = true;
                        rtb_ItemContent.IsTextSelectionEnabled = true;
                    }
                    else
                    {
                        tb_ItemTitle.IsTextSelectionEnabled = false;
                        tb_ItemDateString.IsTextSelectionEnabled = false;
                        rtb_ItemContent.IsTextSelectionEnabled = false;
                    }

                    //Load the item content
                    bool SetHtmlToRichTextBlock = false;
                    if (!string.IsNullOrWhiteSpace(CustomItemContent))
                    {
                        await HtmlToRichTextBlock(rtb_ItemContent, CustomItemContent, string.Empty);
                        SetHtmlToRichTextBlock = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(LoadTable.item_content_full))
                    {
                        SetHtmlToRichTextBlock = await HtmlToRichTextBlock(rtb_ItemContent, LoadTable.item_content_full, string.Empty);
                    }

                    //Check if html to xaml has failed
                    if (!SetHtmlToRichTextBlock || !rtb_ItemContent.Blocks.Any())
                    {
                        //Load summary text
                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(new Run() { Text = AVFunctions.StringCut(LoadTable.item_content, AppVariables.MaximumItemTextLength, "...") });

                        //Add paragraph to rich text block
                        rtb_ItemContent.Blocks.Clear();
                        rtb_ItemContent.Blocks.Add(paragraph);
                    }

                    //Wait for item content is loaded
                    await AppAdjust.FinishLayoutUpdateAsync(rtb_ItemContent);

                    //Check if item content contains preview image
                    await CheckItemContentContainsPreviewImage(LoadTable);

                    //Adjust the itemviewer size
                    await AdjustItemViewerSize();
                }
            }
            catch { }
        }

        //Check if item content contains preview image
        private async Task CheckItemContentContainsPreviewImage(TableItems LoadTable)
        {
            try
            {
                int ItemImagecount = 0;
                bool FoundPreviewImage = false;

                //Check the preview image
                string ItemImageLink = LoadTable.item_image;
                if (string.IsNullOrWhiteSpace(ItemImageLink))
                {
                    item_image.item_source.Source = null;
                    item_image.Visibility = Visibility.Collapsed;
                    return;
                }

                //Check if there are images and the preview image is included
                CheckTextBlockForPreviewImage(rtb_ItemContent, ItemImageLink, ref ItemImagecount, ref FoundPreviewImage);

                //Update the preview image based on result
                if (ItemImagecount == 0 || !FoundPreviewImage)
                {
                    System.Diagnostics.Debug.WriteLine("No media found in rich text block, adding item image.");

                    //Check if media is a gif(v) file
                    bool ImageIsGif = ItemImageLink.ToLower().Contains(".gif");

                    //Check if low bandwidth mode is enabled
                    if (ImageIsGif && (bool)AppVariables.ApplicationSettings["LowBandwidthMode"])
                    {
                        //System.Diagnostics.Debug.WriteLine("Low bandwidth mode skipping gif.");
                        item_image.item_status.Text = "Gif not loaded,\nlow bandwidth mode.";
                        item_image.Visibility = Visibility.Visible;
                        return;
                    }

                    if (ImageIsGif)
                    {
                        item_image.item_video_status.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconVideoPause.png", false);
                        item_image.item_video.Visibility = Visibility.Visible;
                    }

                    item_image.MaxHeight = AppVariables.MaximumItemImageHeight;
                    item_image.item_source.Source = await AVImage.LoadBitmapImage(ItemImageLink, true);
                    item_image.Visibility = Visibility.Visible;
                }
                else
                {
                    item_image.item_source.Source = null;
                    item_image.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                item_image.item_source.Source = null;
                item_image.Visibility = Visibility.Collapsed;
            }
        }

        //Check if there are images and the preview image is included
        private void CheckTextBlockForPreviewImage(DependencyObject SearchElement, string ItemImageLink, ref int ItemImagecount, ref bool FoundPreviewImage)
        {
            try
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(SearchElement); i++)
                {
                    try
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(SearchElement, i);
                        if (child.GetType() == typeof(ImageContainer))
                        {
                            ItemImagecount++;
                            ImageContainer frameworkElement = child as ImageContainer;
                            BitmapImage bitmapSource = frameworkElement.item_source.Source as BitmapImage;

                            string CompareBitmapLink = Regex.Replace(bitmapSource.UriSource.ToString(), @"^(?:http(?:s)?://)?(?:www(?:[0-9]+)?\.)?", string.Empty, RegexOptions.IgnoreCase).ToLower();
                            string CompareItemImageLink = Regex.Replace(ItemImageLink, @"^(?:http(?:s)?://)?(?:www(?:[0-9]+)?\.)?", string.Empty, RegexOptions.IgnoreCase).ToLower();
                            //System.Diagnostics.Debug.WriteLine("Comparing image: " + CompareBitmapLink + " vs " + CompareItemImageLink);

                            if (CompareBitmapLink == CompareItemImageLink)
                            {
                                FoundPreviewImage = true;
                                break;
                            }
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("No image, checking if there is a sub image.");
                            CheckTextBlockForPreviewImage(child, ItemImageLink, ref ItemImagecount, ref FoundPreviewImage);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        //Close the popup
        public void ClosePopup()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Closing the item popup...");

                //Cleanup xaml resources
                item_image.item_source.Source = null;
                rtb_ItemContent.Blocks.Clear();

                //Disable page events
                DisablePageEvents();

                //Close the popup
                popup_Main.IsOpen = false;
                PopupIsOpen = false;
            }
            catch { }
        }

        //Register page events
        private void RegisterPageEvents()
        {
            try
            {
                //Monitor user touch swipe
                if (!(bool)AppVariables.ApplicationSettings["DisableSwipeActions"])
                {
                    grid_SwipeBar.ManipulationMode = ManipulationModes.TranslateX;
                    grid_SwipeBar.ManipulationStarted += Page_ManipulationStarted;
                    grid_SwipeBar.ManipulationDelta += Page_ManipulationDelta;
                    grid_SwipeBar.ManipulationCompleted += Page_ManipulationCompleted;
                }

                EventHideShowHeader += new DelegateHideShowHeader(HideShowHeader);

                //Monitor mouse presses
                grid_Main.PointerPressed += Page_PointerReleased;

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
                //Monitor user touch swipe
                grid_Main.ManipulationStarted -= Page_ManipulationStarted;
                grid_Main.ManipulationDelta -= Page_ManipulationDelta;
                grid_Main.ManipulationCompleted -= Page_ManipulationCompleted;

                EventHideShowHeader -= new DelegateHideShowHeader(HideShowHeader);

                //Monitor mouse presses
                grid_Main.PointerPressed -= Page_PointerReleased;

                //Monitor key presses
                grid_Main.PreviewKeyUp -= Page_PreviewKeyUp; //DesktopOnly
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

                    //Adjust the itemviewer size
                    await AdjustItemViewerSize();
                }
            }
            catch { }
        }

        //Adjust the itemviewer size
        private async Task AdjustItemViewerSize()
        {
            try
            {
                //Adjust the itemviewer size
                Rect ScreenResolution = AVFunctions.AppWindowResolution();
                grid_Main.Width = ScreenResolution.Width;
                grid_Main.Height = ScreenResolution.Height;
                stackpanel_NewsItem.Width = ScreenResolution.Width;

                //Get the itemviewer elements target width
                await Task.Delay(10);
                double TargetLayoutWidth = stackpanel_NewsItem.ActualWidth - stackpanel_NewsItem.Padding.Left - stackpanel_NewsItem.Padding.Right;

                //Adjust the itemviewer elements width
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(rtb_ItemContent); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(rtb_ItemContent, i);
                    FrameworkElement frameworkElement = child as FrameworkElement;
                    object childType = child.GetType();

                    //Adjust the video and webframe size
                    if (childType == typeof(VideoContainer) || childType == typeof(WebContainer))
                    {
                        frameworkElement.Width = TargetLayoutWidth;
                        frameworkElement.Height = TargetLayoutWidth * 9 / 16;
                    }
                    //Adjust the image and gif size
                    else if (childType == typeof(ImageContainer))
                    {
                        bool ScaleImage = frameworkElement.Tag == null;
                        if (ScaleImage)
                        {
                            frameworkElement.Width = TargetLayoutWidth;
                        }
                    }
                    //Adjust other element sizes
                    else
                    {
                        frameworkElement.Width = TargetLayoutWidth;
                    }
                }
            }
            catch { }
        }

        //Item status scroll events
        private double PreviousScrollOffset = -1;
        private async void button_StatusCurrentItem_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                string ReturnToPrevious = string.Empty;
                if (PreviousScrollOffset != -1) { ReturnToPrevious = "Scroll to previous"; }

                int MsgBoxResult = await MessagePopup.Popup("View scroller", "Would you like to scroll in the itemviewer?", "Scroll to beginning", "Scroll to the middle", "Scroll to the end", ReturnToPrevious, "", true);
                if (MsgBoxResult == 1)
                {
                    await Task.Delay(10);
                    PreviousScrollOffset = scrollviewer_NewsItem.VerticalOffset;
                    scrollviewer_NewsItem.ChangeView(null, 0, null);
                }
                else if (MsgBoxResult == 2)
                {
                    await Task.Delay(10);
                    PreviousScrollOffset = scrollviewer_NewsItem.VerticalOffset;
                    scrollviewer_NewsItem.ChangeView(null, scrollviewer_NewsItem.ScrollableHeight / 2, null);
                }
                else if (MsgBoxResult == 3)
                {
                    await Task.Delay(10);
                    PreviousScrollOffset = scrollviewer_NewsItem.VerticalOffset;
                    scrollviewer_NewsItem.ChangeView(null, scrollviewer_NewsItem.ScrollableHeight, null);
                }
                else if (MsgBoxResult == 4)
                {
                    await Task.Delay(10);
                    double CurrentOffset = scrollviewer_NewsItem.VerticalOffset;
                    scrollviewer_NewsItem.ChangeView(null, PreviousScrollOffset, null);
                    PreviousScrollOffset = CurrentOffset;
                }
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
                textblock_StatusApplication.Text = ProgressMsg;
                grid_StatusApplication.Visibility = Visibility.Visible;

                //Disable content
                button_StatusCurrentItem.IsHitTestVisible = false;
                button_StatusCurrentItem.Opacity = 0.30;
                rtb_ItemContent.IsHitTestVisible = false;
                rtb_ItemContent.Opacity = 0.30;
                item_image.IsHitTestVisible = false;
                item_image.Opacity = 0.30;

                //Disable buttons
                iconStar.IsHitTestVisible = false;
                iconStar.Opacity = 0.30;
                iconItem.IsHitTestVisible = false;
                iconItem.Opacity = 0.30;
                iconBrowser.IsHitTestVisible = false;
                iconBrowser.Opacity = 0.30;
                iconShare.IsHitTestVisible = false;
                iconShare.Opacity = 0.30;
                iconBack.Opacity = 0.30;
                iconBack.IsHitTestVisible = false;
                button_LoadFullItem.Opacity = 0.30;
                button_LoadFullItem.IsHitTestVisible = false;
                button_OpenInBrowser.Opacity = 0.30;
                button_OpenInBrowser.IsHitTestVisible = false;
                button_GoBackPage.Opacity = 0.30;
                button_GoBackPage.IsHitTestVisible = false;
            }
            catch { AppVariables.BusyApplication = true; }
        }

        void ProgressEnableUI()
        {
            try
            {
                //Disable progressbar
                grid_StatusApplication.Visibility = Visibility.Collapsed;
                textblock_StatusApplication.Text = string.Empty;

                //Enable content
                button_StatusCurrentItem.IsHitTestVisible = true;
                button_StatusCurrentItem.Opacity = 1;
                rtb_ItemContent.IsHitTestVisible = true;
                rtb_ItemContent.Opacity = 1;
                item_image.IsHitTestVisible = true;
                item_image.Opacity = 1;

                //Enable buttons
                iconStar.IsHitTestVisible = true;
                iconStar.Opacity = 1;
                iconItem.IsHitTestVisible = true;
                iconItem.Opacity = 1;
                iconBrowser.IsHitTestVisible = true;
                iconBrowser.Opacity = 1;
                iconShare.IsHitTestVisible = true;
                iconShare.Opacity = 1;
                iconBack.Opacity = 1;
                iconBack.IsHitTestVisible = true;
                button_LoadFullItem.Opacity = 1;
                button_LoadFullItem.IsHitTestVisible = true;
                button_OpenInBrowser.Opacity = 1;
                button_OpenInBrowser.IsHitTestVisible = true;
                button_GoBackPage.Opacity = 1;
                button_GoBackPage.IsHitTestVisible = true;

                AppVariables.BusyApplication = false;
            }
            catch { AppVariables.BusyApplication = false; }
        }

        //User Interface - Buttons
        void iconBack_Tap(object sender, RoutedEventArgs e)
        {
            try { ClosePopup(); } catch { }
        }

        async void iconShare_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);
                ShareItem(vCurrentWebSource);
            }
            catch { }
        }

        async void iconStar_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);

                //Update the status bar
                if (vCurrentWebSource.item_star_status == Visibility.Collapsed) { ProgressDisableUI("Starring the item..."); }
                else { ProgressDisableUI("Unstarring the item..."); }

                //Get and set the current page name
                string CurrentPageName = App.vApplicationFrame.SourcePageType.ToString();

                if (CurrentPageName.EndsWith("StarredPage"))
                {
                    bool MarkedAsStar = await MarkItemAsStarPrompt(vCurrentWebSource, true, true, false, false, true);

                    //Update the header and selection feeds
                    if (MarkedAsStar) { await EventUpdateTotalItemsCount(null, null, false, true); }
                }
                else
                {
                    await MarkItemAsStarPrompt(vCurrentWebSource, true, false, false, false, true);
                }

                //Update the star status
                if (vCurrentWebSource.item_star_status == Visibility.Collapsed)
                {
                    ToolTipService.SetToolTip(iconStar, "Star item");
                    iconImageStar.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconStarAdd.png", false);
                }
                else
                {
                    ToolTipService.SetToolTip(iconStar, "Unstar item");
                    iconImageStar.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconStarRemove.png", false);
                }

                ProgressEnableUI();
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

        async void iconBrowserMenu_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);
                await OpenBrowser(null, true);
            }
            catch { }
        }

        async void iconBrowserItem_Tap(object sender, RoutedEventArgs e)
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
                int MsgBoxResult = 0;

                //Check internet connection
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    await MessagePopup.Popup("No internet connection", "You currently don't have an internet connection available to open this item or link in the webviewer or your webbrowser.", "Ok", "", "", "", "", false);
                    return;
                }

                //Check webbrowser only links
                if (TargetUri != null)
                {
                    string TargetString = TargetUri.ToString();
                    if (!TargetString.StartsWith("http")) { MsgBoxResult = 2; }
                }

                if (MsgBoxResult != 2)
                {
                    string LowMemoryWarning = string.Empty;
                    if (AVFunctions.DevMemoryAvailableMB() < 200) { LowMemoryWarning = "\n\n* Your device is currently low on available memory and may cause issues when you open this link or item in the webviewer."; }
                    MsgBoxResult = await MessagePopup.Popup("Open this item or link", "Do you want to open this item or link in the webviewer or your webbrowser?" + LowMemoryWarning, "Webviewer (In-app)", "Webbrowser (Device)", "", "", "", true);
                }

                if (MsgBoxResult == 1)
                {
                    if (closePopup) { ClosePopup(); }
                    //Open item in webviewer
                    WebViewer webViewer = new WebViewer();
                    await webViewer.OpenPopup(TargetUri, vCurrentWebSource);
                }
                else if (MsgBoxResult == 2)
                {
                    if (closePopup) { ClosePopup(); }
                    //Open item in webbrowser
                    if (TargetUri != null)
                    {
                        await Launcher.LaunchUriAsync(TargetUri);
                    }
                    else
                    {
                        await Launcher.LaunchUriAsync(new Uri(vCurrentWebSource.item_link, UriKind.RelativeOrAbsolute));
                    }
                }
            }
            catch { }
        }

        //Download and load the full item
        private async void LoadFullItem_Tap(object sender, RoutedEventArgs e)
        {
            try { await LoadFullItem(); } catch { }
        }

        private async void iconItem_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);
                await LoadFullItem();
            }
            catch { }
        }

        private async Task LoadFullItem()
        {
            try
            {
                if (button_LoadFullItem.Content.ToString() == "Load the original item")
                {
                    ProgressDisableUI("Loading the original item...");

                    //Load the original item content
                    string OriginalContent = vCurrentWebSource.item_content_full;

                    //Load item into the viewer
                    await LoadItem(OriginalContent);

                    //Update the button text
                    button_LoadFullItem.Content = "Load the full item";
                    ToolTipService.SetToolTip(iconItem, "Load the full item");
                    iconImageItem.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconItemFull.png", false);
                    PreviousScrollOffset = -1;

                    //Scroll to the top
                    await Task.Delay(10);
                    scrollviewer_NewsItem.ChangeView(null, 0, null);
                }
                else
                {
                    //Check if internet is available
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        ProgressDisableUI("Loading the full item...");

                        int RetryCount = 0;
                        await DownloadFullItemContent(RetryCount);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No network available to fully load this item.");
                        await MessagePopup.Popup("No internet connection", "You currently don't have an internet connection available to fully load this item.", "Ok", "", "", "", "", false);
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
                string FullContent = await WebParser(vCurrentWebSource.item_link, false, true);
                if (!string.IsNullOrWhiteSpace(FullContent))
                {
                    //Load item into the viewer
                    await LoadItem(FullContent);

                    //Update the button text
                    button_LoadFullItem.Content = "Load the original item";
                    ToolTipService.SetToolTip(iconItem, "Load the original item");
                    iconImageItem.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconItemSumm.png", false);
                    PreviousScrollOffset = -1;

                    //Scroll to the top
                    await Task.Delay(10);
                    scrollviewer_NewsItem.ChangeView(null, 0, null);
                }
                else
                {
                    if (RetryCount == 3)
                    {
                        //Check if internet is available
                        if (NetworkInterface.GetIsNetworkAvailable())
                        {
                            System.Diagnostics.Debug.WriteLine("There is currently no full item content available.");
                            int MsgBoxResult = await MessagePopup.Popup("No item content available", "There is currently no full item content available, would you like to open the item in the browser?", "Open in browser", "", "", "", "", true);
                            if (MsgBoxResult == 1)
                            {
                                await OpenBrowser(null, true);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("There is currently no full item content available. (No Internet)");
                            await MessagePopup.Popup("No item content available", "There is currently no full item content available but it might also be your internet connection, please check your internet connection and try again.", "Ok", "", "", "", "", false);
                        }
                    }
                    else
                    {
                        RetryCount++;
                        System.Diagnostics.Debug.WriteLine("Retrying to download full item content: " + RetryCount);
                        await DownloadFullItemContent(RetryCount);
                    }
                }
            }
            catch { }
        }

        private void webview_Window_ContainsFullScreenElementChanged(WebView sender, object args)
        {
            try
            {
                //Get the fullscreen webview uri
                WebView sendview_Browser = sender as WebView;
                if (sendview_Browser.Visibility == Visibility.Visible)
                {
                    System.Diagnostics.Debug.WriteLine("Switching to windowed webview.");

                    //Load the url in windowed webviewer
                    vWindowWebview.Source = sendview_Browser.Source;
                    sendview_Browser.Visibility = Visibility.Collapsed;

                    //Unload the webviewer
                    sendview_Browser.Stop();
                    sendview_Browser.NavigateToString(string.Empty);
                    sendview_Browser = null;

                    //Set Landscape Display
                    if ((bool)AppVariables.ApplicationSettings["DisableLandscapeDisplay"])
                    {
                        DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                    }
                }
            }
            catch { }
        }

        private async void webview_Full_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            try
            {
                args.Handled = true;
                System.Diagnostics.Debug.WriteLine("Preventing new web window: " + args.Uri.AbsolutePath.ToString());
                await OpenBrowser(args.Uri, false);
            }
            catch { }
        }

        private void webview_Full_ContainsFullScreenElementChanged(WebView sender, object args)
        {
            try
            {
                if (webview_Fullscreen.Visibility == Visibility.Collapsed)
                {
                    System.Diagnostics.Debug.WriteLine("Switching to fullscreen webview.");

                    //Get the windowed current webview uri
                    WebView sendview_Browser = sender as WebView;
                    vWindowWebview = sendview_Browser;

                    //Load the url in fullscreen webviewer
                    webview_Fullscreen.Source = sendview_Browser.Source;
                    webview_Fullscreen.Visibility = Visibility.Visible;

                    //Unload the webviewer
                    sendview_Browser.Stop();
                    sendview_Browser.NavigateToString(string.Empty);
                    sendview_Browser = null;

                    //Set Landscape Display
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape;
                }
            }
            catch { }
        }
    }
}