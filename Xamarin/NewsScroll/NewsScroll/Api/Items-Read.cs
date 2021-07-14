using ArnoldVinkCode;
using NewsScroll.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> ItemsRead(ObservableCollection<Items> UpdateList, bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { EventProgressDisableUI("Downloading read status...", true); }
                Debug.WriteLine("Downloading read status...");

                //Get all stored items from the database
                List<TableItems> CurrentItems = await vSQLConnection.Table<TableItems>().ToListAsync();
                if (CurrentItems.Any())
                {
                    //Get last stored item date minus starred items
                    TableItems LastStoredItem = CurrentItems.Where(x => x.item_star_status == false).OrderByDescending(x => x.item_datetime).LastOrDefault();
                    if (LastStoredItem != null)
                    {
                        //Date time calculations
                        DateTime RemoveItemsRange = LastStoredItem.item_datetime.AddHours(-1);
                        //Debug.WriteLine("Downloading read items till: " + LastStoredItem.item_title + "/" + RemoveItemsRange);
                        long UnixTimeTicks = (RemoveItemsRange.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000000; //Second

                        string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };
                        Uri DownloadUri = new Uri(ApiConnectionUrl + "stream/items/ids?output=json&s=user/-/state/com.google/read&n=" + AppVariables.ItemsToSyncMax + "&ot=" + UnixTimeTicks);
                        string DownloadString = await AVDownloader.DownloadStringAsync(10000, "News Scroll", RequestHeader, DownloadUri);

                        if (!string.IsNullOrWhiteSpace(DownloadString))
                        {
                            JObject WebJObject = JObject.Parse(DownloadString);
                            if (WebJObject["itemRefs"] != null && WebJObject["itemRefs"].HasValues)
                            {
                                if (!Silent) { EventProgressDisableUI("Updating " + WebJObject["itemRefs"].Count() + " read status...", true); }
                                Debug.WriteLine("Updating " + WebJObject["itemRefs"].Count() + " read status...");

                                //Check and set the received read item ids
                                string ReadUpdateString = string.Empty;
                                List<string> ReadItemsList = new List<string>();
                                foreach (JToken JTokenRoot in WebJObject["itemRefs"])
                                {
                                    string FoundItemId = JTokenRoot["id"].ToString().Replace(" ", string.Empty).Replace("tag:google.com,2005:reader/item/", string.Empty);
                                    ReadUpdateString += "'" + FoundItemId + "',";
                                    ReadItemsList.Add(FoundItemId);
                                }

                                //Update the read status in database
                                if (ReadItemsList.Any())
                                {
                                    ReadUpdateString = AVFunctions.StringRemoveEnd(ReadUpdateString, ",");
                                    int UpdatedItems = await vSQLConnection.ExecuteAsync("UPDATE TableItems SET item_read_status = ('1') WHERE item_id IN (" + ReadUpdateString + ") AND item_read_status = ('0')");
                                    Debug.WriteLine("Updated read items: " + UpdatedItems);
                                }

                                //Update the read status in list
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    try
                                    {
                                        List<Items> ReadItemsIDList = UpdateList.Where(x => x.item_read_status == false && ReadItemsList.Any(y => y == x.item_id)).ToList();
                                        foreach (Items ReadItem in ReadItemsIDList)
                                        {
                                            ReadItem.item_read_status = true;
                                        }
                                    }
                                    catch { }
                                });

                                //Update the unread status in database
                                string UnreadUpdateString = string.Empty;
                                List<string> UnreadItemsList = (await vSQLConnection.Table<TableItems>().ToListAsync()).Where(x => x.item_read_status == true && x.item_datetime > RemoveItemsRange).Select(x => x.item_id).Except(ReadItemsList).ToList();
                                foreach (string UnreadItem in UnreadItemsList) { UnreadUpdateString += "'" + UnreadItem + "',"; }
                                if (UnreadItemsList.Any())
                                {
                                    UnreadUpdateString = AVFunctions.StringRemoveEnd(UnreadUpdateString, ",");
                                    int UpdatedItems = await vSQLConnection.ExecuteAsync("UPDATE TableItems SET item_read_status = ('0') WHERE item_id IN (" + UnreadUpdateString + ") AND item_read_status = ('1')");
                                    Debug.WriteLine("Updated unread items: " + UpdatedItems);
                                }

                                //Update the unread status in list
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    try
                                    {
                                        List<Items> UnreadItemsIDList = UpdateList.Where(x => x.item_read_status == true && UnreadItemsList.Any(y => y == x.item_id)).ToList();
                                        foreach (Items UnreadItem in UnreadItemsIDList)
                                        {
                                            UnreadItem.item_read_status = false;
                                        }
                                    }
                                    catch { }
                                });
                            }
                        }
                    }
                }

                if (EnableUI) { EventProgressEnableUI(); }
                return true;
            }
            catch
            {
                EventProgressEnableUI();
                return false;
            }
        }
    }
}