using ArnoldVinkCode;
using NewsScroll.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> ItemsRead(ObservableCollection<Items> UpdateList, bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { await EventProgressDisableUI("Downloading read status...", true); }
                System.Diagnostics.Debug.WriteLine("Downloading read status...");

                //Get all stored items from the database
                List<TableItems> CurrentItems = await SQLConnection.Table<TableItems>().ToListAsync();
                if (CurrentItems.Any())
                {
                    //Get last stored item date minus starred items
                    TableItems LastStoredItem = CurrentItems.Where(x => x.item_star_status == false).OrderByDescending(x => x.item_datetime).LastOrDefault();
                    if (LastStoredItem != null)
                    {
                        //Date time calculations
                        DateTime RemoveItemsRange = LastStoredItem.item_datetime.AddHours(-1);
                        //System.Diagnostics.Debug.WriteLine("Downloading read items till: " + LastStoredItem.item_title + "/" + RemoveItemsRange);
                        long UnixTimeTicks = (RemoveItemsRange.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000000; //Second

                        string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };
                        Uri DownloadUri = new Uri(ApiConnectionUrl + "stream/items/ids?output=json&s=user/-/state/com.google/read&n=" + AppVariables.ItemsMaximumLoad + "&ot=" + UnixTimeTicks);
                        string DownloadString = await AVDownloader.DownloadStringAsync(10000, "News Scroll", RequestHeader, DownloadUri);

                        if (!string.IsNullOrWhiteSpace(DownloadString))
                        {
                            JObject WebJObject = JObject.Parse(DownloadString);
                            if (WebJObject["itemRefs"] != null && WebJObject["itemRefs"].HasValues)
                            {
                                if (!Silent) { await EventProgressDisableUI("Updating " + WebJObject["itemRefs"].Count() + " read status...", true); }
                                System.Diagnostics.Debug.WriteLine("Updating " + WebJObject["itemRefs"].Count() + " read status...");

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
                                    int UpdatedItems = await SQLConnection.ExecuteAsync("UPDATE TableItems SET item_read_status = ('1') WHERE item_id IN (" + ReadUpdateString + ") AND item_read_status = ('0')");
                                    System.Diagnostics.Debug.WriteLine("Updated read items: " + UpdatedItems);
                                }

                                //Update the read status in list
                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    try
                                    {
                                        List<Items> ReadItemsIDList = UpdateList.Where(x => x.item_read_status == Visibility.Collapsed && ReadItemsList.Any(y => y == x.item_id)).ToList();
                                        foreach (Items ReadItem in ReadItemsIDList) { ReadItem.item_read_status = Visibility.Visible; }
                                    }
                                    catch { }
                                });

                                //Update the unread status in database
                                string UnreadUpdateString = string.Empty;
                                List<string> UnreadItemsList = (await SQLConnection.Table<TableItems>().ToListAsync()).Where(x => x.item_read_status == true && x.item_datetime > RemoveItemsRange).Select(x => x.item_id).Except(ReadItemsList).ToList();
                                foreach (string UnreadItem in UnreadItemsList) { UnreadUpdateString += "'" + UnreadItem + "',"; }
                                if (UnreadItemsList.Any())
                                {
                                    UnreadUpdateString = AVFunctions.StringRemoveEnd(UnreadUpdateString, ",");
                                    int UpdatedItems = await SQLConnection.ExecuteAsync("UPDATE TableItems SET item_read_status = ('0') WHERE item_id IN (" + UnreadUpdateString + ") AND item_read_status = ('1')");
                                    System.Diagnostics.Debug.WriteLine("Updated unread items: " + UpdatedItems);
                                }

                                //Update the unread status in list
                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    try
                                    {
                                        List<Items> UnreadItemsIDList = UpdateList.Where(x => x.item_read_status == Visibility.Visible && UnreadItemsList.Any(y => y == x.item_id)).ToList();
                                        foreach (Items UnreadItem in UnreadItemsIDList) { UnreadItem.item_read_status = Visibility.Collapsed; }
                                    }
                                    catch { }
                                });
                            }
                        }
                    }
                }

                if (EnableUI) { await EventProgressEnableUI(); }
                return true;
            }
            catch
            {
                await EventProgressEnableUI();
                return false;
            }
        }
    }
}