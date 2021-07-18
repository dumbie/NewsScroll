using ArnoldVinkCode;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
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
                StringContent PostContent = new StringContent("ac=unsubscribe&s=feed/" + FeedId, Encoding.UTF8, "application/x-www-form-urlencoded");
                string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "subscription/edit"), PostContent);
                if (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>"))
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

                    System.Diagnostics.Debug.WriteLine("Deleted the feed and items off: " + FeedId);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to delete feed: " + FeedId + " / server error.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to delete feed: " + FeedId + " / " + ex.Message);
                return false;
            }
        }
    }
}