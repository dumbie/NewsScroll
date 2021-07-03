using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;

namespace NewsScroll
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NewsPage();
        }

        public static void NavigateToPage(ContentPage targetPage, bool wipeStack)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = targetPage;
                    //await Application.Current.MainPage.Navigation.PushModalAsync(targetPage);
                });
            }
            catch { }
        }

        protected override async void OnStart()
        {
            try
            {
                Debug.WriteLine("NewsScroll startup checks.");

                //Check settings
                await SettingsPage.SettingsCheck();

                //Register Application Events
                EventsRegister();

                //Connect to the database
                if (!DatabaseConnect())
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    string messageResult = await AVMessageBox.Popup("Failed to connect to the database", "Your database will be cleared, please restart the application to continue.", messageAnswers);

                    await DatabaseReset();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    return;
                }

                //Create the database tables
                await DatabaseCreate();
            }
            catch { }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}