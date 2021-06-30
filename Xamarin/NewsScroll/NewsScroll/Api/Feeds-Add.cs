using System.Threading.Tasks;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> AddFeed(string FeedLink)
        {
            //try
            //{
            //    string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };
            //    HttpStringContent PostContent = new HttpStringContent("quickadd=" + WebUtility.HtmlEncode(FeedLink), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");
            //    Uri PostUri = new Uri(ApiConnectionUrl + "subscription/quickadd");

            //    HttpResponseMessage PostHttp = await AVDownloader.SendPostRequestAsync(10000, "News Scroll", RequestHeader, PostUri, PostContent);
            //    JObject WebJObject = JObject.Parse(PostHttp.Content.ToString());
            //    if (WebJObject["numResults"].ToString() == "0")
            //    {
            //        //await AVMessageBox.Popup("Invalid feed link", "The entered feed link is invalid or does not contain a feed, please check your link and try again.", "Ok", "", "", "", "", false);
            //        //Debug.WriteLine(WebJObject["error"].ToString());
            //        return false;
            //    }
            //    else
            //    {
            //        //await AVMessageBox.Popup("Feed has been added", "Your new feed has been added to your account, and will appear on the next feed refresh.", "Ok", "", "", "", "", false);
            //        return true;
            //    }
            //}
            //catch
            //{
            //    //await AVMessageBox.Popup("Failed to add feed", "Please check your account settings, internet connection and try again.", "Ok", "", "", "", "", false);
            //    return false;
            //}
            return false;
        }
    }
}