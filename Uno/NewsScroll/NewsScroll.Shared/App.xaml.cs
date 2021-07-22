using NewsScroll.Styles;
using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static NewsScroll.Startup.Startup;

namespace NewsScroll
{
    public sealed partial class App : Application
    {
        //Application launch variables
        public static Frame vApplicationFrame = null;
        private static Window vApplicationWindow = null;
        private static bool vApplicationLaunching = true;

        //Initialize application
        public App()
        {
            try
            {
                this.InitializeComponent();

                //Allow native frame navigation
#if __IOS__ || __ANDROID__
                Uno.UI.FeatureConfiguration.Style.ConfigureNativeFrameNavigation();
#endif
            }
            catch { }
        }

        //Handle application launch
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                if (vApplicationLaunching)
                {
                    //Update application launch status
                    vApplicationLaunching = false;

                    //Register dynamic resource style
                    Current.Resources["DynamicStyle"] = new DynamicStyle();

                    //Check application startup
                    await ApplicationStartup();

                    //Set application frame and window
                    vApplicationWindow = Windows.UI.Xaml.Window.Current;
                    vApplicationFrame = new Frame();
                    vApplicationWindow.Content = vApplicationFrame;

                    //Navigate to startup page
                    vApplicationFrame.Navigate(typeof(NewsPage), e.Arguments);
                    vApplicationWindow.Activate();
                }
            }
            catch { }
        }
    }
}