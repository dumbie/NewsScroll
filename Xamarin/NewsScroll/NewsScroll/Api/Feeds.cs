﻿using ArnoldVinkCode;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> Feeds(bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { EventProgressDisableUI("Downloading latest feeds...", true); }
                Debug.WriteLine("Downloading latest feeds...");

                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };
                string DownloadString = await AVDownloader.DownloadStringAsync(10000, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "subscription/list?output=json"));

                JObject WebJObject = JObject.Parse(DownloadString);
                if (WebJObject["subscriptions"] != null && WebJObject["subscriptions"].HasValues)
                {
                    if (!Silent) { EventProgressDisableUI("Processing " + WebJObject["subscriptions"].Count() + " feeds...", true); }
                    Debug.WriteLine("Processing " + WebJObject["subscriptions"].Count() + " feeds...");

                    List<string> ApiFeedIdList = new List<string>();
                    string[] LocalFileList = AVFiles.Directory_ListFiles(string.Empty, true);

                    List<TableFeeds> TableUpdatedFeeds = new List<TableFeeds>();
                    List<TableFeeds> TableCurrentFeeds = await vSQLConnection.Table<TableFeeds>().ToListAsync();

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
                            //Debug.WriteLine("Adding feed: " + FeedTitle);

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
                            //Debug.WriteLine("Updating feed: " + FeedTitle);

                            TableResult.feed_title = FeedTitle;
                            TableResult.feed_link = FullUrl.Scheme + "://" + FullUrl.Host;

                            if (JTokenRoot["categories"] != null && JTokenRoot["categories"].HasValues)
                            {
                                TableResult.feed_folder = JTokenRoot["categories"][0]["label"].ToString();
                            }

                            TableUpdatedFeeds.Add(TableResult);
                        }

                        //Check and download feed logo
                        if (!LocalFileList.Any(x => x.EndsWith(FeedId + ".png")))
                        {
                            try
                            {
                                if (!Silent) { EventProgressDisableUI("Downloading " + FeedTitle + " icon...", true); }

                                Uri IconUrl = new Uri("https://s2.googleusercontent.com/s2/favicons?domain=" + FullUrl.Host);
                                byte[] HttpFeedIcon = await AVDownloader.DownloadByteAsync(3000, "News Scroll", null, IconUrl);
                                if (HttpFeedIcon != null && HttpFeedIcon.Length > 75)
                                {
                                    AVFiles.File_SaveBytes(FeedId + ".png", HttpFeedIcon, true, true);
                                    Debug.WriteLine("Downloaded transparent logo: " + HttpFeedIcon.Length + "bytes/" + IconUrl);
                                }
                                else
                                {
                                    Debug.WriteLine("No logo found for: " + IconUrl);
                                }
                            }
                            catch { }
                        }
                    }

                    //Update the feeds in database
                    if (TableUpdatedFeeds.Any())
                    {
                        await vSQLConnection.InsertAllAsync(TableUpdatedFeeds, "OR REPLACE");
                    }

                    //Delete removed feeds from the database
                    List<string> DeletedFeeds = TableCurrentFeeds.Select(x => x.feed_id).Except(ApiFeedIdList).ToList();
                    Debug.WriteLine("Found deleted feeds: " + DeletedFeeds.Count());
                    foreach (string DeleteFeedId in DeletedFeeds) { await DeleteFeed(DeleteFeedId); }
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