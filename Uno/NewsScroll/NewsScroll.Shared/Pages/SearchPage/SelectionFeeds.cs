using ArnoldVinkCode;
using NewsScroll.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using static NewsScroll.Database.Database;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class SearchPage
    {
        //Load feeds in selection
        async Task LoadSelectionFeeds(bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { await ProgressDisableUI("Loading selection feeds...", true); }
                System.Diagnostics.Debug.WriteLine("Loading selection feeds, silent: " + Silent);

                combobox_FeedSelection.IsHitTestVisible = false;
                combobox_FeedSelection.Opacity = 0.30;
                await ClearObservableCollection(List_FeedSelect);

                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                //Check if received lists are empty
                List<TableFeeds> LoadTableFeeds = await SQLConnection.Table<TableFeeds>().OrderBy(x => x.feed_folder).ToListAsync();

                //Filter un/ignored feeds
                List<TableFeeds> UnignoredFeedList = LoadTableFeeds.Where(x => x.feed_ignore_status == false).ToList();

                //Add all feeds selection
                Feeds FeedItemAll = new Feeds();
                FeedItemAll.feed_icon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconRSS-Dark.png", false);
                FeedItemAll.feed_title = "All available items";
                FeedItemAll.feed_collection_status = true;
                FeedItemAll.feed_id = "0";
                List_FeedSelect.Add(FeedItemAll);

                //Feeds that are not ignored and contain items
                foreach (TableFeeds Feed in UnignoredFeedList)
                {
                    Feeds TempFeed = new Feeds();
                    TempFeed.feed_id = Feed.feed_id;

                    //Add folder
                    string FeedFolder = Feed.feed_folder;
                    if (string.IsNullOrWhiteSpace(FeedFolder)) { FeedFolder = "No folder"; }
                    Feeds FolderUpdate = List_FeedSelect.Where(x => x.feed_folder_title == FeedFolder && x.feed_folder_status).FirstOrDefault();
                    if (FolderUpdate == null)
                    {
                        //Load folder icon
                        BitmapImage FolderIcon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconFolder-Dark.png", false);

                        //Add folder
                        Feeds FolderItem = new Feeds();
                        FolderItem.feed_icon = FolderIcon;
                        FolderItem.feed_folder_title = FeedFolder;
                        FolderItem.feed_folder_status = true;
                        List_FeedSelect.Add(FolderItem);
                        //System.Diagnostics.Debug.WriteLine("Added folder...");
                    }

                    //Add feed
                    //Load feed icon
                    BitmapImage FeedIcon = null;
                    if (Feed.feed_id.StartsWith("user/")) { FeedIcon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconUser-Dark.png", false); } else { FeedIcon = await AVImage.LoadBitmapImage("ms-appdata:///local/" + Feed.feed_id + ".png", false); }
                    if (FeedIcon == null) { FeedIcon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconRSS-Dark.png", false); }

                    //Get the current feed item count
                    Feeds FeedItem = new Feeds();
                    FeedItem.feed_icon = FeedIcon;
                    FeedItem.feed_title = Feed.feed_title;
                    FeedItem.feed_id = Feed.feed_id;
                    List_FeedSelect.Add(FeedItem);

                    //Update folder
                    FolderUpdate = List_FeedSelect.Where(x => x.feed_folder_title == FeedFolder && x.feed_folder_status).FirstOrDefault();
                    if (FolderUpdate != null)
                    {
                        FolderUpdate.feed_folder_ids.Add(Feed.feed_id);
                        FolderUpdate.feed_item_count = FolderUpdate.feed_item_count + FeedItem.feed_item_count;
                        //System.Diagnostics.Debug.WriteLine("Updated folder...");
                    }
                }

                combobox_FeedSelection.SelectedIndex = 0;
                combobox_FeedSelection.IsHitTestVisible = true;
                combobox_FeedSelection.Opacity = 1;
            }
            catch { }
            if (EnableUI) { await ProgressEnableUI(); }
        }
    }
}