using System.Diagnostics;
using System.Threading.Tasks;
using static NewsScroll.Events.Events;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> ItemsStarred(bool Preload, bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { await EventProgressDisableUI("Downloading starred status...", true); }
                Debug.WriteLine("Downloading starred status...");

                //string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };
                //string DownloadString = await AVDownloader.DownloadStringAsync(15000, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "stream/items/ids?output=json&s=user/-/state/com.google/starred&n=" + AppVariables.StarredMaximumLoad));

                //if (!String.IsNullOrWhiteSpace(DownloadString))
                //{
                //    JObject WebJObject = JObject.Parse(DownloadString);
                //    if (WebJObject["itemRefs"] != null && WebJObject["itemRefs"].HasValues)
                //    {
                //        if (!Silent) { await EventProgressDisableUI("Updating " + WebJObject["itemRefs"].Count() + " star status...", true); }
                //        Debug.WriteLine("Updating " + WebJObject["itemRefs"].Count() + " star status...");

                //        //Wait for busy database
                //        await ApiUpdate.WaitForBusyDatabase();

                //        //Check and set the received star item ids
                //        List<TableItems> TableEditItems = await vSQLConnection.Table<TableItems>().ToListAsync();

                //        string DownloadItemIds = String.Empty;
                //        List<String> StarItemsID = new List<String>();
                //        foreach (JToken JTokenRoot in WebJObject["itemRefs"])
                //        {
                //            string FoundItemId = JTokenRoot["id"].ToString().Replace(" ", String.Empty).Replace("tag:google.com,2005:reader/item/", String.Empty);
                //            StarItemsID.Add(FoundItemId);

                //            //Check if star item exists
                //            if (!TableEditItems.Any(x => x.item_id == FoundItemId)) { DownloadItemIds += "&i=" + FoundItemId; }
                //        }

                //        //Update the star status in database
                //        string StarUpdateString = String.Empty;
                //        foreach (String StarItem in StarItemsID) { StarUpdateString += "'" + StarItem + "',"; }
                //        StarUpdateString = AVFunctions.StringRemoveEnd(StarUpdateString, ",");
                //        if (StarItemsID.Any())
                //        {
                //            Int32 UpdatedItems = await vSQLConnection.ExecuteAsync("UPDATE TableItems SET item_star_status = ('1'), item_read_status = ('1') WHERE item_id IN (" + StarUpdateString + ") AND item_star_status = ('0')");
                //            Debug.WriteLine("Updated star items: " + UpdatedItems);
                //        }

                //        //Update the unstar status in database
                //        List<String> UnstarItemsID = TableEditItems.Where(x => x.item_star_status == true).Select(x => x.item_id).Except(StarItemsID).ToList();
                //        string UnstarUpdateString = String.Empty;
                //        foreach (String UnstarItem in UnstarItemsID) { UnstarUpdateString += "'" + UnstarItem + "',"; }
                //        UnstarUpdateString = AVFunctions.StringRemoveEnd(UnstarUpdateString, ",");
                //        if (UnstarItemsID.Any())
                //        {
                //            Int32 UpdatedItems = await vSQLConnection.ExecuteAsync("UPDATE TableItems SET item_star_status = ('0') WHERE item_id IN (" + UnstarUpdateString + ") AND item_star_status = ('1')");
                //            Debug.WriteLine("Updated unstar items: " + UpdatedItems);
                //        }

                //        //Download all missing starred items
                //        if (!String.IsNullOrWhiteSpace(DownloadItemIds))
                //        {
                //            if (!Silent) { await EventProgressDisableUI("Downloading starred items...", true); }
                //            Debug.WriteLine("Downloading starred items...");

                //            bool UpdatedItems = await MultiItems(DownloadItemIds, true, true, Silent, EnableUI);
                //            if (!UpdatedItems)
                //            {
                //                await EventProgressEnableUI();
                //                return false;
                //            }
                //        }
                //    }

                //    if (EnableUI) { await EventProgressEnableUI(); }
                //    return true;
                //}
                //else
                //{
                //    await EventProgressEnableUI();
                //    return false;
                //}
                return false;
            }
            catch
            {
                await EventProgressEnableUI();
                return false;
            }
        }
    }
}