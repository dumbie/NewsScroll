﻿using ArnoldVinkCode;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NewsScroll.MainElements
{
    public partial class grid_item_image
    {
        //Property item_image
        private static readonly DependencyProperty property_item_image = DependencyProperty.Register("value_item_image", typeof(BitmapImage), typeof(grid_item_image), new PropertyMetadata(null, propertycallback_item_image));
        public BitmapImage value_item_image
        {
            get { return (BitmapImage)GetValue(property_item_image); }
            set { SetValue(property_item_image, value); }
        }
        private static void propertycallback_item_image(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                grid_item_image sender = dependencyObject as grid_item_image;
                if (sender != null) { sender.propertychanged_item_image(args); }
            }
            catch { }
        }
        private async void propertychanged_item_image(DependencyPropertyChangedEventArgs args)
        {
            try
            {
                //Check if image is available
                if (args.NewValue == null)
                {
                    item_status.Text = "Image is not available,\nopen item in browser to view it.";
                    item_status.Visibility = Visibility.Visible;
                    item_source.Source = null;
                    return;
                }

                //Debug.WriteLine("Image source updating...");
                BitmapImage BitmapImage = (BitmapImage)args.NewValue;

                //Check if media is a gif(v) file
                string ImageLink = BitmapImage.UriSource.ToString();
                bool ImageIsGif = ImageLink.ToLower().Contains(".gif");

                //Check if low bandwidth mode is enabled
                if (ImageIsGif && (bool)AppVariables.ApplicationSettings["LowBandwidthMode"])
                {
                    item_status.Text = "Gif not loaded,\nlow bandwidth mode.";
                    item_status.Visibility = Visibility.Visible;
                    item_source.Source = null;
                }
                else
                {
                    item_status.Text = "Image loading,\nor is not available.";
                    item_status.Visibility = Visibility.Visible;
                    item_source.Source = BitmapImage;
                    BitmapImage.DownloadProgress += bitmapimage_DownloadProgress;
                }

                //Display or hide the video icon
                if (ImageIsGif)
                {
                    //Debug.WriteLine("Image is animated visibility updating...");
                    if (!BitmapImage.AutoPlay)
                    {
                        ToolTipService.SetToolTip(item_video, "Play the video");
                        item_video_status.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconVideoPlay.png", false);
                    }
                    else
                    {
                        ToolTipService.SetToolTip(item_video, "Pause the video");
                        item_video_status.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconVideoPause.png", false);
                    }
                    item_video.Visibility = Visibility.Visible;
                }
                else
                {
                    //Debug.WriteLine("Image is static visibility updating...");
                    item_video.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        //Initialize framework element
        public grid_item_image()
        {
            this.InitializeComponent();
        }

        private void item_source_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 1);
                item_border.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]);
            }
            catch { }
        }

        private void item_source_PointerLostExited(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 2);
                item_border.BorderBrush = new SolidColorBrush(Colors.Transparent);
            }
            catch { }
        }

        public void bitmapimage_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            try
            {
                item_status.Text = "Image loading: " + e.Progress + "%\nor is not available.";
                item_status.Visibility = Visibility.Visible;
            }
            catch { }
        }

        private void bitmapimage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                //BitmapImage bitmapSource = item_source.Source as BitmapImage;
                //Debug.WriteLine("Image list failed, showing the status:" + bitmapSource.UriSource.ToString());
                item_status.Text = "Image failed to load.";
                item_status.Visibility = Visibility.Visible;
                item_source.Source = null;
            }
            catch { }
        }

        private void bitmapimage_ImageOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                if (item_source.Source.GetType() == typeof(BitmapImage))
                {
                    BitmapImage bitmapSource = item_source.Source as BitmapImage;

                    //Check the loaded image display size
                    int PixelWidth = bitmapSource.PixelWidth;
                    int PixelHeight = bitmapSource.PixelHeight;
                    bool ScaleImage = this.Tag == null;
                    if (PixelHeight != 0 && PixelWidth != 0)
                    {
                        if (PixelHeight <= 10 && PixelWidth <= 10)
                        {
                            Debug.WriteLine("Image is below 10px: " + PixelHeight + "h" + "/" + PixelWidth + "w");
                            item_status.Text = "Image is too small.";
                            item_status.Visibility = Visibility.Visible;
                            item_video.Visibility = Visibility.Collapsed;
                            item_border.Visibility = Visibility.Collapsed;
                            return;
                        }
                        else if (ScaleImage && PixelHeight <= 120 && PixelWidth <= 120)
                        {
                            Debug.WriteLine("Image is below 120px: " + PixelHeight + "h" + "/" + PixelWidth + "w " + bitmapSource.UriSource);
                            this.Tag = "NoScale";
                            this.Width = PixelWidth;
                            this.Height = PixelHeight;
                            item_source.Width = PixelWidth;
                            item_source.Height = PixelHeight;
                            item_video.Visibility = Visibility.Collapsed;
                            item_description.Visibility = Visibility.Collapsed;
                        }
                    }
                }

                //Debug.WriteLine("Image list opened, hiding the status.");
                item_status.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        private async void item_video_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapImage bitmapSource = item_source.Source as BitmapImage;
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

        private async void item_source_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                string ImageLink = string.Empty;
                if (item_source.Source.GetType() == typeof(BitmapImage))
                {
                    BitmapImage bitmapSource = item_source.Source as BitmapImage;
                    ImageLink = bitmapSource.UriSource.ToString();

                    //Check if media is a gif(v) file
                    if (ImageLink.ToLower().Contains(".gif"))
                    {
                        ToolTipService.SetToolTip(item_video, "Play the video");
                        item_video_status.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconVideoPlay.png", false);
                        bitmapSource.Stop();
                    }
                }
                else if (item_source.Source.GetType() == typeof(SvgImageSource))
                {
                    SvgImageSource SvgSource = item_source.Source as SvgImageSource;
                    ImageLink = SvgSource.UriSource.ToString();
                }

                Debug.WriteLine("Itemviewer image tapped: " + ImageLink);
                ImageViewer imageViewer = new ImageViewer();
                await imageViewer.OpenPopup(ImageLink);
            }
            catch { }
        }
    }
}