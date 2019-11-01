using NewsScroll.Styles;
using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static NewsScroll.Startup.Startup;

namespace NewsScroll
{
    sealed partial class App : Application
    {
        //Application launch variables
        static public Frame vApplicationFrame = new Frame();
        static public string vLaunchTileActivatedCommand = String.Empty;
        static public string vLaunchVoiceActivatedCommand = String.Empty;
        static public string vLaunchVoiceActivatedSpoken = String.Empty;
        static public string vApplicationLaunchArgs = String.Empty;
        static public bool vApplicationLaunching = true;
        static public bool vApplicationActivating = false;

        //Handle application unhandled exception
        public App()
        {
            UnhandledException += async (sender, e) =>
            {
                if (e != null)
                {
                    e.Handled = true;
                    Nullable<bool> MessageDialogResult = null;
                    MessageDialog MessageDialog = new MessageDialog("Sadly enough something went wrong in the application, if the application no longer works like it should you can try to restart the application, do you want to try to continue or close the application?\n\nThe application received the following error message: " + e.Message, "News Scroll");
                    MessageDialog.Commands.Add(new UICommand("Continue", new UICommandInvokedHandler((cmd) => MessageDialogResult = true)));
                    MessageDialog.Commands.Add(new UICommand("Close App", new UICommandInvokedHandler((cmd) => MessageDialogResult = false)));
                    await MessageDialog.ShowAsync();

                    if (MessageDialogResult == true) { return; } else { Application.Current.Exit(); }
                }
            };
        }

        //Handle application launching
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                //Set launch commands to string
                vApplicationLaunchArgs = args.Arguments;
                vLaunchTileActivatedCommand = args.TileId;
                vLaunchVoiceActivatedCommand = String.Empty;
                vLaunchVoiceActivatedSpoken = String.Empty;

                if (vApplicationLaunching)
                {
                    //Register the style updater
                    Current.Resources["StyleUpdater"] = new StyleUpdater();

                    //Check application startup
                    await ApplicationStart();

                    //Start initializing user interface
                    vApplicationFrame.Navigate(typeof(NewsPage));
                    vApplicationFrame.BackStack.Clear();

                    Window.Current.Content = vApplicationFrame;
                    Window.Current.Activate();

                    //Set application launch to false
                    vApplicationLaunching = false;
                }
                else { vApplicationActivating = true; }
            }
            catch { }
        }

        //Handle application activation
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            try
            {
                //Set launch commands to string
                vApplicationLaunchArgs = args.Kind.ToString();
                if (args.Kind == ActivationKind.VoiceCommand)
                {
                    vLaunchTileActivatedCommand = String.Empty;
                    vLaunchVoiceActivatedCommand = ((VoiceCommandActivatedEventArgs)args).Result.RulePath[0];
                    vLaunchVoiceActivatedSpoken = ((VoiceCommandActivatedEventArgs)args).Result.Text;
                }

                if (vApplicationLaunching)
                {
                    //Register the style updater
                    Current.Resources["StyleUpdater"] = new StyleUpdater();

                    //Check application startup
                    await ApplicationStart();

                    //Start initializing user interface
                    vApplicationFrame.Navigate(typeof(NewsPage));
                    vApplicationFrame.BackStack.Clear();

                    Window.Current.Content = vApplicationFrame;
                    Window.Current.Activate();

                    //Set application launch to false
                    vApplicationLaunching = false;
                }
                else { vApplicationActivating = true; }
            }
            catch { }
        }
    }
}