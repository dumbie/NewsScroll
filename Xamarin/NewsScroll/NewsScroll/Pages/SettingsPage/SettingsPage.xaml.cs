using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Database.Database;

namespace NewsScroll
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        //Page variables
        private string vPreviousAccount = string.Empty;

        public SettingsPage()
        {
            InitializeComponent();
            Page_Loaded();
        }

        private async void Page_Loaded()
        {
            try
            {
                //Load and set the settings
                await SettingsCheck();
                SettingsLoad();
                SettingsSave();

                //Store the previous username
                vPreviousAccount = AppSettingLoad("ApiAccount").ToString();

                //Load and set database size
                await UpdateSizeInformation();
            }
            catch { }
        }

        private async void iconNews_Tap(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NewsPage());
        }

        private void iconHelp_Tap(object sender, EventArgs e)
        {

        }

        private void iconMenu_Tap(object sender, EventArgs e)
        {

        }

        private void btn_RegisterAccount_Click(object sender, EventArgs e)
        {

        }

        private void ClearStoredItems_Click(object sender, EventArgs e)
        {

        }

        private void btn_OpenWiFiSettings_Click(object sender, EventArgs e)
        {

        }

        private async void ProjectWebsite_Click(object sender, EventArgs e)
        {
            try
            {
                await Browser.OpenAsync(new Uri("https://projects.arnoldvink.com"), BrowserLaunchMode.SystemPreferred);
            }
            catch { }
        }

        private void iconStar_Tap(object sender, EventArgs e)
        {

        }

        private void iconSearch_Tap(object sender, EventArgs e)
        {

        }

        private void iconApi_Tap(object sender, EventArgs e)
        {

        }

        //Load and set database size
        async Task UpdateSizeInformation()
        {
            try
            {
                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                string DatabaseSize = GetDatabaseSize();
                int TotalItems = await vSQLConnection.Table<TableItems>().CountAsync();
                int TotalFeeds = await vSQLConnection.Table<TableFeeds>().CountAsync();

                txt_OfflineStoredSize.Text = "Offline stored database is " + DatabaseSize + "\nin size and contains a total of " + TotalItems + "\nstored items and has " + TotalFeeds + " feeds in it.";
            }
            catch { }
        }
    }
}