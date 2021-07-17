using Xamarin.Forms;
using static NewsScroll.AppStartup.AppStartup;

namespace NewsScroll
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
                MainPage = new NewsPage();
            }
            catch { }
        }

        public static void NavigateToPage(ContentPage targetPage, bool wipeStack, bool popUp)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (popUp)
                    {
                        await Application.Current.MainPage.Navigation.PushModalAsync(targetPage, false);
                    }
                    else
                    {
                        Application.Current.MainPage = targetPage;
                    }
                });
            }
            catch { }
        }

        protected override async void OnStart()
        {
            try
            {
                await ApplicationStart();
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