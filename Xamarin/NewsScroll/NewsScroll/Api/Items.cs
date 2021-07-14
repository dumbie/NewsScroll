using ArnoldVinkCode;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> AllNewsItems(bool Preload, bool IgnoreDate, bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { EventProgressDisableUI("Downloading latest items...", true); }
                Debug.WriteLine("Downloading latest items...");

                //Date time calculations
                long UnixTimeTicks = 0;
                DateTime RemoveItemsRange = DateTime.UtcNow.AddDays(-Convert.ToDouble(AppSettingLoad("RemoveItemsRange")));
                if (AppSettingLoad("LastItemsUpdate").ToString() == "Never")
                {
                    UnixTimeTicks = (RemoveItemsRange.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000000; //Second
                }
                else
                {
                    UnixTimeTicks = ((Convert.ToDateTime(AppSettingLoad("LastItemsUpdate"), AppVariables.CultureInfoEnglish).AddMinutes(-15)).Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000000; //Second
                }

                //Set the last update time string
                string LastUpdate = DateTime.UtcNow.ToString(AppVariables.CultureInfoEnglish);

                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };
                string DownloadString = await AVDownloader.DownloadStringAsync(20000, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "stream/contents?output=json&n=" + AppVariables.ItemsToSyncMax + "&ot=" + UnixTimeTicks));

                bool UpdatedItems = await DownloadToTableItemList(Preload, IgnoreDate, DownloadString, Silent, EnableUI);
                if (UpdatedItems)
                {
                    //Save the last update time string
                    await AppSettingSave("LastItemsUpdate", LastUpdate);

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

        static public async Task<bool> MultiItems(string DownloadItemIds, bool Preload, bool IgnoreDate, bool Silent, bool EnableUI)
        {
            try
            {
                string DownloadItems = DownloadItemIds.Replace(" ", string.Empty).Replace("tag:google.com,2005:reader/item/", string.Empty);
                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };

                StringContent PostContent = new StringContent(DownloadItems, Encoding.UTF8, "application/x-www-form-urlencoded");
                Uri PostUri = new Uri(ApiConnectionUrl + "stream/items/contents?output=json");

                string PostHttp = await AVDownloader.SendPostRequestAsync(20000, "News Scroll", RequestHeader, PostUri, PostContent);

                return await DownloadToTableItemList(Preload, IgnoreDate, PostHttp, Silent, EnableUI);
            }
            catch
            {
                return false;
            }
        }

        static public async Task<bool> SingleItem(string DownloadId, bool Preload, bool IgnoreDate, bool Silent, bool EnableUI)
        {
            try
            {
                string DownloadItems = DownloadId.Replace(" ", string.Empty).Replace("tag:google.com,2005:reader/item/", string.Empty);
                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };

                Uri DownloadUri = new Uri(ApiConnectionUrl + "stream/items/contents?output=json&i=" + DownloadItems);
                string DownloadString = await AVDownloader.DownloadStringAsync(7500, "News Scroll", RequestHeader, DownloadUri);

                return await DownloadToTableItemList(Preload, IgnoreDate, DownloadString, Silent, EnableUI);
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> DownloadToTableItemList(bool Preload, bool IgnoreDate, string DownloadString, bool Silent, bool EnableUI)
        {
            try
            {
                JObject WebJObject = JObject.Parse(DownloadString);
                if (WebJObject["items"] != null && WebJObject["items"].HasValues)
                {
                    int TotalItemsCount = WebJObject["items"].Count();
                    if (!Silent) { EventProgressDisableUI("Processing " + TotalItemsCount + " items...", true); }
                    Debug.WriteLine("Processing " + TotalItemsCount + " items...");

                    //Check the received item ids
                    int ProcessedItems = 0;
                    int PreloadedItems = AppVariables.ItemsToPreloadBatch;

                    List<TableItems> TableUpdatedItems = new List<TableItems>();
                    List<TableItems> TableCurrentItems = await vSQLConnection.Table<TableItems>().ToListAsync();

                    //Filter un/ignored feeds
                    List<string> IgnoredFeedList = (await vSQLConnection.Table<TableFeeds>().Where(x => x.feed_ignore_status == true).ToListAsync()).Select(x => x.feed_id).ToList();

                    foreach (JToken JTokenRoot in WebJObject["items"]) //.OrderByDescending(obj => (string)obj["crawlTimeMsec"])
                    {
                        string FoundItemId = JTokenRoot["id"].ToString().Replace(" ", string.Empty).Replace("tag:google.com,2005:reader/item/", string.Empty);
                        TableItems TableResult = TableCurrentItems.Where(x => x.item_id == FoundItemId).FirstOrDefault();
                        if (TableResult == null)
                        {
                            //Debug.WriteLine("Adding item: " + FoundItemId);

                            TableResult = JSonToTableItem(FoundItemId, JTokenRoot, IgnoreDate, AppVariables.LoadStarred);
                            if (TableResult != null)
                            {
                                TableUpdatedItems.Add(TableResult);
                                TableCurrentItems.Add(TableResult);
                            }
                        }
                        else
                        {
                            //Debug.WriteLine("Updating item: " + FoundItemId);

                            //Check item status
                            string Categories = string.Empty;
                            try { Categories = JTokenRoot["categories"].ToString(); } catch { }

                            //Read the item
                            if (Categories.Contains("\"user/-/state/com.google/read\""))
                            {
                                TableResult.item_read_status = true;
                            }
                            else
                            {
                                TableResult.item_read_status = false;
                            }

                            //Star the item
                            if (AppVariables.LoadStarred || Categories.Contains("\"user/-/state/com.google/starred\""))
                            {
                                TableResult.item_star_status = true;
                            }
                            else
                            {
                                TableResult.item_star_status = false;
                            }

                            TableUpdatedItems.Add(TableResult);
                        }

                        //Check if items need to be preloaded
                        if (Preload && TableResult != null && PreloadedItems <= AppVariables.ItemsToPreloadMax)
                        {
                            //Validate the processed item
                            if (ProcessItemLoad.ValidateTableItems(IgnoredFeedList, TableResult)) { ProcessedItems++; }

                            //Preload items to the list
                            if (ProcessedItems == PreloadedItems || ProcessedItems == TotalItemsCount)
                            {
                                Debug.WriteLine("Preloading " + AppVariables.ItemsToPreloadBatch + " new items...");
                                await ProcessItemLoad.DatabaseToList(null, TableCurrentItems, AppVariables.CurrentItemsLoaded, AppVariables.ItemsToPreloadBatch, true, false);
                                PreloadedItems += AppVariables.ItemsToPreloadBatch;
                            }
                        }
                    }

                    //Update the items in database
                    if (TableUpdatedItems.Any())
                    {
                        if (!Silent) { EventProgressDisableUI("Updating " + TableUpdatedItems.Count() + " items...", true); }
                        Debug.WriteLine("Updating " + TableUpdatedItems.Count() + " items...");
                        await vSQLConnection.InsertAllAsync(TableUpdatedItems, "OR REPLACE");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed DownloadToTableItemList: " + ex.Message);
                return false;
            }
        }

        private static TableItems JSonToTableItem(string ItemId, JToken JTokenRoot, bool IgnoreDate, bool Starred)
        {
            try
            {
                //Check date before adding
                //DateTime item_date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((double)JTokenRoot["published"]);
                DateTime item_date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds((double)JTokenRoot["crawlTimeMsec"]);
                if (IgnoreDate || item_date > DateTime.UtcNow.AddDays(-Convert.ToDouble(AppSettingLoad("RemoveItemsRange"))))
                {
                    TableItems AddItem = new TableItems();
                    AddItem.item_id = ItemId;
                    AddItem.item_datetime = item_date;

                    //Set the feed id
                    string StreamId = "0";
                    try
                    {
                        StreamId = JTokenRoot["origin"]["streamId"].ToString().Replace(" ", string.Empty).Replace("feed/", string.Empty).Replace("/state/com.google/broadcast", string.Empty);
                    }
                    catch { Debug.WriteLine("Failed to get feed id for item: " + ItemId); }
                    AddItem.item_feed_id = StreamId;

                    //Set an unknown feed title
                    string FeedTitle = "Unknown feed";
                    try
                    {
                        if (StreamId == "0")
                        {
                            FeedTitle = JTokenRoot["origin"]["title"].ToString();
                            AddItem.item_feed_title = FeedTitle;
                        }
                        else if (StreamId.StartsWith("user/"))
                        {
                            FeedTitle = JTokenRoot["origin"]["title"].ToString() + " (" + JTokenRoot["origin"]["feed_title"].ToString() + ")";
                            AddItem.item_feed_title = FeedTitle;
                        }
                    }
                    catch { Debug.WriteLine("Failed to get feed title for item: " + ItemId); }

                    //Set the item title
                    string TitleString = "Unknown title";
                    try
                    {
                        TitleString = WebUtility.HtmlDecode(JTokenRoot["title"].ToString());
                        //TitleString = WebUtility.UrlDecode(TitleString);
                        TitleString = TitleString.Replace("\n", string.Empty).Replace("\r", string.Empty);
                        TitleString = Regex.Replace(TitleString, "<.*?>", string.Empty, RegexOptions.Singleline);
                    }
                    catch { Debug.WriteLine("Failed to get item title for item: " + ItemId); }
                    AddItem.item_title = TitleString;

                    //Set the item author
                    string AuthorString = "Unknown author";
                    try
                    {
                        AuthorString = WebUtility.HtmlDecode(JTokenRoot["author"].ToString());
                        //AuthorString = WebUtility.UrlDecode(AuthorString);
                        AuthorString = AuthorString.Replace("\n", string.Empty).Replace("\r", string.Empty);
                    }
                    catch { Debug.WriteLine("Failed to get item author for item: " + ItemId); }
                    AddItem.item_author = AuthorString;

                    //Set the item link
                    string ItemLinkString = string.Empty;
                    try
                    {
                        ItemLinkString = WebUtility.HtmlDecode(JTokenRoot["canonical"][0]["href"].ToString());
                        ItemLinkString = WebUtility.UrlDecode(ItemLinkString);
                    }
                    catch { Debug.WriteLine("Failed to get item link for item: " + ItemId); }
                    AddItem.item_link = ItemLinkString;

                    //Set the item content
                    string ItemContentString = string.Empty;
                    try
                    {
                        ItemContentString = JTokenRoot["summary"]["content"].ToString();
                        ItemContentString = WebUtility.HtmlDecode(ItemContentString);
                        //ItemContentString = WebUtility.UrlDecode(ItemContentString);
                    }
                    catch { Debug.WriteLine("Failed to get item content for item: " + ItemId); }
                    AddItem.item_content = Process.ProcessItemTextSummary(ItemContentString, false, true);
                    AddItem.item_content_full = ItemContentString;

                    //Check the item image
                    string ItemImageLink = string.Empty;
                    try
                    {
                        //Get the image source link
                        if (JTokenRoot["enclosure"] != null && JTokenRoot["enclosure"][0]["href"] != null && (JTokenRoot["enclosure"][0]["type"].ToString().StartsWith("image") || JTokenRoot["enclosure"][0]["type"].ToString().StartsWith("unknown")))
                        {
                            ItemImageLink = JTokenRoot["enclosure"][0]["href"].ToString();
                        }

                        //Check the image source link
                        if (string.IsNullOrWhiteSpace(ItemImageLink))
                        {
                            ItemImageLink = Process.GetItemHtmlFirstImage(AddItem.item_content_full, string.Empty);
                        }

                        //Decode the image source link
                        ItemImageLink = WebUtility.HtmlDecode(ItemImageLink);
                        ItemImageLink = WebUtility.UrlDecode(ItemImageLink);

                        //Split http(s):// tags from uri
                        if (ItemImageLink.Contains("https://") && ItemImageLink.LastIndexOf("https://") <= 20) { ItemImageLink = ItemImageLink.Substring(ItemImageLink.LastIndexOf("https://")); }
                        if (ItemImageLink.Contains("http://") && ItemImageLink.LastIndexOf("http://") <= 20) { ItemImageLink = ItemImageLink.Substring(ItemImageLink.LastIndexOf("http://")); }

                        //Check if image link is valid
                        if (ItemImageLink.StartsWith("//"))
                        {
                            Debug.WriteLine("Invalid image link, adding: http: to " + ItemImageLink);
                            ItemImageLink = "http:" + ItemImageLink;
                        }
                        else if (ItemImageLink.StartsWith("/"))
                        {
                            if (JTokenRoot["origin"]["htmlUrl"] != null)
                            {
                                Debug.WriteLine("Invalid image link, adding: " + JTokenRoot["origin"]["htmlUrl"].ToString() + " to " + ItemImageLink);
                                ItemImageLink = JTokenRoot["origin"]["htmlUrl"].ToString() + ItemImageLink;
                            }
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("Failed to get item image for item: " + ItemId);
                    }
                    AddItem.item_image = ItemImageLink;

                    //Check item status
                    string Categories = string.Empty;
                    try { Categories = JTokenRoot["categories"].ToString(); } catch { Debug.WriteLine("Failed to check categories for item: " + ItemId); }

                    //Read the item
                    if (Categories.Contains("\"user/-/state/com.google/read\""))
                    {
                        AddItem.item_read_status = true;
                    }
                    else
                    {
                        AddItem.item_read_status = false;
                    }

                    //Star the item
                    if (Starred || Categories.Contains("\"user/-/state/com.google/starred\""))
                    {
                        AddItem.item_star_status = true;
                    }
                    else
                    {
                        AddItem.item_star_status = false;
                    }

                    return AddItem;
                }
                else
                {
                    //Debug.WriteLine("Invalid process item: " + ItemId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to process item: " + ItemId + "/" + ex.Message);
                return null;
            }
        }
    }
}