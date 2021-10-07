using System.Collections.Generic;
using System.Globalization;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.ViewManagement;

namespace NewsScroll
{
    class AppVariables
    {
        //Application Variables
        public static IDictionary<string, object> ApplicationSettings = ApplicationData.Current.LocalSettings.Values;
        public static ApplicationView ApplicationView = ApplicationView.GetForCurrentView();
        public static DisplayRequest DisplayRequest = new DisplayRequest();
        public static bool SingleTappedEvent = true;
        public static bool BusyApplication = false;
        public static bool HeaderHidden = false;

        //Loading Variables
        public static bool LoadNews = false;
        public static bool LoadStarred = false;
        public static bool LoadSearch = false;
        public static bool LoadFeeds = false;
        public static bool LoadMedia = true;

        //Internet Variables
        public static bool InternetAccess = true;
        public static bool PreviousInternetAccess = true;

        //Culture Variables
        public static CultureInfo CultureInfoEnglish = new CultureInfo("en-US");
        public static CultureInfo CultureInfoLocal = new CultureInfo(Windows.Globalization.Language.CurrentInputMethodLanguageTag);
        public static DateTimeFormatInfo CultureInfoFormat = new CultureInfo(Windows.Globalization.Language.CurrentInputMethodLanguageTag).DateTimeFormat;

        //Blocked url list
        public static string[] BlockedListUrl = new string[] { "feedproxy.google.com", "feedburner.com", "placehold.it", "mbn2_twig" };

        //Items Current Variables
        public static int CurrentShownItemCount = 0;
        public static int CurrentTotalItemsCount = 0;
        public static int CurrentFeedsLoaded = 0;
        public static int CurrentItemsLoaded = 0;

        //Items Loading Settings
        public static int ContentToScrollLoad = 5;
        public static int ItemsToScrollLoad = 100000;
        public static int ItemsMaximumLoad = 100000;
        public static int StarredMaximumLoad = 500;
        public static int MaximumItemTextLength = 8000;
        public static int DefaultMediaHeight = 320;

        //Update internet access
        public static void UpdateInternetAccess()
        {
            try
            {
                //Update previous internet access
                PreviousInternetAccess = InternetAccess;

                //Update current internet access
                ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                InternetAccess = connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            }
            catch
            {
                InternetAccess = false;
            }
        }
    }
}