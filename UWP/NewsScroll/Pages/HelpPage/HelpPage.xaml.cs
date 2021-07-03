using ArnoldVinkCode;
using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NewsScroll
{
    public partial class HelpPage : Page
    {
        public HelpPage()
        {
            this.InitializeComponent();
        }

        //Application Navigation
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Loaded += delegate
            {
                try
                {
                    if (!sp_Help.Children.Any())
                    {
                        sp_Help.Children.Add(new TextBlock() { Text = "Creating an Old Reader account" });
                        sp_Help.Children.Add(new TextBlock() { Text = "You can create a free The Old Reader account by going to The Old Reader site https://theoldreader.com once you have registered you will also be able to manage your feeds from the site or directly in this app on The Old Reader page.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nWhere can I find the context menu?" });
                        sp_Help.Children.Add(new TextBlock() { Text = "By clicking on an item you will open it in the itemviewer, when you right click on an item you can share the item or mark it as read or starred.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nWhy are some items text cut off?" });
                        sp_Help.Children.Add(new TextBlock() { Text = "Long items are cut off to keep it more clear and easier to read, you can adjust the item cutting length on the settings page.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nHow can I ignore certain feeds?" });
                        sp_Help.Children.Add(new TextBlock() { Text = "You can un/ignore certain feeds on The Old Reader page, select the feeds you want to un/ignore and click on the 'Ignore' button, now when you go back to the item page those feeds will be hidden or shown.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nHow can I change a feed icon?" });
                        sp_Help.Children.Add(new TextBlock() { Text = "You can manually set a custom feed icon by right clicking on a feed in the list on The Old Reader page.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nHow much items will be loaded?" });
                        sp_Help.Children.Add(new TextBlock() { Text = "When you run the application for the first time it will download all items available untill the items download range you have set on the settings page, downloading ~1500 items uses around 2Mb of your bandwidth depending on the item content.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nHow much space do the offline items use?" });
                        sp_Help.Children.Add(new TextBlock() { Text = "When there are ~3000 items stored it can use around 10Mb off your device storage, images will be stored in your device's browers cache.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nWhich shortcuts can I use in the app?" });
                        sp_Help.Children.Add(new TextBlock() { Text = "Swipe from left to right: Hides the menu or goes back.\nMouse Back: Goes back or closes popup.\nEscape: Shows or hides the top menu.\nF2: Opens the display settings popup.\nF5: Refreshes the current page content.\n* Some of those shortcuts are page specific.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nDevice screen burn-in protection" });
                        sp_Help.Children.Add(new TextBlock() { Text = "If you are afraid the top menu may burn into your screen while reading news items, you can double click on the menu button to hide it.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nSpecial thanks" });
                        sp_Help.Children.Add(new TextBlock() { Text = "The Old Reader and SmartReader Web Parser.", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nDevelopment donation" });
                        sp_Help.Children.Add(new TextBlock() { Text = "If you appreciate my project and want to support me with my projects you can make a donation through https://donation.arnoldvink.com", Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });

                        //Set the version text
                        PackageVersion AppVersion = Package.Current.Id.Version;
                        sp_Help.Children.Add(new TextBlock() { Text = "\r\nApplication made by Arnold Vink" });
                        sp_Help.Children.Add(new TextBlock() { Text = "Version: v" + AppVersion.Major + "." + AppVersion.Minor + "." + AppVersion.Build + "." + AppVersion.Revision, Style = (Style)Application.Current.Resources["TextBlockLightGray"], TextWrapping = TextWrapping.Wrap });
                    }
                }
                catch { }
            };
        }

        //Open the project website
        async void btn_ProjectWebsite_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AVFunctions.DevMobile()) { await Launcher.LaunchUriAsync(new Uri("https://m.arnoldvink.com/?p=projects")); }
                else { await Launcher.LaunchUriAsync(new Uri("https://projects.arnoldvink.com")); }
            }
            catch { }
        }

        //Open the donation page
        async void btn_MakeDonation_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AVFunctions.DevMobile()) { await Launcher.LaunchUriAsync(new Uri("https://m.arnoldvink.com/?p=donation")); }
                else { await Launcher.LaunchUriAsync(new Uri("https://donation.arnoldvink.com")); }
            }
            catch { }
        }

        //Open Privacy Policy page
        async void btn_PrivacyPolicy_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri("https://privacy.arnoldvink.com"));
            }
            catch { }
        }

        //User Interface - Buttons
        void iconSettings_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                App.vApplicationFrame.Navigate(typeof(SettingsPage));
                App.vApplicationFrame.BackStack.Clear();
            }
            catch { }
        }
    }
}