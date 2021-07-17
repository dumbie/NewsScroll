using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppVariables;

namespace NewsScroll
{
    public partial class ImageContainer
    {
        //Initialize
        public ImageContainer()
        {
            InitializeComponent();
        }

        //Set image
        public async Task SetImageLink(string imageLink)
        {
            try
            {
                //Check if image is available
                if (string.IsNullOrWhiteSpace(imageLink))
                {
                    item_status.Text = "Image is not available,\nopen item in browser to view it.";
                    item_status.IsVisible = false;
                    item_source.Source = null;
                    return;
                }

                //Check if media is a gif(v) file
                //string ImageLink = ImageSource.UriSource.ToString();
                bool imageIsGif = imageLink.ToLower().Contains(".gif");

                //Check if low bandwidth mode is enabled
                if (imageIsGif && (bool)AppSettingLoad("LowBandwidthMode"))
                {
                    item_status.Text = "Gif not loaded,\nlow bandwidth mode.";
                    item_status.IsVisible = true;
                    item_source.Source = null;
                    return;
                }

                //Set image status
                item_status.Text = "Image loading,\nor is not available.";
                item_status.IsVisible = true;

                //Set image source
                Uri imageUri = new Uri(imageLink);
                Stream imageStream = await dependencyAVImages.DownloadResizeImage(imageUri, 1024, 1024);
                item_source.Source = ImageSource.FromStream(() => imageStream);
                //ImageSource.DownloadProgress += ImageSource_DownloadProgress;

                //Set image status
                item_status.IsVisible = false;

                //Display or hide the video icon
                if (imageIsGif)
                {
                    if (item_source.IsAnimationPlaying)
                    {
                        item_video.Source = ImageSource.FromResource("NewsScroll.Assets.iconVideoPause.png");
                    }
                    else
                    {
                        item_video.Source = ImageSource.FromResource("NewsScroll.Assets.iconVideoPlay.png");
                    }
                    item_video.IsVisible = true;
                }
                else
                {
                    item_video.IsVisible = false;
                }
            }
            catch { }
        }

        private void ImageSource_ImageFailed(object sender, EventArgs e)
        {
            try
            {
                //ImageSource bitmapSource = item_source.Source as ImageSource;
                //Debug.WriteLine("Image list failed, showing the status:" + bitmapSource.UriSource.ToString());
                item_status.Text = "Image failed to load.";
                item_status.IsVisible = false;
                item_source.Source = null;
            }
            catch { }
        }

        private void ImageSource_ImageOpened(object sender, EventArgs e)
        {
            try
            {
                //if (item_source.Source.GetType() == typeof(ImageSource))
                //{
                //    ImageSource bitmapSource = item_source.Source as ImageSource;

                //    //Check the loaded image display size
                //    int PixelWidth = bitmapSource.PixelWidth;
                //    int PixelHeight = bitmapSource.PixelHeight;
                //    bool ScaleImage = this.Tag == null;
                //    if (PixelHeight != 0 && PixelWidth != 0)
                //    {
                //        if (PixelHeight <= 10 && PixelWidth <= 10)
                //        {
                //            Debug.WriteLine("Image is below 10px: " + PixelHeight + "h" + "/" + PixelWidth + "w");
                //            item_status.Text = "Image is too small.";
                //            item_status.IsVisible = false;
                //            item_video.IsVisible = false;
                //            item_border.IsVisible = false;
                //            return;
                //        }
                //        else if (ScaleImage && PixelHeight <= 120 && PixelWidth <= 120)
                //        {
                //            Debug.WriteLine("Image is below 120px: " + PixelHeight + "h" + "/" + PixelWidth + "w " + bitmapSource.UriSource);
                //            this.Tag = "NoScale";
                //            this.Width = PixelWidth;
                //            this.Height = PixelHeight;
                //            item_source.Width = PixelWidth;
                //            item_source.Height = PixelHeight;
                //            item_video.IsVisible = false;
                //            item_description.IsVisible = false;
                //        }
                //    }
                //}

                //Debug.WriteLine("Image list opened, hiding the status.");
                item_status.IsVisible = false;
            }
            catch { }
        }

        private void item_video_Click(object sender, EventArgs e)
        {
            try
            {
                if (item_source.IsAnimationPlaying)
                {
                    item_video.Source = ImageSource.FromResource("NewsScroll.Assets.iconVideoPlay.png");
                    item_source.IsAnimationPlaying = false;
                }
                else
                {
                    item_video.Source = ImageSource.FromResource("NewsScroll.Assets.iconVideoPause.png");
                    item_source.IsAnimationPlaying = true;
                }
            }
            catch { }
        }

        private void item_source_Clicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Image container image tapped: " + image_link);

                //Check image link
                if (string.IsNullOrWhiteSpace(image_link))
                {
                    return;
                }

                //Check if media is a gif(v) file
                if (image_link.ToLower().Contains(".gif"))
                {
                    item_video.Source = ImageSource.FromResource("NewsScroll.Assets.iconVideoPlay.png");
                    item_source.IsAnimationPlaying = false;
                }

                //Open the image in popup
                new ImagePopup().Popup(image_link);
            }
            catch { }
        }
    }
}