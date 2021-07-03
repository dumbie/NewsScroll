using NewsScroll.Classes;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NewsScroll
{
    class AppVariables
    {
        //Application Variables
        public static bool PreviousOnlineStatus = NetworkInterface.GetIsNetworkAvailable();
        public static int vSingleTappedClickCount = 0;
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
        public static CultureInfo CultureInfoLocal = new CultureInfo(CultureInfo.CurrentUICulture.ToString());

        //Blocked url list
        public static string[] BlockedListUrl = new string[] { "feedproxy.google.com", "feedburner.com", "placehold.it", "mbn2_twig" };

        //Items Current Variables
        public static int CurrentTotalItemsCount = 0;
        public static int CurrentFeedsLoaded = 0;
        public static int CurrentItemsLoaded = 0;

        //Items Loading Settings
        public static int ItemsToPreloadBatch = 5;
        public static int ItemsToPreloadMax = 50;
        public static int ItemsToScrollLoad = 100;
        public static int ContentToScrollLoad = 6;
        public static int ItemsMaximumLoad = 100000;
        public static int StarredMaximumLoad = 500;
        public static int MaximumItemTextLength = 8000;
        public static int MaximumItemImageHeight = 320;

        //Search Variables
        public static Feeds vSearchFeed = null;
        public static string vSearchTerm = string.Empty;
        public static string vSearchFeedTitle = string.Empty;

        //Check double click count
        public static async Task<int> DoubleClickCheck()
        {
            try
            {
                vSingleTappedClickCount++;
                if (vSingleTappedClickCount == 1)
                {
                    await Task.Delay(300);
                    if (vSingleTappedClickCount == 1)
                    {
                        vSingleTappedClickCount = 0;
                        return 1;
                    }
                    else
                    {
                        vSingleTappedClickCount = 0;
                        return 2;
                    }
                }
            }
            catch { }
            return 0;
        }
    }
}