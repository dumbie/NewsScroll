using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static NewsScroll.Api.Api;
using static NewsScroll.Events.Events;

namespace NewsScroll
{
    public partial class WebViewer : UserControl
    {
        //Webviewer Variables
        private Items vCurrentWebSource = null;
        private bool PopupIsOpen = false;

        public WebViewer() { this.InitializeComponent(); }

        //Open the popup
        public async Task OpenPopup(Uri targetUri, Items webSource)
        {
            try
            {
                if (PopupIsOpen)
                {
                    System.Diagnostics.Debug.WriteLine("The popup is already open...");
                    return;
                }

                //Check if internet is available
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    vCurrentWebSource = webSource;
                    txt_AppInfo.Text = webSource.feed_title;

                    //Browse to the uri
                    if (targetUri != null) { webview_Browser.Navigate(targetUri); }
                    else { webview_Browser.Navigate(new Uri(webSource.item_link)); }

                    //Open the popup
                    popup_Main.IsOpen = true;
                    PopupIsOpen = true;

                    //Focus on the popup
                    iconMenu.Focus(FocusState.Programmatic);

                    //Adjust the swiping direction
                    SwipeBarAdjust();

                    //Check if the header is hidden
                    if (AppVariables.HeaderHidden) { await HideShowHeader(true); }

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

                    //Register page events
                    RegisterPageEvents();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("There is no internet connection available...");
                    ClosePopup();
                    return;
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed loading web page.");
                ClosePopup();
            }
        }

        //Close the popup
        public void ClosePopup()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Closing the webbrowser...");

                //Unload the webviewer
                webview_Browser.Stop();
                webview_Browser.NavigateToString(string.Empty);
                webview_Browser = null;

                //Disable page events
                DisablePageEvents();

                //Close the popup
                popup_Main.IsOpen = false;
                PopupIsOpen = false;
            }
            catch { }
        }

        //Handle go back button
        private async Task GoBack()
        {
            try
            {
                if (webview_Browser.CanGoBack)
                {
                    await HideShowMenu(true);
                    int MsgBoxResult = await MessagePopup.Popup("Webviewer", "Do you want to go to the previous page or close the browser?", "Go to previous page", "Close webviewer", "", "", "", true);
                    if (MsgBoxResult == 1) { webview_Browser.GoBack(); }
                    else if (MsgBoxResult == 2) { ClosePopup(); }
                }
                else
                {
                    ClosePopup();
                }
            }
            catch { }
        }

        private void NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            try
            {
                if (args.Uri != null && !string.IsNullOrWhiteSpace(args.Uri.ToString()))
                {
                    System.Diagnostics.Debug.WriteLine("Loading: " + args.Uri);
                    if (!AppVariables.BusyApplication) { ProgressDisableUI("Loading: " + args.Uri); }
                }
            }
            catch { }
        }

        private void OnDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Page content loaded.");
                ProgressEnableUI();
            }
            catch { }
        }

        //Prevent new window opening
        private void OnNewWindowRequested(object sender, WebViewNewWindowRequestedEventArgs e)
        {
            try
            {
                e.Handled = true;
                System.Diagnostics.Debug.WriteLine("Preventing new web window.");
                webview_Browser.Navigate(e.Uri);
            }
            catch { }
        }

        //Switch to fullscreen mode
        private async void ContainsFullScreenElementChanged(WebView sender, object args)
        {
            try
            {
                if (sender.ContainsFullScreenElement)
                {
                    System.Diagnostics.Debug.WriteLine("Switching to fullscreen webview.");
                    grid_PopupButton.Visibility = Visibility.Collapsed;
                    await HideShowHeader(true);

                    //Set Landscape Display
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Switching to windowed webview.");
                    grid_PopupButton.Visibility = Visibility.Visible;

                    //Set Landscape Display
                    if ((bool)AppVariables.ApplicationSettings["DisableLandscapeDisplay"])
                    {
                        DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                    }
                }
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
        private void OnLayoutUpdated(object sender, object e)
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

                    grid_Main.Width = NewLayoutWidth;
                    grid_Main.Height = NewLayoutHeight;
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

                //Disable buttons
                iconStar.IsHitTestVisible = false;
                iconStar.Opacity = 0.30;
                iconShare.IsHitTestVisible = false;
                iconShare.Opacity = 0.30;
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

                //Enable buttons
                iconStar.IsHitTestVisible = true;
                iconStar.Opacity = 1;
                iconShare.IsHitTestVisible = true;
                iconShare.Opacity = 1;

                AppVariables.BusyApplication = false;
            }
            catch { AppVariables.BusyApplication = false; }
        }

        //User Interface - Buttons
        async void iconBack_Tap(object sender, RoutedEventArgs e)
        {
            try { await GoBack(); } catch { }
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

        async void iconBrowser_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                await HideShowMenu(true);
                int MsgBoxResult = await MessagePopup.Popup("Open this item or link", "Do you want to open this item or link in your webbrowser?", "Open in webbrowser", "", "", "", "", true);
                if (MsgBoxResult == 1)
                {
                    ClosePopup();
                    //Open item in webbrowser
                    await Launcher.LaunchUriAsync(new Uri(vCurrentWebSource.item_link, UriKind.RelativeOrAbsolute));
                }
            }
            catch { }
        }
    }
}