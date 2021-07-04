using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;

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
                        bool IgnoreStatus = Feed.feed_ignore_status ? true : false;

                        //Load feed folder
                        string FeedFolder = Feed.feed_folder;
                        if (string.IsNullOrWhiteSpace(FeedFolder))
                        {
                            FeedFolder = "No folder";
                        }

                        //Load feed icon
                        ImageSource FeedIcon = null;
                        if (FeedId.StartsWith("user/"))
                        {
                            FeedIcon = ImageSource.FromResource("NewsScroll.Assets.iconUser-Dark.png");
                        }
                        else
                        {
                            FeedIcon = AVFiles.File_LoadImage(FeedId + ".png", true);
                        }
                        if (FeedIcon == null)
                        {
                            FeedIcon = ImageSource.FromResource("NewsScroll.Assets.iconRSS-Dark.png");
                        }

                        //Add feed to list
                        Device.BeginInvokeOnMainThread(() =>
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
                    if (AppVariables.CurrentFeedsLoaded == 1) { EventHideProgressionStatus(); }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed processing multiple feeds from database: " + ex.Message);
                return false;
            }
        }
    }
}