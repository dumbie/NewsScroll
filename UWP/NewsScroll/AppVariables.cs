using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.ViewManagement;

namespace NewsScroll
{
    class AppVariables
    {
        //Application Variables
        public static IDictionary<string, object> ApplicationSettings = ApplicationData.Current.LocalSettings.Values;
        public static bool PreviousOnlineStatus = NetworkInterface.GetIsNetworkAvailable();
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

        //Culture Variables
        public static CultureInfo CultureInfoEnglish = new CultureInfo("en-US");
        public static CultureInfo CultureInfoLocal = new CultureInfo(Windows.Globalization.Language.CurrentInputMethodLanguageTag);
        public static DateTimeFormatInfo CultureInfoFormat = new CultureInfo(Windows.System.UserProfile.GlobalizationPreferences.HomeGeographicRegion).DateTimeFormat;

        //Blocked url list
        public static String[] BlockedListUrl = new String[] { "feedproxy.google.com", "feedburner.com", "placehold.it", "mbn2_twig" };

        //Items Current Variables
        public static Int32 CurrentTotalItemsCount = 0;
        public static Int32 CurrentFeedsLoaded = 0;
        public static Int32 CurrentItemsLoaded = 0;

        //Items Loading Settings
        public static Int32 ItemsToPreloadBatch = 5;
        public static Int32 ItemsToPreloadMax = 50;
        public static Int32 ItemsToScrollLoad = 100;
        public static Int32 ContentToScrollLoad = 6;
        public static Int32 ItemsMaximumLoad = 100000;
        public static Int32 StarredMaximumLoad = 500;
        public static Int32 MaximumItemTextLength = 8000;
        public static Int32 MaximumItemImageHeight = 320;
    }
}