using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;
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

        protected override async void OnStart()
        {
            try
            {
                Debug.WriteLine("News scroll startup checks.");

                //Check settings
                await SettingsPage.SettingsCheck();

                //Connect to the database
                if (!DatabaseConnect())
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    string messageResult = await AVMessageBox.Popup("Failed to connect to the database", "Your database will be cleared, please restart the application to continue.", messageAnswers);

                    await DatabaseReset();
                    //fix
                    //Application.Current.Exit();
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
