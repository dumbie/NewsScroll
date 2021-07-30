using ArnoldVinkCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using static NewsScroll.Events.Events;

namespace NewsScroll
{
    public partial class ImagePopup : UserControl
    {
        //Popup Variables
        private static BitmapImage vBitmapImage = null;
        private static string vBitmapLink = string.Empty;

        //Initialize popup
        public ImagePopup() { this.InitializeComponent(); }

        //Open the popup
        public async Task OpenPopup(string ImageLink)
        {
            try
            {
                //Open the popup
                popup_Main.IsOpen = true;

                //Focus on the popup
                iconClose.Focus(FocusState.Programmatic);

                //Adjust the swiping direction
                SwipeBarAdjust();

                //Set Landscape Display
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape;

                //Load the image source
                vBitmapLink = ImageLink;
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
                if (!string.IsNullOrWhiteSpace(vBitmapLink))
                {
                    //Check internet connection
                    if (!AppVariables.InternetAccess)
                    {
                        await new MessagePopup().OpenPopup("Failed to save", "Failed to save the image, please check your internet connection and try again.", "Ok", "", "", "", "", false);
                        System.Diagnostics.Debug.WriteLine("Failed to download the image, no internet access.");
                        return;
                    }

                    //Get the file name
                    string FileName = "Unknown";
                    try
                    {
                        FileName = Path.GetFileNameWithoutExtension(vBitmapLink);
                    }
                    catch { }
                    if (string.IsNullOrWhiteSpace(FileName)) { FileName = "Unknown"; }

                    //Get the file extension
                    string FileExtensionFile = ".jpg";
                    string FileExtensionDisplay = "JPG";
                    try
                    {
                        FileExtensionFile = Path.GetExtension(vBitmapLink).ToLower();
                    }
                    catch { }
                    if (string.IsNullOrWhiteSpace(FileExtensionFile)) { FileExtensionFile = ".jpg"; }
                    FileExtensionDisplay = FileExtensionFile.ToUpper().Replace(".", string.Empty);

                    FileSavePicker saveFilePicker = new FileSavePicker();
                    saveFilePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                    saveFilePicker.FileTypeChoices.Add(FileExtensionDisplay, new List<string>() { FileExtensionFile });
                    saveFilePicker.SuggestedFileName = FileName;

                    StorageFile saveStorageFile = await saveFilePicker.PickSaveFileAsync();
                    if (saveStorageFile != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Downloading and saving online image: " + vBitmapLink);
                        await EventProgressDisableUI("Downloading and saving image...", false);

                        Uri imageUri = new Uri(vBitmapLink);
                        byte[] ImageBuffer = await AVDownloader.DownloadByteAsync(10000, "News Scroll", null, imageUri);
                        if (ImageBuffer != null)
                        {
                            await FileIO.WriteBytesAsync(saveStorageFile, ImageBuffer);
                        }
                        else
                        {
                            await new MessagePopup().OpenPopup("Failed to save", "Failed to save the image, please check your internet connection and try again.", "Ok", "", "", "", "", false);
                            System.Diagnostics.Debug.WriteLine("Failed to download the image, no internet access.");
                        }

                        await EventProgressEnableUI();
                    }
                }
            }
            catch (Exception ex)
            {
                await new MessagePopup().OpenPopup("Failed to save", "Failed to save the image, please check your internet connection and try again.", "Ok", "", "", "", "", false);
                System.Diagnostics.Debug.WriteLine("Failed to save the image: " + ex.Message);
                await EventProgressEnableUI();
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