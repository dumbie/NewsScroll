using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;

namespace NewsScroll
{
    partial class Process
    {
        public static async Task<bool> ProcessTableItemsToList(ObservableCollection<Items> AddList, List<TableItems> LoadTableItems, bool AddFromTop, bool HideReadStatus, bool HideStarStatus)
        {
            try
            {
                //Check if media needs to load
                AppVariables.LoadMedia = true;
                if (!NetworkInterface.GetIsNetworkAvailable() && !(bool)AppSettingLoad("DisplayImagesOffline")) { AppVariables.LoadMedia = false; }

                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                List<TableFeeds> FeedList = await vSQLConnection.Table<TableFeeds>().ToListAsync();
                foreach (TableItems LoadTable in LoadTableItems)
                {
                    //Load and check item id
                    string ItemId = LoadTable.item_id;
                    if (!AddList.Any(x => x.item_id == ItemId))
                    {
                        //Load feed id and title
                        string FeedId = LoadTable.item_feed_id;
                        string FeedTitle = "Unknown feed";

                        TableFeeds TableResult = FeedList.Where(x => x.feed_id == FeedId).FirstOrDefault();
                        if (TableResult == null)
                        {
                            //Debug.WriteLine("Feed not found: " + FeedId);
                            if (LoadTable.item_feed_title != null && !string.IsNullOrWhiteSpace(LoadTable.item_feed_title))
                            {
                                Debug.WriteLine("Backup feed title: " + LoadTable.item_feed_title);
                                FeedTitle = LoadTable.item_feed_title;
                            }
                            else if (FeedId.StartsWith("user/"))
                            {
                                Debug.WriteLine("Detected an user feed.");
                                FeedTitle = "User feed";
                            }
                        }
                        else
                        {
                            //Set the feed item title
                            FeedTitle = TableResult.feed_title;
                        }

                        //Load item image
                        bool ItemImageVisibility = false;
                        string ItemImageLink = LoadTable.item_image;
                        if (!string.IsNullOrWhiteSpace(ItemImageLink) && AppVariables.LoadMedia)
                        {
                            ItemImageVisibility = true;
                        }

                        //Set the date time string
                        DateTime convertedDate = DateTime.SpecifyKind(LoadTable.item_datetime, DateTimeKind.Utc).ToLocalTime();
                        string DateAuthorString = convertedDate.ToString(AppVariables.CultureInfoLocal.DateTimeFormat.LongDatePattern, AppVariables.CultureInfoLocal.DateTimeFormat) + ", " + convertedDate.ToString(AppVariables.CultureInfoLocal.DateTimeFormat.ShortTimePattern, AppVariables.CultureInfoLocal.DateTimeFormat);

                        //Add the author to date time
                        if ((bool)AppSettingLoad("DisplayItemsAuthor") && !string.IsNullOrWhiteSpace(LoadTable.item_author))
                        {
                            DateAuthorString += " by " + LoadTable.item_author;
                        }

                        //Check item read or star status
                        bool ReadVisibility = false;
                        if (!HideReadStatus) { ReadVisibility = LoadTable.item_read_status ? true : false; }
                        bool StarredVisibility = false;
                        if (!HideStarStatus) { StarredVisibility = LoadTable.item_star_status ? true : false; }

                        //Load item content
                        string item_content = string.Empty;
                        if ((bool)AppSettingLoad("ContentCutting"))
                        {
                            item_content = AVFunctions.StringCut(LoadTable.item_content, Convert.ToInt32(AppSettingLoad("ContentCuttingLength")), "...");
                        }
                        else
                        {
                            item_content = AVFunctions.StringCut(LoadTable.item_content, AppVariables.MaximumItemTextLength, "...");
                        }

                        //Add item to the ListView
                        Items NewItem = new Items() { feed_id = FeedId, feed_title = FeedTitle, item_id = ItemId, item_read_status = ReadVisibility, item_star_status = StarredVisibility, item_title = LoadTable.item_title, item_image_visibility = ItemImageVisibility, item_image_link = ItemImageLink, item_content = item_content, item_link = LoadTable.item_link, item_datestring = DateAuthorString, item_datetime = LoadTable.item_datetime };
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                if (AddFromTop)
                                {
                                    AddList.Insert(0, NewItem);
                                }
                                else
                                {
                                    AddList.Add(NewItem);
                                }
                            }
                            catch { }
                        });

                        //Update the added item count
                        AppVariables.CurrentItemsLoaded++;

                        //Request information update
                        if (AppVariables.CurrentItemsLoaded == 1) { EventHideProgressionStatus(); }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed processing multiple items from database: " + ex.Message);
                return false;
            }
        }

        //Update the item image content
        public static void ItemUpdateImages(object itemObject, bool resetContent)
        {
            try
            {
                Items updateItem = (Items)itemObject;
                if (resetContent)
                {
                    //Debug.WriteLine("Disappearing item: " + updateItem.item_title);
                    if (updateItem.item_image != null)
                    {
                        updateItem.item_image = null;
                    }
                    if (updateItem.feed_icon != null)
                    {
                        updateItem.feed_icon = null;
                    }
                }
                else
                {
                    //Debug.WriteLine("Appearing item: " + updateItem.item_title);
                    string ItemImageLink = updateItem.item_image_link;
                    //fix AppVariables.LoadMedia
                    if (updateItem.item_image == null && !string.IsNullOrWhiteSpace(ItemImageLink) && updateItem.item_image_visibility == true)
                    {
                        updateItem.item_image = ImageSource.FromUri(new Uri(ItemImageLink));
                    }
                    if (updateItem.feed_icon == null)
                    {
                        //Load feed icon
                        if (updateItem.feed_id.StartsWith("user/"))
                        {
                            updateItem.feed_icon = ImageSource.FromResource("NewsScroll.Assets.iconUser-Dark.png");
                        }
                        else
                        {
                            updateItem.feed_icon = AVFiles.File_LoadImage(updateItem.feed_id + ".png", true);
                        }
                        if (updateItem.feed_icon == null)
                        {
                            updateItem.feed_icon = ImageSource.FromResource("NewsScroll.Assets.iconRSS-Dark.png");
                        }
                    }
                }
            }
            catch { }
        }
    }
}