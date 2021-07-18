using ArnoldVinkCode;
using ArnoldVinkMessageBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using static NewsScroll.Startup.Startup;

namespace NewsScroll
{
    public partial class ImageViewer : UserControl
    {
        //Popup Variables
        private static BitmapImage vBitmapImage = null;
        private bool PopupIsOpen = false;

        //Initialize popup
        public ImageViewer() { this.InitializeComponent(); }

        //Open the popup
        public async Task OpenPopup(string ImageLink)
        {
            try
            {
                if (PopupIsOpen)
                {
                    System.Diagnostics.Debug.WriteLine("The popup is already open...");
                    return;
                }

                //Open the popup
                popup_Main.IsOpen = true;
                PopupIsOpen = true;

                //Focus on the popup
                iconClose.Focus(FocusState.Programmatic);

                //Adjust the swiping direction
                SwipeBarAdjust();

                //Show the switch fullscreen mode icon
                if (!AVFunctions.DevMobile()) { iconScreenMode.Visibility = Visibility.Visible; }

                //Set Landscape Display
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape;

                //Load the image source
                vBitmapImage = await AVImage.LoadBitmapImage(ImageLink, true);
                if (vBitmapImage != null)
                {
                    //Set the image to viewer
                    image_source.Source = vBitmapImage;
                    image_status.Visibility = Visibility.Visible;

                    //Check if media is a gif(v) file
                    bool ImageIsGif = ImageLink.ToLower().Contains(".gif");
                    if (ImageIsGif) { item_video.Visibility = Visibility.Visible; }
                }
                else
                {
                    image_status.Text = "Image failed to load.";
                    image_source.Source = null;
                    image_status.Visibility = Visibility.Visible;
                }

                //Register page events
                RegisterPageEvents();
            }
            catch { }
        }

        //Close the popup
        private void iconClose_Tap(object sender, RoutedEventArgs e) { try { ClosePopup(); } catch { } }
        public void ClosePopup()
        {
            try
            {
                //Reset the image resources
                vBitmapImage = null;
                image_source.Source = null;

                //Disable page events
                DisablePageEvents();

                //Set Landscape Display
                if ((bool)AppVariables.ApplicationSettings["DisableLandscapeDisplay"])
                {
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                }

                //Close the popup
                popup_Main.IsOpen = false;
                PopupIsOpen = false;
            }
            catch { }
        }

        //Hide and show menu
        private async void scrollviewer_ImageViewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                AppVariables.SingleTappedEvent = true;
                await Task.Delay(300);
                if (AppVariables.SingleTappedEvent)
                {
                    if (grid_Header.Visibility == Visibility.Visible) { grid_Header.Visibility = Visibility.Collapsed; }
                    else { grid_Header.Visibility = Visibility.Visible; }
                }
            }
            catch { }
        }

        //Zoom in and out
        private void scrollviewer_ImageViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                AppVariables.SingleTappedEvent = false;
                if (scrollviewer_ImageViewer.ZoomFactor == 1.0f)
                {
                    System.Diagnostics.Debug.WriteLine("Zooming the scrollviewer");
                    Point currentPoint = e.GetPosition(scrollviewer_ImageViewer);
                    scrollviewer_ImageViewer.ChangeView((currentPoint.X * 0.5), null, 1.5f, true);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Resetting the scrollviewer");
                    scrollviewer_ImageViewer.ChangeView(null, null, 1.0f, true);
                }
            }
            catch { }
        }

        //Switch between screen modes
        private void button_SwitchScreenMode_Tap(object sender, RoutedEventArgs e)
        {
            try { SwitchScreenMode(); } catch { }
        }

        //Play/pause the video
        private async void item_video_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapImage bitmapSource = image_source.Source as BitmapImage;
                if (bitmapSource.IsPlaying)
                {
                    ToolTipService.SetToolTip(item_video, "Play the video");
                    item_video_status.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconVideoPlay.png", false);
                    bitmapSource.Stop();
                }
                else
                {
                    ToolTipService.SetToolTip(item_video, "Pause the video");
                    item_video_status.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconVideoPause.png", false);
                    bitmapSource.Play();
                }
            }
            catch { }
        }

        //Save the image
        private async void button_iconSave_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                if (vBitmapImage != null && vBitmapImage.PixelHeight > 0)
                {
                    //Get the url path
                    string UrlPath = string.Empty;
                    try { UrlPath = vBitmapImage.UriSource.AbsolutePath; } catch { }
                    if (string.IsNullOrWhiteSpace(UrlPath)) { UrlPath = vBitmapImage.UriSource.ToString(); }

                    //Get the file name
                    string FileName = "Unknown";
                    try { FileName = Path.GetFileNameWithoutExtension(UrlPath); } catch { }
                    if (string.IsNullOrWhiteSpace(FileName)) { FileName = "Unknown"; }

                    //Check if network is available
                    bool IsNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();

                    //Get the file extension
                    string FileExtensionFile = ".jpg";
                    string FileExtensionDisplay = "JPG";
                    if (IsNetworkAvailable)
                    {
                        try { FileExtensionFile = Path.GetExtension(UrlPath).ToLower(); } catch { }
                        if (string.IsNullOrWhiteSpace(FileExtensionFile)) { FileExtensionFile = ".jpg"; }
                        FileExtensionDisplay = FileExtensionFile.ToUpper().Replace(".", string.Empty);
                    }
                    else
                    {
                        int MessageResult = await AVMessageBox.Popup("Offline saving", "Saving images while in offline mode may save the image in a lower quality and animations will be saved as a static image.", "Save image", "", "", "", "", true);
                        if (MessageResult == 0) { return; }
                    }

                    FileSavePicker FileSavePicker = new FileSavePicker();
                    FileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                    FileSavePicker.FileTypeChoices.Add(FileExtensionDisplay, new List<string>() { FileExtensionFile });
                    FileSavePicker.SuggestedFileName = FileName;

                    StorageFile NewFile = await FileSavePicker.PickSaveFileAsync();
                    if (NewFile != null)
                    {
                        //Download the bitmapimage source uri
                        if (IsNetworkAvailable)
                        {
                            System.Diagnostics.Debug.WriteLine("Saving online image...");

                            byte[] ImageBuffer = await AVDownloader.DownloadByteAsync(10000, "News Scroll", null, vBitmapImage.UriSource);
                            if (ImageBuffer != null) { await FileIO.WriteBytesAsync(NewFile, ImageBuffer); }
                            else
                            {
                                await AVMessageBox.Popup("Failed to save", "Failed to save the image, please check your internet connection and try again.", "Ok", "", "", "", "", false);
                                System.Diagnostics.Debug.WriteLine("Failed to download the image.");
                            }
                        }
                        //Capture the image displayed in xaml
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Saving offline image...");

                            ////Set the image size temporarily to bitmap size
                            //Double PreviousWidth = image_ImageViewer.ActualWidth;
                            //Double PreviousHeight = image_ImageViewer.ActualHeight;
                            //image_ImageViewer.Width = vBitmapImage.PixelWidth;
                            //image_ImageViewer.Height = vBitmapImage.PixelHeight;

                            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
                            await renderTargetBitmap.RenderAsync(image_source);
                            IBuffer PixelBuffer = await renderTargetBitmap.GetPixelsAsync();

                            using (IRandomAccessStream fileStream = await NewFile.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                BitmapEncoder bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);
                                bitmapEncoder.SetPixelData(
                                    BitmapPixelFormat.Bgra8,
                                    BitmapAlphaMode.Ignore,
                                    (uint)renderTargetBitmap.PixelWidth,
                                    (uint)renderTargetBitmap.PixelHeight,
                                    DisplayInformation.GetForCurrentView().LogicalDpi,
                                    DisplayInformation.GetForCurrentView().LogicalDpi,
                                    PixelBuffer.ToArray());
                                await bitmapEncoder.FlushAsync();
                            }

                            //System.Diagnostics.Debug.WriteLine("From: " + image_ImageViewer.Width + " setting back: " + PreviousWidth);
                            //image_ImageViewer.Width = PreviousWidth;
                            //image_ImageViewer.Height = PreviousHeight;
                        }
                    }
                }
            }
            catch
            {
                await AVMessageBox.Popup("Failed to save", "Failed to save the image, please check your internet connection and try again.", "Ok", "", "", "", "", false);
                System.Diagnostics.Debug.WriteLine("Failed to save the image.");
            }
        }

        //Register page events
        private void RegisterPageEvents()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Registering page events...");

                //Monitor user touch swipe
                if (!(bool)AppVariables.ApplicationSettings["DisableSwipeActions"])
                {
                    grid_SwipeBar.ManipulationMode = ManipulationModes.TranslateX;
                    grid_SwipeBar.ManipulationStarted += Page_ManipulationStarted;
                    grid_SwipeBar.ManipulationDelta += Page_ManipulationDelta;
                    grid_SwipeBar.ManipulationCompleted += Page_ManipulationCompleted;
                }

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

                    image_source.MaxWidth = NewLayoutWidth;
                    image_source.MaxHeight = NewLayoutHeight;
                }
            }
            catch { }
        }

        private void Image_source_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Image viewer opened, hiding the status.");
                image_status.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        private void Image_source_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                image_status.Text = "Image failed to load.";
                image_source.Source = null;
                image_status.Visibility = Visibility.Visible;
            }
            catch { }
        }
    }
}