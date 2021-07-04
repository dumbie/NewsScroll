using ArnoldVinkCode;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> ItemsStarred(bool Preload, bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { EventProgressDisableUI("Downloading starred status...", true); }
                Debug.WriteLine("Downloading starred status...");

                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };
                string DownloadString = await AVDownloader.DownloadStringAsync(15000, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "stream/items/ids?output=json&s=user/-/state/com.google/starred&n=" + AppVariables.StarredMaximumLoad));

                if (!string.IsNullOrWhiteSpace(DownloadString))
                {
                    JObject WebJObject = JObject.Parse(DownloadString);
                    if (WebJObject["itemRefs"] != null && WebJObject["itemRefs"].HasValues)
                    {
                        if (!Silent) { EventProgressDisableUI("Updating " + WebJObject["itemRefs"].Count() + " star status...", true); }
                        Debug.WriteLine("Updating " + WebJObject["itemRefs"].Count() + " star status...");

                        //Wait for busy database
                        await ApiUpdate.WaitForBusyDatabase();

                        //Check and set the received star item ids
                        List<TableItems> TableEditItems = await vSQLConnection.Table<TableItems>().ToListAsync();

                        string DownloadItemIds = string.Empty;
                        List<string> StarItemsID = new List<string>();
                        foreach (JToken JTokenRoot in WebJObject["itemRefs"])
                        {
                            string FoundItemId = JTokenRoot["id"].ToString().Replace(" ", string.Empty).Replace("tag:google.com,2005:reader/item/", string.Empty);
                            StarItemsID.Add(FoundItemId);

                            //Check if star item exists
                            if (!TableEditItems.Any(x => x.item_id == FoundItemId)) { DownloadItemIds += "&i=" + FoundItemId; }
                        }

                        //Update the star status in database
                        string StarUpdateString = string.Empty;
                        foreach (string StarItem in StarItemsID) { StarUpdateString += "'" + StarItem + "',"; }
                        StarUpdateString = AVFunctions.StringRemoveEnd(StarUpdateString, ",");
                        if (StarItemsID.Any())
                        {
                            int UpdatedItems = await vSQLConnection.ExecuteAsync("UPDATE TableItems SET item_star_status = ('1'), item_read_status = ('1') WHERE item_id IN (" + StarUpdateString + ") AND item_star_status = ('0')");
                            Debug.WriteLine("Updated star items: " + UpdatedItems);
                        }

                        //Update the unstar status in database
                        List<string> UnstarItemsID = TableEditItems.Where(x => x.item_star_status == true).Select(x => x.item_id).Except(StarItemsID).ToList();
                        string UnstarUpdateString = string.Empty;
                        foreach (string UnstarItem in UnstarItemsID) { UnstarUpdateString += "'" + UnstarItem + "',"; }
                        UnstarUpdateString = AVFunctions.StringRemoveEnd(UnstarUpdateString, ",");
                        if (UnstarItemsID.Any())
                        {
                            int UpdatedItems = await vSQLConnection.ExecuteAsync("UPDATE TableItems SET item_star_status = ('0') WHERE item_id IN (" + UnstarUpdateString + ") AND item_star_status = ('1')");
                            Debug.WriteLine("Updated unstar items: " + UpdatedItems);
                        }

                        //Download all missing starred items
                        if (!string.IsNullOrWhiteSpace(DownloadItemIds))
                        {
                            if (!Silent) { EventProgressDisableUI("Downloading starred items...", true); }
                            Debug.WriteLine("Downloading starred items...");

                            bool UpdatedItems = await MultiItems(DownloadItemIds, true, true, Silent, EnableUI);
                            if (!UpdatedItems)
                            {
                                EventProgressEnableUI();
                                return false;
                            }
                        }
                    }

                    if (EnableUI) { EventProgressEnableUI(); }
                    return true;
                }
                else
                {
                    EventProgressEnableUI();
                    return false;
                }
            }
            catch
            {
                EventProgressEnableUI();
                return false;
            }
        }
    }
}