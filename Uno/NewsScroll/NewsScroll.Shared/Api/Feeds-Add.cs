using ArnoldVinkCode;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> AddFeed(string FeedLink)
        {
            try
            {
                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };
                StringContent PostContent = new StringContent("quickadd=" + WebUtility.HtmlEncode(FeedLink), Encoding.UTF8, "application/x-www-form-urlencoded");
                Uri PostUri = new Uri(ApiConnectionUrl + "subscription/quickadd");

                string PostHttp = await AVDownloader.SendPostRequestAsync(10000, "News Scroll", RequestHeader, PostUri, PostContent);
                JObject WebJObject = JObject.Parse(PostHttp);
                if (WebJObject["numResults"].ToString() == "0")
                {
                    await MessagePopup.Popup("Invalid feed link", "The entered feed link is invalid or does not contain a feed, please check your link and try again.", "Ok", "", "", "", "", false);
                    //System.Diagnostics.Debug.WriteLine(WebJObject["error"].ToString());
                    return false;
                }
                else
                {
                    await MessagePopup.Popup("Feed has been added", "Your new feed has been added to your account, and will appear on the next feed refresh.", "Ok", "", "", "", "", false);
                    return true;
                }
            }
            catch
            {
                await MessagePopup.Popup("Failed to add feed", "Please check your account settings, internet connection and try again.", "Ok", "", "", "", "", false);
                return false;
            }
        }
    }
}