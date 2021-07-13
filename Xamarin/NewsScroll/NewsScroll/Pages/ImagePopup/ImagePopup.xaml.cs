using ArnoldVinkCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppVariables;

namespace NewsScroll
{
    public partial class ImagePopup : ContentPage
    {
        //Popup Variables
        private static string vImageLink = string.Empty;

        //Initialize popup
        public ImagePopup()
        {
            InitializeComponent();
            Page_Loaded();
        }

        private void Page_Loaded()
        {
            try
            {
                Debug.WriteLine("Loading image viewer...");

                //Focus on the popup
                iconClose.Focus();

                //Adjust the swiping direction
                SwipeBarAdjust();

                //Register page events
                RegisterPageEvents();
            }
            catch { }
        }

        //Load and set the image
        private async Task LoadImage(string imageLink)
        {
            try
            {
                //Load the image source
                vImageLink = imageLink;
                Uri imageUri = new Uri(vImageLink);
                Stream imageStream = await dependencyAVImages.DownloadResizeImage(imageUri, 2048, 2048);
                ImageSource imageSource = ImageSource.FromStream(() => imageStream);

                if (imageSource != null)
                {
                    //Set the image to viewer
                    image_source.Source = imageSource;
                    image_status.IsVisible = false;

                    //Check if media is a gif(v) file
                    bool ImageIsGif = imageLink.ToLower().Contains(".gif");
                    if (ImageIsGif)
                    {
                        item_video.IsVisible = true;
                    }
                }
                else
                {
                    image_status.Text = "Image failed to load.";
                    image_source.Source = null;
                    image_status.IsVisible = true;
                }
            }
            catch { }
        }

        //Show the popup
        public void Popup(string imageLink)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    ImagePopup newPopup = new ImagePopup();
                    await Application.Current.MainPage.Navigation.PushModalAsync(newPopup, false);
                    await newPopup.LoadImage(imageLink);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create imagepopup: " + ex.Message);
            }
        }

        //Close the popup
        private void iconClose_Tap(object sender, EventArgs e) { try { Close(); } catch { } }
        public void Close()
        {
            try
            {
                //Reset the image resources
                image_source.Source = null;

                //Disable page events
                DisablePageEvents();

                //Close the popup
                this.IsVisible = false;
            }
            catch { }
        }

        //Hide and show menu
        private void image_source_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (grid_Header.IsVisible == true)
                {
                    grid_Header.IsVisible = false;
                }
                else
                {
                    grid_Header.IsVisible = true;
                }
            }
            catch { }
        }

        //Play/pause the video
        private void item_video_Click(object sender, EventArgs e)
        {
            try
            {
                if (image_source.IsAnimationPlaying)
                {
                    item_video.Source = ImageSource.FromResource("NewsScroll.Assets.iconVideoPlay.png");
                    image_source.IsAnimationPlaying = false;
                }
                else
                {
                    item_video.Source = ImageSource.FromResource("NewsScroll.Assets.iconVideoPause.png");
                    image_source.IsAnimationPlaying = true;
                }
            }
            catch { }
        }

        //Save the image
        private async void button_iconSave_Tap(object sender, EventArgs e)
        {
            try
            {
                string fileName = Path.GetFileName(vImageLink);
                string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                string filePath = Path.Combine(localFolder, fileName);

                //Download image
                Uri imageUri = new Uri(vImageLink);
                Stream imageStream = await dependencyAVImages.DownloadResizeImage(imageUri, 999999, 999999);

                //Save image
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    imageStream.Position = 0;
                    await imageStream.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    AVFiles.File_SaveBytes(filePath, imageBytes, true, false);
                }
            }
            catch
            {
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Ok");

                await MessagePopup.Popup("Failed to save", "Failed to save the image, please check your internet connection and try again.", messageAnswers);
                Debug.WriteLine("Failed to save the image.");
            }
        }

        //Register page events
        private void RegisterPageEvents()
        {
            try
            {
                Debug.WriteLine("Registering page events...");

                //Monitor user touch swipe
                if (!(bool)AppSettingLoad("DisableSwipeActions"))
                {
                    SwipeBarAdjust();
                }
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
            }
            catch { }
        }
    }
}