using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    class ProcessItemLoad
    {
        //Load items from database to list
        public static async Task DatabaseToList(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, int SkipItems, int NumberOfItems, bool Silent, bool EnableUI)
        {
            try
            {
                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                //Load items from the table
                if (AppVariables.LoadNews)
                {
                    if (!Silent) { await EventProgressDisableUI("Checking news items...", true); }
                    System.Diagnostics.Debug.WriteLine("Checking news items...");

                    //Check if received lists are empty
                    if (LoadTableFeeds == null) { LoadTableFeeds = await SQLConnection.Table<TableFeeds>().ToListAsync(); }
                    if (LoadTableItems == null) { LoadTableItems = await SQLConnection.Table<TableItems>().ToListAsync(); }

                    //Filter un/ignored feeds
                    List<string> IgnoredFeedList = LoadTableFeeds.Where(x => x.feed_ignore_status == true).Select(x => x.feed_id).ToList();
                    LoadTableItems = FilterNewsItems(IgnoredFeedList, LoadTableItems, NewsPage.vCurrentLoadingFeedFolder, SkipItems, NumberOfItems);

                    if (!Silent) { await EventProgressDisableUI("Loading " + LoadTableItems.Count() + " news items...", true); }
                    System.Diagnostics.Debug.WriteLine("Loading " + LoadTableItems.Count() + ", skipped " + SkipItems + " loaded news items...");

                    await Process.ProcessTableItemsToList(List_NewsItems, LoadTableItems, false, false, false);
                }
                else if (AppVariables.LoadStarred)
                {
                    if (!Silent) { await EventProgressDisableUI("Checking starred items...", true); }
                    System.Diagnostics.Debug.WriteLine("Checking starred items...");

                    //Check if received lists are empty
                    if (LoadTableItems == null) { LoadTableItems = await SQLConnection.Table<TableItems>().ToListAsync(); }

                    //Filter starred items
                    LoadTableItems = LoadTableItems.Where(x => x.item_star_status == true).OrderByDescending(x => x.item_datetime).Skip(SkipItems).Take(NumberOfItems).ToList();

                    if (!Silent) { await EventProgressDisableUI("Loading " + LoadTableItems.Count() + " starred items...", true); }
                    System.Diagnostics.Debug.WriteLine("Loading " + LoadTableItems.Count() + ", skipped " + SkipItems + " loaded starred items...");

                    await Process.ProcessTableItemsToList(List_StarredItems, LoadTableItems, false, false, false);
                }
                else if (AppVariables.LoadSearch)
                {
                    if (!Silent) { await EventProgressDisableUI("Searching for: " + SearchPage.vSearchTerm, true); }
                    System.Diagnostics.Debug.WriteLine("Searching for: " + SearchPage.vSearchTerm);

                    //Check if received lists are empty
                    if (LoadTableFeeds == null) { LoadTableFeeds = await SQLConnection.Table<TableFeeds>().ToListAsync(); }
                    if (LoadTableItems == null) { LoadTableItems = await SQLConnection.Table<TableItems>().ToListAsync(); }

                    //Filter un/ignored feeds
                    List<string> UnignoredFeedList = LoadTableFeeds.Where(x => x.feed_ignore_status == false).Select(x => x.feed_id).ToList();

                    //Get the search items by feed or folder
                    if (SearchPage.vSearchFeed.feed_folder_status)
                    {
                        List<string> SearchFolders = SearchPage.vSearchFeed.feed_folder_ids;
                        System.Diagnostics.Debug.WriteLine("Search in folders: " + string.Join(", ", SearchFolders));
                        LoadTableItems = LoadTableItems.Where(x => x.item_title.ToLower().Contains(SearchPage.vSearchTerm.ToLower()) && SearchFolders.Any(y => y == x.item_feed_id) && UnignoredFeedList.Any(y => y == x.item_feed_id)).ToList();
                    }
                    else
                    {
                        string FeedId = SearchPage.vSearchFeed.feed_id;
                        if (FeedId != "0")
                        {
                            System.Diagnostics.Debug.WriteLine("Search in feed: " + FeedId);
                            LoadTableItems = LoadTableItems.Where(x => x.item_title.ToLower().Contains(SearchPage.vSearchTerm.ToLower()) && x.item_feed_id == FeedId && UnignoredFeedList.Any(y => y == x.item_feed_id)).ToList();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Search in all items.");
                            LoadTableItems = LoadTableItems.Where(x => x.item_title.ToLower().Contains(SearchPage.vSearchTerm.ToLower()) && UnignoredFeedList.Any(y => y == x.item_feed_id)).ToList();
                        }
                    }

                    //Search items in table
                    LoadTableItems = LoadTableItems.OrderByDescending(x => x.item_datetime).Skip(SkipItems).Take(NumberOfItems).ToList();

                    //if (!Silent) { await EventProgressDisableUI("Loading " + LoadTableItems.Count() + " found items...", true); }
                    System.Diagnostics.Debug.WriteLine("Loading " + LoadTableItems.Count() + ", skipped " + SkipItems + " found items...");

                    await Process.ProcessTableItemsToList(List_SearchItems, LoadTableItems, false, false, false);
                }
                else if (AppVariables.LoadFeeds)
                {
                    if (!Silent) { await EventProgressDisableUI("Checking feeds...", true); }
                    System.Diagnostics.Debug.WriteLine("Checking feeds...");

                    //Check if received lists are empty
                    if (LoadTableFeeds == null) { LoadTableFeeds = await SQLConnection.Table<TableFeeds>().ToListAsync(); }

                    if (!Silent) { await EventProgressDisableUI("Loading " + LoadTableFeeds.Count() + " feeds...", true); }
                    System.Diagnostics.Debug.WriteLine("Loading " + LoadTableFeeds.Count() + " feeds...");

                    await Process.ProcessTableFeedsToList(List_Feeds, LoadTableFeeds);
                }

                if (EnableUI) { await EventProgressEnableUI(); }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed loading items: " + ex.Message);
                await EventProgressEnableUI();
            }
        }

        //Count items from database
        public static async Task<int> DatabaseToCount(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, bool Silent, bool EnableUI)
        {
            int FoundItems = 0;
            try
            {
                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                //Load items from the table
                if (AppVariables.LoadNews)
                {
                    if (!Silent) { await EventProgressDisableUI("Counting news items...", true); }
                    System.Diagnostics.Debug.WriteLine("Counting news items...");

                    //Check if received lists are empty
                    if (LoadTableFeeds == null) { LoadTableFeeds = await SQLConnection.Table<TableFeeds>().ToListAsync(); }
                    if (LoadTableItems == null) { LoadTableItems = await SQLConnection.Table<TableItems>().ToListAsync(); }

                    //Filter un/ignored feeds
                    List<string> IgnoredFeedList = LoadTableFeeds.Where(x => x.feed_ignore_status == true).Select(x => x.feed_id).ToList();
                    FoundItems = FilterNewsItems(IgnoredFeedList, LoadTableItems, NewsPage.vCurrentLoadingFeedFolder, 0, AppVariables.ItemsMaximumLoad).Count();
                }
                else if (AppVariables.LoadStarred)
                {
                    if (!Silent) { await EventProgressDisableUI("Counting starred items...", true); }
                    System.Diagnostics.Debug.WriteLine("Counting starred items...");

                    //Check if received lists are empty
                    if (LoadTableItems == null) { LoadTableItems = await SQLConnection.Table<TableItems>().ToListAsync(); }

                    //Filter starred items
                    FoundItems = LoadTableItems.Where(x => x.item_star_status == true).Count();
                }
                else if (AppVariables.LoadSearch)
                {
                    if (!Silent) { await EventProgressDisableUI("Counting search items...", true); }
                    System.Diagnostics.Debug.WriteLine("Counting search items...");

                    //Check if received lists are empty
                    if (LoadTableFeeds == null) { LoadTableFeeds = await SQLConnection.Table<TableFeeds>().ToListAsync(); }
                    if (LoadTableItems == null) { LoadTableItems = await SQLConnection.Table<TableItems>().ToListAsync(); }

                    //Filter un/ignored feeds
                    List<string> UnignoredFeedList = LoadTableFeeds.Where(x => x.feed_ignore_status == false).Select(x => x.feed_id).ToList();

                    //Get the search items by feed or folder
                    if (SearchPage.vSearchFeed.feed_folder_status)
                    {
                        List<string> SearchFolders = SearchPage.vSearchFeed.feed_folder_ids;
                        System.Diagnostics.Debug.WriteLine("Search in folders: " + string.Join(", ", SearchFolders));
                        LoadTableItems = LoadTableItems.Where(x => x.item_title.ToLower().Contains(SearchPage.vSearchTerm.ToLower()) && SearchFolders.Any(y => y == x.item_feed_id) && UnignoredFeedList.Any(y => y == x.item_feed_id)).ToList();
                    }
                    else
                    {
                        string FeedId = SearchPage.vSearchFeed.feed_id;
                        if (FeedId != "0")
                        {
                            System.Diagnostics.Debug.WriteLine("Search in feed: " + FeedId);
                            LoadTableItems = LoadTableItems.Where(x => x.item_title.ToLower().Contains(SearchPage.vSearchTerm.ToLower()) && x.item_feed_id == FeedId && UnignoredFeedList.Any(y => y == x.item_feed_id)).ToList();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Search in all items.");
                            LoadTableItems = LoadTableItems.Where(x => x.item_title.ToLower().Contains(SearchPage.vSearchTerm.ToLower()) && UnignoredFeedList.Any(y => y == x.item_feed_id)).ToList();
                        }
                    }

                    //Count items in table
                    FoundItems = LoadTableItems.Count();
                }
                else if (AppVariables.LoadFeeds)
                {
                    if (!Silent) { await EventProgressDisableUI("Counting your feeds...", true); }
                    System.Diagnostics.Debug.WriteLine("Counting your feeds...");

                    //Check if received lists are empty
                    if (LoadTableFeeds == null) { LoadTableFeeds = await SQLConnection.Table<TableFeeds>().ToListAsync(); }

                    //Load feeds from table
                    FoundItems = LoadTableFeeds.Count();
                }

                if (EnableUI) { await EventProgressEnableUI(); }
                return FoundItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed counting items: " + ex.Message);
                await EventProgressEnableUI();
                return FoundItems;
            }
        }

        //Filter news items
        public static List<TableItems> FilterNewsItems(List<string> IgnoredFeedList, List<TableItems> LoadTableItems, Feeds FilterFeed, int SkipItems, int NumberOfItems)
        {
            try
            {
                DateTime RemoveItemsRange = DateTime.UtcNow.AddDays(-Convert.ToDouble(AppVariables.ApplicationSettings["RemoveItemsRange"]));
                if (FilterFeed.feed_id == "0")
                {
                    LoadTableItems = LoadTableItems.Where(x => x.item_datetime > RemoveItemsRange && !IgnoredFeedList.Any(y => y == x.item_feed_id)).OrderByDescending(x => x.item_datetime).Skip(SkipItems).Take(NumberOfItems).ToList();
                }
                else if (FilterFeed.feed_id == "1")
                {
                    LoadTableItems = LoadTableItems.Where(x => x.item_read_status == true && x.item_datetime > RemoveItemsRange && !IgnoredFeedList.Any(y => y == x.item_feed_id)).OrderByDescending(x => x.item_datetime).Skip(SkipItems).Take(NumberOfItems).ToList();
                }
                else if (FilterFeed.feed_id == "2")
                {
                    LoadTableItems = LoadTableItems.Where(x => x.item_read_status == false && x.item_datetime > RemoveItemsRange && !IgnoredFeedList.Any(y => y == x.item_feed_id)).OrderByDescending(x => x.item_datetime).Skip(SkipItems).Take(NumberOfItems).ToList();
                }
                else
                {
                    if ((bool)AppVariables.ApplicationSettings["DisplayReadMarkedItems"])
                    {
                        LoadTableItems = LoadTableItems.Where(x => x.item_datetime > RemoveItemsRange && (x.item_feed_id == FilterFeed.feed_id || FilterFeed.feed_folder_ids.Any(y => y == x.item_feed_id)) && !IgnoredFeedList.Any(y => y == x.item_feed_id)).OrderByDescending(x => x.item_datetime).Skip(SkipItems).Take(NumberOfItems).ToList();
                    }
                    else
                    {
                        LoadTableItems = LoadTableItems.Where(x => x.item_read_status == false && x.item_datetime > RemoveItemsRange && (x.item_feed_id == FilterFeed.feed_id || FilterFeed.feed_folder_ids.Any(y => y == x.item_feed_id)) && !IgnoredFeedList.Any(y => y == x.item_feed_id)).OrderByDescending(x => x.item_datetime).Skip(SkipItems).Take(NumberOfItems).ToList();
                    }
                }
            }
            catch { }
            return LoadTableItems;
        }

        //Validate table items
        public static bool ValidateTableItems(List<string> IgnoredFeedList, TableItems ValidateItem)
        {
            try
            {
                if (AppVariables.LoadStarred) { return ValidateItem.item_star_status; }
                else
                {
                    DateTime RemoveItemsRange = DateTime.UtcNow.AddDays(-Convert.ToDouble(AppVariables.ApplicationSettings["RemoveItemsRange"]));
                    if ((bool)AppVariables.ApplicationSettings["DisplayReadMarkedItems"])
                    {
                        if (ValidateItem.item_datetime > RemoveItemsRange && !IgnoredFeedList.Any(y => y == ValidateItem.item_feed_id)) { return true; }
                    }
                    else
                    {
                        if (ValidateItem.item_read_status == false && ValidateItem.item_datetime > RemoveItemsRange && !IgnoredFeedList.Any(y => y == ValidateItem.item_feed_id)) { return true; }
                    }
                }
            }
            catch { }
            return false;
        }
    }
}