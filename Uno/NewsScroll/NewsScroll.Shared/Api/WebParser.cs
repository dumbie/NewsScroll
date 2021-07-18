using SmartReader;
using System.Net;
using System.Threading.Tasks;
using static NewsScroll.Events.Events;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<string> WebParser(string ItemLink, bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { await EventProgressDisableUI("Downloading the full item...", true); }
                System.Diagnostics.Debug.WriteLine("Downloading the full item: " + ItemLink);

                ItemLink = WebUtility.HtmlDecode(ItemLink);
                ItemLink = WebUtility.UrlDecode(ItemLink);

                Reader smartReader = new Reader(ItemLink);
                //fixsmartReader.SetCustomUserAgent();
                Article smartArticle = await smartReader.GetArticleAsync();

                if (!Silent) { await EventProgressDisableUI("Processing the full item...", true); }
                System.Diagnostics.Debug.WriteLine("Processing the full item.");

                string DownloadString = smartArticle.Content;
                DownloadString = WebUtility.HtmlDecode(DownloadString);
                //DownloadString = WebUtility.UrlDecode(DownloadString);

                if (EnableUI) { await EventProgressEnableUI(); }
                return DownloadString;
            }
            catch
            {
                await EventProgressEnableUI();
                return string.Empty;
            }
        }
    }
}