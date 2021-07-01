using ArnoldVinkCode;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> AddFeed(string FeedLink)
        {
            try
            {
                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };

                string PostString = "quickadd=" + WebUtility.HtmlEncode(FeedLink);
                StringContent PostContent = new StringContent(PostString, Encoding.UTF8, "application/x-www-form-urlencoded");
                Uri PostUri = new Uri(ApiConnectionUrl + "subscription/quickadd");

                string PostHttp = await AVDownloader.SendPostRequestAsync(10000, "News Scroll", RequestHeader, PostUri, PostContent);
                JObject WebJObject = JObject.Parse(PostHttp);
                if (WebJObject["numResults"].ToString() == "0")
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await AVMessageBox.Popup("Invalid feed link", "The entered feed link is invalid or does not contain a feed, please check your link and try again.", messageAnswers);
                    //Debug.WriteLine(WebJObject["error"].ToString());
                    return false;
                }
                else
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await AVMessageBox.Popup("Feed has been added", "Your new feed has been added to your account, and will appear on the next feed refresh.", messageAnswers);
                    return true;
                }
            }
            catch
            {
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Ok");

                await AVMessageBox.Popup("Failed to add feed", "Please check your account settings, internet connection and try again.", messageAnswers);
                return false;
            }
        }
    }
}