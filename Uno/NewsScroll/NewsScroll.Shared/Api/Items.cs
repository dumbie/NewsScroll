using ArnoldVinkCode;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> AllNewsItems(bool IgnoreDate, bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { await EventProgressDisableUI("Downloading latest items...", true); }
                System.Diagnostics.Debug.WriteLine("Downloading latest items...");

                //Date time calculations
                long UnixTimeTicks = 0;
                DateTime RemoveItemsRange = DateTime.UtcNow.AddDays(-Convert.ToDouble(AppVariables.ApplicationSettings["RemoveItemsRange"]));
                if (AppVariables.ApplicationSettings["LastItemsUpdate"].ToString() == "Never")
                {
                    UnixTimeTicks = (RemoveItemsRange.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000000; //Second
                }
                else
                {
                    UnixTimeTicks = ((Convert.ToDateTime(AppVariables.ApplicationSettings["LastItemsUpdate"], AppVariables.CultureInfoEnglish).AddMinutes(-15)).Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000000; //Second
                }

                //Set the last update time string
                string LastUpdate = DateTime.UtcNow.ToString(AppVariables.CultureInfoEnglish);

                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };
                string DownloadString = await AVDownloader.DownloadStringAsync(20000, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "stream/contents?output=json&n=" + AppVariables.ItemsMaximumLoad + "&ot=" + UnixTimeTicks));

                bool UpdatedItems = await DownloadToTableItemList(IgnoreDate, DownloadString, Silent, EnableUI);
                if (UpdatedItems)
                {
                    //Save the last update time string
                    AppVariables.ApplicationSettings["LastItemsUpdate"] = LastUpdate;

                    if (EnableUI) { await EventProgressEnableUI(); }
                    return true;
                }
                else
                {
                    await EventProgressEnableUI();
                    return false;
                }
            }
            catch
            {
                await EventProgressEnableUI();
                return false;
            }
        }

        static public async Task<bool> MultipleItems(string DownloadItemIds, bool IgnoreDate, bool Silent, bool EnableUI)
        {
            try
            {
                string DownloadItems = DownloadItemIds.Replace(" ", string.Empty).Replace("tag:google.com,2005:reader/item/", string.Empty);
                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };

                StringContent PostContent = new StringContent(DownloadItems, Encoding.UTF8, "application/x-www-form-urlencoded");
                Uri PostUri = new Uri(ApiConnectionUrl + "stream/items/contents?output=json");
                string PostHttp = await AVDownloader.SendPostRequestAsync(20000, "News Scroll", RequestHeader, PostUri, PostContent);

                return await DownloadToTableItemList(IgnoreDate, PostHttp, Silent, EnableUI);
            }
            catch { return false; }
        }

        static public async Task<bool> SingleItem(string DownloadId, bool IgnoreDate, bool Silent, bool EnableUI)
        {
            try
            {
                string DownloadItems = DownloadId.Replace(" ", string.Empty).Replace("tag:google.com,2005:reader/item/", string.Empty);
                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };

                Uri DownloadUri = new Uri(ApiConnectionUrl + "stream/items/contents?output=json&i=" + DownloadItems);
                string DownloadString = await AVDownloader.DownloadStringAsync(7500, "News Scroll", RequestHeader, DownloadUri);

                return await DownloadToTableItemList(IgnoreDate, DownloadString, Silent, EnableUI);
            }
            catch { return false; }
        }

        private static async Task<bool> DownloadToTableItemList(bool IgnoreDate, string DownloadString, bool Silent, bool EnableUI)
        {
            try
            {
                JObject WebJObject = JObject.Parse(DownloadString);
                if (WebJObject["items"] != null && WebJObject["items"].HasValues)
                {
                    int TotalItemsCount = WebJObject["items"].Count();
                    if (!Silent) { await EventProgressDisableUI("Processing " + TotalItemsCount + " items...", true); }
                    System.Diagnostics.Debug.WriteLine("Processing " + TotalItemsCount + " items...");

                    List<TableItems> TableUpdatedItems = new List<TableItems>();
                    List<TableItems> TableCurrentItems = await SQLConnection.Table<TableItems>().ToListAsync();

                    //Filter un/ignored feeds
                    List<string> IgnoredFeedList = (await SQLConnection.Table<TableFeeds>().Where(x => x.feed_ignore_status == true).ToListAsync()).Select(x => x.feed_id).ToList();

                    foreach (JToken JTokenRoot in WebJObject["items"]) //.OrderByDescending(obj => (string)obj["crawlTimeMsec"])
                    {
                        string FoundItemId = JTokenRoot["id"].ToString().Replace(" ", string.Empty).Replace("tag:google.com,2005:reader/item/", string.Empty);
                        TableItems TableResult = TableCurrentItems.Where(x => x.item_id == FoundItemId).FirstOrDefault();
                        if (TableResult == null)
                        {
                            //System.Diagnostics.Debug.WriteLine("Adding item: " + FoundItemId);

                            TableResult = JSonToTableItem(FoundItemId, JTokenRoot, IgnoreDate, AppVariables.LoadStarred);
                            if (TableResult != null)
                            {
                                TableUpdatedItems.Add(TableResult);
                                TableCurrentItems.Add(TableResult);
                            }
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Updating item: " + FoundItemId);

                            //Check item status
                            string Categories = string.Empty;
                            try { Categories = JTokenRoot["categories"].ToString(); } catch { }

                            //Read the item
                            if (Categories.Contains("\"user/-/state/com.google/read\"")) { TableResult.item_read_status = true; }
                            else { TableResult.item_read_status = false; }

                            //Star the item
                            if (AppVariables.LoadStarred || Categories.Contains("\"user/-/state/com.google/starred\"")) { TableResult.item_star_status = true; }
                            else { TableResult.item_star_status = false; }

                            TableUpdatedItems.Add(TableResult);
                        }
                    }

                    //Update the items in database
                    if (TableUpdatedItems.Any())
                    {
                        if (!Silent) { await EventProgressDisableUI("Updating " + TableUpdatedItems.Count() + " items...", true); }
                        System.Diagnostics.Debug.WriteLine("Updating " + TableUpdatedItems.Count() + " items...");

                        await SQLConnection.InsertAllAsync(TableUpdatedItems, "OR REPLACE");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed DownloadToTableItemList: " + ex.Message);
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
                if (IgnoreDate || item_date > DateTime.UtcNow.AddDays(-Convert.ToDouble(AppVariables.ApplicationSettings["RemoveItemsRange"])))
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
                    catch { System.Diagnostics.Debug.WriteLine("Failed to get feed id for item: " + ItemId); }
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
                    catch { System.Diagnostics.Debug.WriteLine("Failed to get feed title for item: " + ItemId); }

                    //Set the item title
                    string TitleString = "Unknown title";
                    try
                    {
                        TitleString = WebUtility.HtmlDecode(JTokenRoot["title"].ToString());
                        //TitleString = WebUtility.UrlDecode(TitleString);
                        TitleString = TitleString.Replace("\n", string.Empty).Replace("\r", string.Empty);
                        TitleString = Regex.Replace(TitleString, "<.*?>", string.Empty, RegexOptions.Singleline);
                    }
                    catch { System.Diagnostics.Debug.WriteLine("Failed to get item title for item: " + ItemId); }
                    AddItem.item_title = TitleString;

                    //Set the item author
                    string AuthorString = "Unknown author";
                    try
                    {
                        AuthorString = WebUtility.HtmlDecode(JTokenRoot["author"].ToString());
                        //AuthorString = WebUtility.UrlDecode(AuthorString);
                        AuthorString = AuthorString.Replace("\n", string.Empty).Replace("\r", string.Empty);
                    }
                    catch { System.Diagnostics.Debug.WriteLine("Failed to get item author for item: " + ItemId); }
                    AddItem.item_author = AuthorString;

                    //Set the item link
                    string ItemLinkString = string.Empty;
                    try
                    {
                        ItemLinkString = WebUtility.HtmlDecode(JTokenRoot["canonical"][0]["href"].ToString());
                        ItemLinkString = WebUtility.UrlDecode(ItemLinkString);
                    }
                    catch { System.Diagnostics.Debug.WriteLine("Failed to get item link for item: " + ItemId); }
                    AddItem.item_link = ItemLinkString;

                    //Set the item content
                    string ItemContentString = string.Empty;
                    try
                    {
                        ItemContentString = JTokenRoot["summary"]["content"].ToString();
                        ItemContentString = WebUtility.HtmlDecode(ItemContentString);
                        //ItemContentString = WebUtility.UrlDecode(ItemContentString);
                    }
                    catch { System.Diagnostics.Debug.WriteLine("Failed to get item content for item: " + ItemId); }
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
                        else
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
                            System.Diagnostics.Debug.WriteLine("Invalid image link, adding: http: to " + ItemImageLink);
                            ItemImageLink = "http:" + ItemImageLink;
                        }
                        else if (ItemImageLink.StartsWith("/"))
                        {
                            if (JTokenRoot["origin"]["htmlUrl"] != null)
                            {
                                System.Diagnostics.Debug.WriteLine("Invalid image link, adding: " + JTokenRoot["origin"]["htmlUrl"].ToString() + " to " + ItemImageLink);
                                ItemImageLink = JTokenRoot["origin"]["htmlUrl"].ToString() + ItemImageLink;
                            }
                        }
                    }
                    catch { System.Diagnostics.Debug.WriteLine("Failed to get item image for item: " + ItemId); }
                    AddItem.item_image = ItemImageLink;

                    //Check item status
                    string Categories = string.Empty;
                    try { Categories = JTokenRoot["categories"].ToString(); } catch { System.Diagnostics.Debug.WriteLine("Failed to check categories for item: " + ItemId); }

                    //Read the item
                    if (Categories.Contains("\"user/-/state/com.google/read\"")) { AddItem.item_read_status = true; }
                    else { AddItem.item_read_status = false; }

                    //Star the item
                    if (Starred || Categories.Contains("\"user/-/state/com.google/starred\"")) { AddItem.item_star_status = true; }
                    else { AddItem.item_star_status = false; }

                    return AddItem;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("Invalid process item: " + ItemId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to process item: " + ItemId + "/" + ex.Message);
                return null;
            }
        }
    }
}