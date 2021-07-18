using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;

namespace NewsScroll
{
    partial class Process
    {
        public static async Task<bool> ProcessTableFeedsToList(ObservableCollection<Feeds> AddList, List<TableFeeds> LoadTableFeeds)
        {
            try
            {
                foreach (TableFeeds Feed in LoadTableFeeds)
                {
                    //Load and check feed id
                    string FeedId = Feed.feed_id;
                    if (!AddList.Any(x => x.feed_id == FeedId))
                    {
                        Visibility IgnoreStatus = Feed.feed_ignore_status ? Visibility.Visible : Visibility.Collapsed;

                        //Load feed folder
                        string FeedFolder = Feed.feed_folder;
                        if (string.IsNullOrWhiteSpace(FeedFolder)) { FeedFolder = "No folder"; }

                        //Load feed icon
                        BitmapImage FeedIcon = null;
                        if (FeedId.StartsWith("user/")) { FeedIcon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconUser-Dark.png", false); } else { FeedIcon = await AVImage.LoadBitmapImage("ms-appdata:///local/" + FeedId + ".png", false); }
                        if (FeedIcon == null) { FeedIcon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconRSS-Dark.png", false); }

                        //Add feed to list
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                AddList.Add(new Feeds() { feed_id = FeedId, feed_title = Feed.feed_title, feed_icon = FeedIcon, feed_link = Feed.feed_link, feed_folder_title = FeedFolder, feed_ignore_status = IgnoreStatus });
                            }
                            catch { }
                        });
                    }

                    //Update the added item count
                    AppVariables.CurrentFeedsLoaded++;

                    //Request information update
                    if (AppVariables.CurrentFeedsLoaded == 1) { await EventHideProgressionStatus(); }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed processing multiple feeds from database: " + ex.Message);
                return false;
            }
        }
    }
}