using ArnoldVinkCode;
using ArnoldVinkMessageBox;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;

namespace NewsScroll.Api
{
    partial class Api
    {
        //Mark item as read
        static public async Task<bool> MarkItemAsReadPrompt(ObservableCollection<Items> UpdateList, Items ListItem, bool Confirm, bool ForceRead, bool Silent)
        {
            string ActionType = "Un/read";
            try
            {
                //Set the action string text
                if (ForceRead || ListItem.item_read_status == Visibility.Collapsed) { ActionType = "Read"; } else { ActionType = "Unread"; }

                if (Confirm)
                {
                    int MsgBoxResult = await AVMessageBox.Popup("Mark item " + ActionType.ToLower(), "Do you want to mark this item as " + ActionType.ToLower() + "?", "Mark item " + ActionType.ToLower(), "", "", "", "", true);
                    if (MsgBoxResult == 0) { return false; }
                }

                await MarkReadSingle(UpdateList, ListItem, ActionType, Silent);

                if (!Silent) { await EventProgressEnableUI(); }
                return true;
            }
            catch
            {
                await AVMessageBox.Popup("Failed to mark item " + ActionType.ToLower(), "Please check your internet connection and try again.", "Ok", "", "", "", "", false);
                await EventProgressEnableUI();
                return false;
            }
        }

        private static async Task<bool> MarkReadSingle(ObservableCollection<Items> UpdateList, Items ListItem, string ActionType, bool Silent)
        {
            try
            {
                bool MarkStatus = false;
                string ItemId = ListItem.item_id;

                //Check if internet is available
                if (!NetworkInterface.GetIsNetworkAvailable() || ApiMessageError.StartsWith("(Off)"))
                {
                    if (!Silent) { await EventProgressDisableUI("Off marking item as " + ActionType.ToLower() + "...", true); }
                    System.Diagnostics.Debug.WriteLine("Off marking item as " + ActionType.ToLower() + "...");

                    await AddOfflineSync(ItemId, ActionType);
                    MarkStatus = true;
                }
                else
                {
                    if (!Silent) { await EventProgressDisableUI("Marking item as " + ActionType.ToLower() + "...", true); }
                    System.Diagnostics.Debug.WriteLine("Marking item as " + ActionType.ToLower() + "...");

                    string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };

                    StringContent PostContent;
                    if (ActionType == "Read") { PostContent = new StringContent("i=" + ItemId + "&a=user/-/state/com.google/read", Encoding.UTF8, "application/x-www-form-urlencoded"); }
                    else { PostContent = new StringContent("i=" + ItemId + "&r=user/-/state/com.google/read", Encoding.UTF8, "application/x-www-form-urlencoded"); }

                    string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "edit-tag"), PostContent);
                    if (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")) { MarkStatus = true; }
                }

                if (MarkStatus)
                {
                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    //Mark item in database and list
                    TableItems TableEditItems = await SQLConnection.Table<TableItems>().Where(x => x.item_id == ItemId).FirstOrDefaultAsync();
                    if (TableEditItems != null)
                    {
                        //fix
                        //if (ActionType == "Read")
                        //{
                        //    TableEditItems.item_read_status = true;
                        //    ListItem.item_read_status = Visibility.Visible; //Updates itemviewer
                        //    if (//FixApp.vApplicationFrame.SourcePageType.ToString().EndsWith("NewsPage") && NewsPage.vCurrentLoadingFeedFolder.feed_id != "1" && (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"])
                        //    {
                        //        UpdateList.Remove(ListItem);
                        //    }
                        //}
                        //else
                        //{
                        //    TableEditItems.item_read_status = false;
                        //    ListItem.item_read_status = Visibility.Collapsed; //Updates itemviewer
                        //    if (//FixApp.vApplicationFrame.SourcePageType.ToString().EndsWith("NewsPage") && NewsPage.vCurrentLoadingFeedFolder.feed_id == "1" && (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"])
                        //    {
                        //        UpdateList.Remove(ListItem);
                        //    }
                        //}
                    }

                    //Update the items in database
                    await SQLConnection.UpdateAsync(TableEditItems);
                }
                else
                {
                    await AVMessageBox.Popup("Failed to mark item " + ActionType.ToLower(), "Please check your internet connection and try again.", "Ok", "", "", "", "", false);
                    await EventProgressEnableUI();
                }

                return MarkStatus;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to un/read item.");
                return false;
            }
        }

        //Mark items till as read
        public static async Task<bool> MarkReadTill(ObservableCollection<Items> UpdateList, Items EndItem, bool Confirm, bool Silent, bool EnableUI)
        {
            try
            {
                if (Confirm)
                {
                    int MsgBoxResult = await AVMessageBox.Popup("Mark items read till item", "Do you want to mark all items for the selected feed till this item as read?", "Mark read till item", "", "", "", "", true);
                    if (MsgBoxResult == 0) { return false; }
                }

                bool MarkStatus = false;
                string EndItemId = EndItem.item_id;

                //Check the selected items
                List<Items> TableEditItems = UpdateList.Where(x => x.item_id == EndItemId || x.item_read_status == Visibility.Collapsed).ToList();

                //Check if internet is available
                if (!NetworkInterface.GetIsNetworkAvailable() || ApiMessageError.StartsWith("(Off)"))
                {
                    if (!Silent) { await EventProgressDisableUI("Off marking read till item...", true); }
                    System.Diagnostics.Debug.WriteLine("Off marking read till item...");

                    //Add items to string list
                    List<string> ReadItemIds = new List<string>();
                    foreach (Items NewsItem in TableEditItems)
                    {
                        string NewsItemId = NewsItem.item_id;
                        ReadItemIds.Add(NewsItemId);

                        //Check if the end item has been reached
                        if (EndItemId == NewsItemId)
                        {
                            System.Diagnostics.Debug.WriteLine("Added all news items to the string list.");
                            break;
                        }
                    }

                    await AddOfflineSync(ReadItemIds, "Read");
                    MarkStatus = true;
                }
                else
                {
                    if (!Silent) { await EventProgressDisableUI("Marking read till item...", true); }
                    System.Diagnostics.Debug.WriteLine("Marking read till item...");

                    //Add items to post string
                    string PostStringItemIds = string.Empty;
                    foreach (Items NewsItem in TableEditItems)
                    {
                        string NewsItemId = NewsItem.item_id;
                        PostStringItemIds += "&i=" + NewsItemId;

                        //Check if the end item has been reached
                        if (EndItemId == NewsItemId)
                        {
                            System.Diagnostics.Debug.WriteLine("Added all news items to the post string.");
                            break;
                        }
                    }

                    string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };
                    StringContent PostContent = new StringContent("a=user/-/state/com.google/read" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded");

                    string PostHttp = await AVDownloader.SendPostRequestAsync(10000, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "edit-tag"), PostContent);
                    if (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")) { MarkStatus = true; }
                }

                if (MarkStatus)
                {
                    System.Diagnostics.Debug.WriteLine("Marked items till this item as read on the server or offline sync list.");

                    //Add items to post string
                    string SqlStringItemIds = string.Empty;
                    foreach (Items NewsItem in TableEditItems)
                    {
                        string NewsItemId = NewsItem.item_id;
                        SqlStringItemIds += "'" + NewsItemId + "',";

                        //Check if the end item has been reached
                        if (EndItemId == NewsItemId)
                        {
                            System.Diagnostics.Debug.WriteLine("Added all news items to the sql string.");
                            break;
                        }
                    }

                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    SqlStringItemIds = AVFunctions.StringRemoveEnd(SqlStringItemIds, ",");
                    await SQLConnection.ExecuteAsync("UPDATE TableItems SET item_read_status = ('1') WHERE item_id IN (" + SqlStringItemIds + ") AND item_read_status = ('0')");

                    //Update current items list
                    foreach (Items NewsItem in UpdateList.ToList())
                    {
                        //Mark the item as read or remove it from list
                        if ((bool)AppVariables.ApplicationSettings["HideReadMarkedItem"])
                        {
                            UpdateList.Remove(NewsItem);
                        }
                        else
                        {
                            NewsItem.item_read_status = Visibility.Visible;
                        }

                        //Check if the end item has been reached
                        if (EndItemId == NewsItem.item_id)
                        {
                            System.Diagnostics.Debug.WriteLine("Marked items till this item as read in the list and database.");
                            break;
                        }
                    }

                    if (EnableUI) { await EventProgressEnableUI(); }
                }
                else
                {
                    await AVMessageBox.Popup("Failed to mark items read", "Please check your internet connection and try again.", "Ok", "", "", "", "", false);
                    await EventProgressEnableUI();
                }

                return MarkStatus;
            }
            catch
            {
                await AVMessageBox.Popup("Failed to mark items read", "Please check your internet connection and try again.", "Ok", "", "", "", "", false);
                await EventProgressEnableUI();
                return false;
            }
        }

        //Mark all items as read
        public static async Task<bool> MarkReadAll(ObservableCollection<Items> UpdateList, bool Confirm)
        {
            try
            {
                //Check if user is logged in
                if (!CheckLogin())
                {
                    await AVMessageBox.Popup("Not logged in", "Marking all items read can only be done when you are logged in.", "Ok", "", "", "", "", false);
                    return false;
                }

                if (Confirm)
                {
                    int MsgBoxResult = await AVMessageBox.Popup("Mark all items read", "Do you want to mark all items for every feed as read?", "Mark all items read", "", "", "", "", true);
                    if (MsgBoxResult == 0) { return false; }
                }

                bool MarkStatus = false;

                //Check if internet is available
                if (!NetworkInterface.GetIsNetworkAvailable() || ApiMessageError.StartsWith("(Off)"))
                {
                    await EventProgressDisableUI("Off marking all items as read...", true);
                    System.Diagnostics.Debug.WriteLine("Off marking all items as read...");

                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    List<string> UnreadItemList = (await SQLConnection.Table<TableItems>().Where(x => !x.item_read_status).ToListAsync()).Select(x => x.item_id).ToList();
                    await AddOfflineSync(UnreadItemList, "Read");
                    MarkStatus = true;
                }
                else
                {
                    await EventProgressDisableUI("Marking all items as read...", true);
                    System.Diagnostics.Debug.WriteLine("Marking all items as read...");

                    //Date time variables
                    long UnixTimeTicks = 0;
                    if (AppVariables.ApplicationSettings["LastItemsUpdate"].ToString() != "Never")
                    {
                        UnixTimeTicks = (Convert.ToDateTime(AppVariables.ApplicationSettings["LastItemsUpdate"], AppVariables.CultureInfoEnglish).Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10; //Nanoseconds
                    }

                    //Mark all items as read on api server
                    string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };
                    StringContent PostContent = new StringContent("s=user/-/state/com.google/reading-list&ts=" + UnixTimeTicks, Encoding.UTF8, "application/x-www-form-urlencoded");

                    string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "mark-all-as-read"), PostContent);
                    if (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")) { MarkStatus = true; }
                }

                if (MarkStatus)
                {
                    System.Diagnostics.Debug.WriteLine("Marked all items as read on the server or offline sync list.");

                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    //Update items in database
                    await SQLConnection.ExecuteAsync("UPDATE TableItems SET item_read_status = ('1') WHERE item_read_status = ('0')");

                    //Update current items list
                    List<Items> ListItems = UpdateList.Where(x => x.item_read_status == Visibility.Collapsed).ToList();
                    foreach (Items NewsItem in ListItems)
                    {
                        if ((bool)AppVariables.ApplicationSettings["HideReadMarkedItem"])
                        {
                            UpdateList.Remove(NewsItem);
                        }
                        else
                        {
                            NewsItem.item_read_status = Visibility.Visible;
                        }
                    }

                    await EventProgressEnableUI();
                }
                else
                {
                    await AVMessageBox.Popup("Failed to mark all items read", "Please check your internet connection and try again.", "Ok", "", "", "", "", false);
                    await EventProgressEnableUI();
                }

                return MarkStatus;
            }
            catch
            {
                await AVMessageBox.Popup("Failed to mark all items read", "Please check your internet connection and try again.", "Ok", "", "", "", "", false);
                await EventProgressEnableUI();
                return false;
            }
        }

        //Mark item as un/read from string list
        static public async Task<bool> MarkItemReadStringList(List<string> MarkIds, bool MarkType)
        {
            try
            {
                //Add items to post string
                string PostStringItemIds = string.Empty;
                foreach (string ItemId in MarkIds) { PostStringItemIds += "&i=" + ItemId; }

                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };

                StringContent PostContent;
                if (MarkType) { PostContent = new StringContent("a=user/-/state/com.google/read" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded"); }
                else { PostContent = new StringContent("r=user/-/state/com.google/read" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded"); }

                string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "edit-tag"), PostContent);
                if (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")) { return true; } else { return false; }
            }
            catch { return false; }
        }
    }
}