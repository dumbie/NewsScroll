using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static NewsScroll.Api.Api;
using static NewsScroll.Events.Events;

namespace NewsScroll
{
    public partial class ItemPopup : UserControl
    {
        //Popup Variables
        private Items vCurrentItem = null;

        //Initialize popup
        public ItemPopup() { this.InitializeComponent(); }

        //Open the popup
        public async Task OpenPopup(Items Source)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Opening item viewer...");

                //Open the popup
                popup_Main.IsOpen = true;

                //Focus on the popup
                iconMenu.Focus(FocusState.Programmatic);

                //Adjust the swiping direction
                SwipeBarAdjust();

                //Check if the header is hidden
                if (AppVariables.HeaderHidden) { await HideShowHeader(true); }

                //Update the status message
                ProgressDisableUI("Loading the item...");

                //Set item source
                vCurrentItem = Source;
                txt_AppInfo.Text = ApiMessageError + vCurrentItem.feed_title;

                //Check if internet is available
                if (AppVariables.InternetAccess)
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
                if (vCurrentItem.item_star_status == Visibility.Collapsed)
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

        //Close the popup
        public void ClosePopup()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Closing the item popup...");

                //Cleanup xaml resources
                popup_ItemImage.item_source.Source = null;
                popup_ItemContent.Children.Clear();

                //Disable page events
                DisablePageEvents();

                //Close the popup
                popup_Main.IsOpen = false;
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

        //Item status scroll events
        private double PreviousScrollOffset = -1;
        private async void button_StatusCurrentItem_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                string ReturnToPrevious = string.Empty;
                if (PreviousScrollOffset != -1) { ReturnToPrevious = "Scroll to previous"; }

                int MsgBoxResult = await new MessagePopup().OpenPopup("View scroller", "Would you like to scroll in the itemviewer?", "Scroll to beginning", "Scroll to the middle", "Scroll to the end", ReturnToPrevious, "", true);
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
                popup_ItemContent.IsHitTestVisible = false;
                popup_ItemContent.Opacity = 0.30;
                popup_ItemImage.IsHitTestVisible = false;
                popup_ItemImage.Opacity = 0.30;

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
                popup_ItemContent.IsHitTestVisible = true;
                popup_ItemContent.Opacity = 1;
                popup_ItemImage.IsHitTestVisible = true;
                popup_ItemImage.Opacity = 1;

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
                ShareItem(vCurrentItem);
            }
            catch { }
        }

        async void iconStar_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);

                //Update the status bar
                if (vCurrentItem.item_star_status == Visibility.Collapsed) { ProgressDisableUI("Starring the item..."); }
                else { ProgressDisableUI("Unstarring the item..."); }

                //Get and set the current page name
                string CurrentPageName = App.vApplicationFrame.SourcePageType.ToString();

                if (CurrentPageName.EndsWith("StarredPage"))
                {
                    bool MarkedAsStar = await MarkItemAsStarPrompt(vCurrentItem, true, true, false, false, true);

                    //Update the header and selection feeds
                    if (MarkedAsStar) { await EventUpdateTotalItemsCount(null, null, false, true); }
                }
                else
                {
                    await MarkItemAsStarPrompt(vCurrentItem, true, false, false, false, true);
                }

                //Update the star status
                if (vCurrentItem.item_star_status == Visibility.Collapsed)
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
                await personalizePopup.Popup();
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
                //Check internet connection
                if (!AppVariables.InternetAccess)
                {
                    await new MessagePopup().OpenPopup("No internet connection", "You currently don't have an internet connection available to open this item or link in your webbrowser.", "Ok", "", "", "", "", false);
                    return;
                }

                //Close the popup
                if (closePopup)
                {
                    ClosePopup();
                }

                //Open item in webbrowser
                if (TargetUri != null)
                {
                    await Launcher.LaunchUriAsync(TargetUri);
                }
                else
                {
                    await Launcher.LaunchUriAsync(new Uri(vCurrentItem.item_link, UriKind.RelativeOrAbsolute));
                }
            }
            catch { }
        }

        //Download and load the full item
        private async void LoadFullItem_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadFullItem();
            }
            catch { }
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
                    string OriginalContent = vCurrentItem.item_content_full;

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
                    if (AppVariables.InternetAccess)
                    {
                        ProgressDisableUI("Loading the full item...");

                        int RetryCount = 0;
                        await DownloadFullItemContent(RetryCount);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No network available to fully load this item.");
                        await new MessagePopup().OpenPopup("No internet connection", "You currently don't have an internet connection available to fully load this item.", "Ok", "", "", "", "", false);
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
                string FullContent = await WebParser(vCurrentItem.item_link, false, true);
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
                        if (AppVariables.InternetAccess)
                        {
                            System.Diagnostics.Debug.WriteLine("There is currently no full item content available.");
                            int MsgBoxResult = await new MessagePopup().OpenPopup("No item content available", "There is currently no full item content available, would you like to open the item in the browser?", "Open in browser", "", "", "", "", true);
                            if (MsgBoxResult == 1)
                            {
                                await OpenBrowser(null, true);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("There is currently no full item content available. (No Internet)");
                            await new MessagePopup().OpenPopup("No item content available", "There is currently no full item content available but it might also be your internet connection, please check your internet connection and try again.", "Ok", "", "", "", "", false);
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
    }
}