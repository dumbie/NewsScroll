using ArnoldVinkCode;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> Feeds(bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { await EventProgressDisableUI("Downloading latest feeds...", true); }
                System.Diagnostics.Debug.WriteLine("Downloading latest feeds...");

                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };
                string DownloadString = await AVDownloader.DownloadStringAsync(10000, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "subscription/list?output=json"));

                JObject WebJObject = JObject.Parse(DownloadString);
                if (WebJObject["subscriptions"] != null && WebJObject["subscriptions"].HasValues)
                {
                    if (!Silent) { await EventProgressDisableUI("Processing " + WebJObject["subscriptions"].Count() + " feeds...", true); }
                    System.Diagnostics.Debug.WriteLine("Processing " + WebJObject["subscriptions"].Count() + " feeds...");

                    List<string> ApiFeedIdList = new List<string>();
                    IReadOnlyList<IStorageItem> LocalFileList = await ApplicationData.Current.LocalFolder.GetItemsAsync();

                    List<TableFeeds> TableUpdatedFeeds = new List<TableFeeds>();
                    List<TableFeeds> TableCurrentFeeds = await SQLConnection.Table<TableFeeds>().ToListAsync();

                    foreach (JToken JTokenRoot in WebJObject["subscriptions"])
                    {
                        string FeedId = JTokenRoot["sortid"].ToString();
                        string FeedTitle = JTokenRoot["title"].ToString();

                        string HtmlUrl = WebUtility.HtmlDecode(JTokenRoot["htmlUrl"].ToString());
                        HtmlUrl = WebUtility.UrlDecode(HtmlUrl);

                        Uri FullUrl = new Uri(HtmlUrl);
                        ApiFeedIdList.Add(FeedId);

                        TableFeeds TableResult = TableCurrentFeeds.Where(x => x.feed_id == FeedId).FirstOrDefault();
                        if (TableResult == null)
                        {
                            //System.Diagnostics.Debug.WriteLine("Adding feed: " + FeedTitle);

                            TableFeeds AddFeed = new TableFeeds();
                            AddFeed.feed_id = FeedId;
                            AddFeed.feed_title = FeedTitle;
                            AddFeed.feed_link = FullUrl.Scheme + "://" + FullUrl.Host;

                            AddFeed.feed_ignore_status = false;
                            if (JTokenRoot["categories"] != null && JTokenRoot["categories"].HasValues)
                            {
                                AddFeed.feed_folder = JTokenRoot["categories"][0]["label"].ToString();
                            }

                            TableUpdatedFeeds.Add(AddFeed);
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Updating feed: " + FeedTitle);

                            TableResult.feed_title = FeedTitle;
                            TableResult.feed_link = FullUrl.Scheme + "://" + FullUrl.Host;

                            if (JTokenRoot["categories"] != null && JTokenRoot["categories"].HasValues)
                            {
                                TableResult.feed_folder = JTokenRoot["categories"][0]["label"].ToString();
                            }

                            TableUpdatedFeeds.Add(TableResult);
                        }

                        //Check and download feed logo
                        if (!LocalFileList.Any(x => x.Name == FeedId + ".png"))
                        {
                            try
                            {
                                if (!Silent) { await EventProgressDisableUI("Downloading " + FeedTitle + " icon...", true); }

                                Uri IconUrl = new Uri("https://s2.googleusercontent.com/s2/favicons?domain=" + FullUrl.Host);
                                byte[] HttpFeedIcon = await AVDownloader.DownloadByteAsync(3000, "News Scroll", null, IconUrl);
                                if (HttpFeedIcon != null && HttpFeedIcon.Length > 75)
                                {
                                    await AVFile.SaveBytes(FeedId + ".png", HttpFeedIcon);
                                    System.Diagnostics.Debug.WriteLine("Downloaded transparent logo: " + HttpFeedIcon.Length + "bytes/" + IconUrl);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("No logo found for: " + IconUrl);
                                }
                            }
                            catch { }
                        }
                    }

                    //Update the feeds in database
                    if (TableUpdatedFeeds.Any())
                    {
                        await SQLConnection.InsertAllAsync(TableUpdatedFeeds, "OR REPLACE");
                    }

                    //Delete removed feeds from the database
                    List<string> DeletedFeeds = TableCurrentFeeds.Select(x => x.feed_id).Except(ApiFeedIdList).ToList();
                    System.Diagnostics.Debug.WriteLine("Found deleted feeds: " + DeletedFeeds.Count());
                    foreach (string DeleteFeedId in DeletedFeeds) { await DeleteFeed(DeleteFeedId); }
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