using ArnoldVinkCode;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Database.Database;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> DeleteFeed(string FeedId)
        {
            try
            {
                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };

                string PostString = "ac=unsubscribe&s=feed/" + FeedId;
                StringContent PostContent = new StringContent(PostString, Encoding.UTF8, "application/x-www-form-urlencoded");
                Uri PostUri = new Uri(ApiConnectionUrl + "subscription/edit");

                string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, PostUri, PostContent);
                if (PostHttp != null && (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")))
                {
                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    //Clear feed from database
                    await vSQLConnection.ExecuteAsync("DELETE FROM TableFeeds WHERE feed_id = ('" + FeedId + "')");

                    //Clear items from database
                    await vSQLConnection.ExecuteAsync("DELETE FROM TableItems WHERE item_feed_id = ('" + FeedId + "') AND item_star_status = ('0')");

                    //Delete the feed icon
                    AVFiles.File_Delete(FeedId + ".png", true);

                    Debug.WriteLine("Deleted the feed and items off: " + FeedId);
                    return true;
                }
                else
                {
                    Debug.WriteLine("Failed to delete feed: " + FeedId + " / server error.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to delete feed: " + FeedId + " / " + ex.Message);
                return false;
            }
        }
    }
}