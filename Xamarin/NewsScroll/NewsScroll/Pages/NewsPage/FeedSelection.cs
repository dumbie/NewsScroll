﻿using ArnoldVinkCode;
using NewsScroll.Classes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppVariables;
using static NewsScroll.Database.Database;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class NewsPage
    {
        //Change the selection feed
        void ChangeSelectionFeed(Feeds SelectFeed, bool Switch)
        {
            try
            {
                //Check if selection is a folder
                if (SelectFeed.feed_folder_status)
                {
                    Debug.WriteLine("Changing the selection folder to: " + string.Join(", ", SelectFeed.feed_folder_ids) + " (TotalFeeds: " + List_FeedSelect.Count() + "/Switch: " + Switch + ")");
                    vNewsFeed = SelectFeed;
                    vPreviousScrollItem = 0;
                }
                else
                {
                    Debug.WriteLine("Changing the selection feed to: " + SelectFeed.feed_id + " (Total: " + List_FeedSelect.Count() + "/Switch: " + Switch + ")");
                    Feeds TargetFeed = List_FeedSelect.Where(x => x.feed_id == SelectFeed.feed_id).FirstOrDefault();
                    if (TargetFeed != null)
                    {
                        if (Switch)
                        {
                            vNewsFeed = TargetFeed;
                            vPreviousScrollItem = 0;
                            combobox_FeedSelection.SelectedItem = TargetFeed;
                        }
                        else
                        {
                            combobox_FeedSelection.SelectedIndexChanged -= combobox_FeedSelection_SelectionChanged;
                            vNewsFeed = TargetFeed;
                            vPreviousScrollItem = 0;
                            combobox_FeedSelection.SelectedItem = TargetFeed;
                            combobox_FeedSelection.SelectedIndexChanged += combobox_FeedSelection_SelectionChanged;
                        }
                    }
                    else
                    {
                        Feeds TempFeed = new Feeds();
                        TempFeed.feed_id = "0";

                        ChangeSelectionFeed(TempFeed, false);
                    }
                }
            }
            catch { }
        }

        //Load feeds in selection
        async Task LoadSelectionFeeds(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { ProgressDisableUI("Loading selection feeds...", true); }
                Debug.WriteLine("Loading selection feeds, silent: " + Silent);

                combobox_FeedSelection.IsEnabled = false;
                combobox_FeedSelection.Opacity = 0.30;
                await ClearObservableCollection(List_FeedSelect);

                //Check if received lists are empty
                if (LoadTableFeeds == null) { LoadTableFeeds = await vSQLConnection.Table<TableFeeds>().OrderBy(x => x.feed_folder).ToListAsync(); }
                if (LoadTableItems == null) { LoadTableItems = await vSQLConnection.Table<TableItems>().ToListAsync(); }

                //Filter un/ignored feeds
                List<string> IgnoredFeedList = LoadTableFeeds.Where(x => x.feed_ignore_status == true).Select(x => x.feed_id).ToList();
                List<TableFeeds> UnignoredFeedList = LoadTableFeeds.Where(x => x.feed_ignore_status == false).ToList();
                Feeds TempFeed = new Feeds();

                //Add all feeds selection
                TempFeed.feed_id = "0";
                int TotalItemsAll = ProcessItemLoad.FilterNewsItems(IgnoredFeedList, LoadTableItems, TempFeed, 0, AppVariables.ItemsToLoadMax).Count();
                Feeds FeedItemAll = new Feeds();
                FeedItemAll.feed_icon = ImageSource.FromResource("NewsScroll.Assets.iconRSS-Dark.png");
                FeedItemAll.feed_title = "All news items";
                FeedItemAll.feed_item_count = TotalItemsAll;
                FeedItemAll.feed_collection_status = true;
                FeedItemAll.feed_id = "0";
                List_FeedSelect.Add(FeedItemAll);

                //Add unread feeds selection
                TempFeed.feed_id = "2";
                int TotalItemsUnread = ProcessItemLoad.FilterNewsItems(IgnoredFeedList, LoadTableItems, TempFeed, 0, AppVariables.ItemsToLoadMax).Count();
                Feeds FeedItemUnread = new Feeds();
                FeedItemUnread.feed_icon = ImageSource.FromResource("NewsScroll.Assets.iconRSS-Dark.png");
                FeedItemUnread.feed_title = "Unread news items";
                FeedItemUnread.feed_item_count = TotalItemsUnread;
                FeedItemUnread.feed_collection_status = true;
                FeedItemUnread.feed_id = "2";
                List_FeedSelect.Add(FeedItemUnread);

                //Add read feeds selection
                TempFeed.feed_id = "1";
                int TotalItemsRead = ProcessItemLoad.FilterNewsItems(IgnoredFeedList, LoadTableItems, TempFeed, 0, AppVariables.ItemsToLoadMax).Count();
                Feeds FeedItemRead = new Feeds();
                FeedItemRead.feed_icon = ImageSource.FromResource("NewsScroll.Assets.iconRSS-Dark.png");
                FeedItemRead.feed_title = "Read news items";
                FeedItemRead.feed_item_count = TotalItemsRead;
                FeedItemRead.feed_collection_status = true;
                FeedItemRead.feed_id = "1";
                List_FeedSelect.Add(FeedItemRead);

                //Feeds that are not ignored and contain items
                foreach (TableFeeds Feed in UnignoredFeedList)
                {
                    TempFeed.feed_id = Feed.feed_id;
                    int TotalItems = ProcessItemLoad.FilterNewsItems(IgnoredFeedList, LoadTableItems, TempFeed, 0, AppVariables.ItemsToLoadMax).Count();
                    if (TotalItems > 0)
                    {
                        //Add folder
                        string FeedFolder = Feed.feed_folder;
                        if (string.IsNullOrWhiteSpace(FeedFolder)) { FeedFolder = "No folder"; }
                        Feeds FolderUpdate = List_FeedSelect.Where(x => x.feed_folder_title == FeedFolder && x.feed_folder_status).FirstOrDefault();
                        if (FolderUpdate == null)
                        {
                            //Load folder icon
                            ImageSource FolderIcon = ImageSource.FromResource("NewsScroll.Assets.iconFolder-Dark.png");

                            //Add folder
                            Feeds FolderItem = new Feeds();
                            FolderItem.feed_icon = FolderIcon;
                            FolderItem.feed_title = "(Folder) " + FeedFolder;
                            FolderItem.feed_folder_title = FeedFolder;
                            FolderItem.feed_folder_status = true;
                            List_FeedSelect.Add(FolderItem);
                            //Debug.WriteLine("Added folder...");
                        }

                        //Add feed
                        //Load feed icon
                        ImageSource FeedIcon = null;
                        if (Feed.feed_id.StartsWith("user/"))
                        {
                            FeedIcon = ImageSource.FromResource("NewsScroll.Assets.iconUser-Dark.png");
                        }
                        else
                        {
                            FeedIcon = AVFiles.File_LoadImage(Feed.feed_id + ".png", true);
                        }
                        if (FeedIcon == null)
                        {
                            FeedIcon = ImageSource.FromResource("NewsScroll.Assets.iconRSS-Dark.png");
                        }

                        //Get the current feed item count
                        Feeds FeedItem = new Feeds();
                        FeedItem.feed_icon = FeedIcon;
                        FeedItem.feed_title = Feed.feed_title;
                        FeedItem.feed_item_count = TotalItems;
                        FeedItem.feed_id = Feed.feed_id;
                        List_FeedSelect.Add(FeedItem);

                        //Update folder
                        FolderUpdate = List_FeedSelect.Where(x => x.feed_folder_title == FeedFolder && x.feed_folder_status).FirstOrDefault();
                        if (FolderUpdate != null)
                        {
                            FolderUpdate.feed_folder_ids.Add(Feed.feed_id);
                            FolderUpdate.feed_item_count = FolderUpdate.feed_item_count + FeedItem.feed_item_count;
                            //Debug.WriteLine("Updated folder...");
                        }
                    }
                }

                combobox_FeedSelection.IsEnabled = true;
                combobox_FeedSelection.Opacity = 1;
            }
            catch { }
            if (EnableUI) { ProgressEnableUI(); }
        }

        //Update feeds in selection
        async Task UpdateSelectionFeeds(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { ProgressDisableUI("Updating selection feeds...", true); }
                Debug.WriteLine("Updating selection feeds, silent: " + Silent);

                combobox_FeedSelection.IsEnabled = false;
                combobox_FeedSelection.Opacity = 0.30;

                //Check if received lists are empty
                if (LoadTableFeeds == null) { LoadTableFeeds = await vSQLConnection.Table<TableFeeds>().ToListAsync(); }
                if (LoadTableItems == null) { LoadTableItems = await vSQLConnection.Table<TableItems>().ToListAsync(); }

                //Filter un/ignored feeds
                List<string> IgnoredFeedList = LoadTableFeeds.Where(x => x.feed_ignore_status == true).Select(x => x.feed_id).ToList();

                //Update the currently loaded feeds
                foreach (Feeds FeedUpdate in List_FeedSelect.Where(x => !x.feed_folder_status))
                {
                    Feeds TempFeed = new Feeds();
                    TempFeed.feed_id = FeedUpdate.feed_id;

                    FeedUpdate.feed_item_count = ProcessItemLoad.FilterNewsItems(IgnoredFeedList, LoadTableItems, TempFeed, 0, AppVariables.ItemsToLoadMax).Count();
                }

                //Reset the loaded folders item count
                foreach (Feeds FolderReset in List_FeedSelect.Where(x => x.feed_folder_status)) { FolderReset.feed_item_count = 0; }

                //Update the currently loaded folders
                foreach (Feeds FolderUpdate in List_FeedSelect.Where(x => x.feed_folder_status))
                {
                    foreach (string FeedId in FolderUpdate.feed_folder_ids)
                    {
                        Feeds Feed = List_FeedSelect.Where(x => x.feed_id == FeedId).FirstOrDefault();
                        if (Feed != null && Feed.feed_item_count > 0)
                        {
                            FolderUpdate.feed_item_count = FolderUpdate.feed_item_count + Feed.feed_item_count;
                            //Debug.WriteLine("Added folder count: " + Feed.feed_item_count);
                        }
                    }
                }

                //Remove empty feeds and folders from combobox
                bool FeedFolderRemoved = false;
                if (!(bool)AppSettingLoad("DisplayReadMarkedItems"))
                {
                    foreach (Feeds Feed in List_FeedSelect.ToList())
                    {
                        if (Feed.feed_item_count == 0 && !Feed.feed_collection_status)
                        {
                            Debug.WriteLine("Removing feed or folder: " + Feed.feed_title + Feed.feed_folder_title + " from the list.");
                            List_FeedSelect.Remove(Feed);
                            FeedFolderRemoved = true;
                        }
                    }
                }

                //Check if selected feed has been removed and set to read items feed
                if (FeedFolderRemoved && combobox_FeedSelection.SelectedIndex == -1 || (combobox_FeedSelection.SelectedIndex == 0 && vNewsFeed.feed_item_count == 0))
                {
                    Feeds TempFeed = new Feeds();
                    TempFeed.feed_id = "1";

                    //Change the selection feed
                    ChangeSelectionFeed(TempFeed, false);

                    //Load all the items
                    await LoadItems(false, false);
                }
                else
                {
                    //Update the total item count
                    UpdateTotalItemsCount();
                }

                combobox_FeedSelection.IsEnabled = true;
                combobox_FeedSelection.Opacity = 1;
            }
            catch { }
            if (EnableUI) { ProgressEnableUI(); }
        }
    }
}