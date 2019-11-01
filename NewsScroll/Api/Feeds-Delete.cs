using ArnoldVinkCode;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.Http;
using static NewsScroll.Database.Database;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> DeleteFeed(string FeedId)
        {
            try
            {
                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };
                HttpStringContent PostContent = new HttpStringContent("ac=unsubscribe&s=feed/" + FeedId, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");
                HttpResponseMessage PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "subscription/edit"), PostContent);
                if (PostHttp != null && (PostHttp.Content.ToString() == "OK" || PostHttp.Content.ToString().Contains("<error>Not found</error>")))
                {
                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    //Clear feed from database
                    await SQLConnection.ExecuteAsync("DELETE FROM TableFeeds WHERE feed_id = ('" + FeedId + "')");

                    //Clear items from database
                    await SQLConnection.ExecuteAsync("DELETE FROM TableItems WHERE item_feed_id = ('" + FeedId + "') AND item_star_status = ('0')");

                    //Delete the feed icon
                    IStorageItem LocalFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FeedId + ".png");
                    if (LocalFile != null) { try { await LocalFile.DeleteAsync(StorageDeleteOption.PermanentDelete); } catch { } }

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