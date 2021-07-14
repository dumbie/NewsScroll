using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NewsScroll
{
    public partial class HelpPage : ContentPage
    {
        public HelpPage()
        {
            InitializeComponent();
            Page_Loaded();
        }

        //Application Navigation
        private void Page_Loaded()
        {
            try
            {
                if (!sp_Help.Children.Any())
                {
                    Application.Current.Resources.TryGetValue("LabelDark", out object LabelDark);
                    Application.Current.Resources.TryGetValue("LabelLightGray", out object LabelLightGray);

                    sp_Help.Children.Add(new Label() { Text = "Creating an Old Reader account", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "You can create a free The Old Reader account by going to The Old Reader site https://theoldreader.com once you have registered you will also be able to manage your feeds from the site or directly in this app on The Old Reader page.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nWhere can I find the context menu?", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "By clicking on an item you will open it in the itemviewer, when you right click on an item you can share the item or mark it as read or starred.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nWhy are some items text cut off?", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "Long items are cut off to keep it more clear and easier to read, you can adjust the item cutting length on the settings page.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nHow can I ignore certain feeds?", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "You can un/ignore certain feeds on The Old Reader page, select the feeds you want to un/ignore and click on the 'Ignore' button, now when you go back to the item page those feeds will be hidden or shown.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nHow can I change a feed icon?", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "You can manually set a custom feed icon by right clicking on a feed in the list on The Old Reader page.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nHow much items will be loaded?", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "When you run the application for the first time it will download all items available untill the items download range you have set on the settings page, downloading ~1500 items uses around 2Mb of your bandwidth depending on the item content.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nHow much space do the offline items use?", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "When there are ~3000 items stored it can use around 10Mb off your device storage, images will be stored in your device's browers cache.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nWhich shortcuts can I use in the app?", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "Swipe from left to right: Hides the menu or goes back.\nMouse Back: Goes back or closes popup.\nEscape: Shows or hides the top menu.\nF2: Opens the display settings popup.\nF5: Refreshes the current page content.\n* Some of those shortcuts are page specific.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nDevice screen burn-in protection", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "If you are afraid the top menu may burn into your screen while reading news items, you can double click on the menu button to hide it.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nSpecial thanks", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "The Old Reader and SmartReader Web Parser.", Style = (Style)LabelLightGray });

                    sp_Help.Children.Add(new Label() { Text = "\r\nDevelopment donation", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "If you appreciate my project and want to support me with my projects you can make a donation through https://donation.arnoldvink.com", Style = (Style)LabelLightGray });

                    //Set the version text
                    VersionTracking.Track();
                    string currentVersion = VersionTracking.CurrentVersion;
                    sp_Help.Children.Add(new Label() { Text = "\r\nApplication made by Arnold Vink", Style = (Style)LabelDark });
                    sp_Help.Children.Add(new Label() { Text = "Version: v" + currentVersion, Style = (Style)LabelLightGray });
                }
            }
            catch { }
        }

        //Open the project website
        async void btn_ProjectWebsite_Tapped(object sender, EventArgs e)
        {
            try
            {
                await Browser.OpenAsync(new Uri("https://projects.arnoldvink.com"), BrowserLaunchMode.SystemPreferred);
            }
            catch { }
        }

        //Open the donation page
        async void btn_MakeDonation_Tapped(object sender, EventArgs e)
        {
            try
            {
                await Browser.OpenAsync(new Uri("https://donation.arnoldvink.com"), BrowserLaunchMode.SystemPreferred);
            }
            catch { }
        }

        //Open Privacy Policy page
        async void btn_PrivacyPolicy_Tapped(object sender, EventArgs e)
        {
            try
            {
                await Browser.OpenAsync(new Uri("https://privacy.arnoldvink.com"), BrowserLaunchMode.SystemPreferred);
            }
            catch { }
        }

        //User Interface - Buttons
        void iconSettings_Tap(object sender, EventArgs e)
        {
            try
            {
                App.NavigateToPage(new SettingsPage(), true, false);
            }
            catch { }
        }
    }
}