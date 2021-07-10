using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.AppVariables;
using static NewsScroll.Database.Database;

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
                if (ForceRead || !ListItem.item_read_status) { ActionType = "Read"; } else { ActionType = "Unread"; }

                if (Confirm)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Mark item " + ActionType.ToLower());
                    messageAnswers.Add("Cancel");

                    string messageResult = await MessagePopup.Popup("Mark item " + ActionType.ToLower(), "Do you want to mark this item as " + ActionType.ToLower() + "?", messageAnswers);
                    if (messageResult == "Cancel")
                    {
                        return false;
                    }
                }

                await MarkReadSingle(UpdateList, ListItem, ActionType, Silent);

                if (!Silent) { EventProgressEnableUI(); }
                return true;
            }
            catch
            {
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Ok");

                await MessagePopup.Popup("Failed to mark item " + ActionType.ToLower(), "Please check your internet connection and try again.", messageAnswers);
                EventProgressEnableUI();
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
                if (Connectivity.NetworkAccess != NetworkAccess.Internet || ApiMessageError.StartsWith("(Off)"))
                {
                    if (!Silent) { EventProgressDisableUI("Off marking item as " + ActionType.ToLower() + "...", true); }
                    Debug.WriteLine("Off marking item as " + ActionType.ToLower() + "...");

                    await AddOfflineSync(ItemId, ActionType);
                    MarkStatus = true;
                }
                else
                {
                    if (!Silent) { EventProgressDisableUI("Marking item as " + ActionType.ToLower() + "...", true); }
                    Debug.WriteLine("Marking item as " + ActionType.ToLower() + "...");

                    string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };

                    StringContent PostContent;
                    if (ActionType == "Read")
                    {
                        PostContent = new StringContent("i=" + ItemId + "&a=user/-/state/com.google/read", Encoding.UTF8, "application/x-www-form-urlencoded");
                    }
                    else
                    {
                        PostContent = new StringContent("i=" + ItemId + "&r=user/-/state/com.google/read", Encoding.UTF8, "application/x-www-form-urlencoded");
                    }
                    Uri PostUri = new Uri(ApiConnectionUrl + "edit-tag");

                    string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, PostUri, PostContent);
                    if (PostHttp != null && (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")))
                    {
                        MarkStatus = true;
                    }
                }

                if (MarkStatus)
                {
                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    //Get the current page name
                    string currentPage = App.Current.MainPage.ToString();

                    //Mark item in database and list
                    TableItems TableEditItems = await vSQLConnection.Table<TableItems>().Where(x => x.item_id == ItemId).FirstOrDefaultAsync();
                    if (TableEditItems != null)
                    {
                        if (ActionType == "Read")
                        {
                            TableEditItems.item_read_status = true;
                            ListItem.item_read_status = true;
                            ListItem.item_read_icon = ImageSource.FromResource("NewsScroll.Assets.iconRead-Dark.png");
                            if (currentPage.EndsWith("NewsPage") && vNewsFeed.feed_id != "1" && (bool)AppSettingLoad("HideReadMarkedItem"))
                            {
                                UpdateList.Remove(ListItem);
                            }
                        }
                        else
                        {
                            TableEditItems.item_read_status = false;
                            ListItem.item_read_status = false;
                            ListItem.item_read_icon = null;
                            if (currentPage.EndsWith("NewsPage") && vNewsFeed.feed_id == "1" && (bool)AppSettingLoad("HideReadMarkedItem"))
                            {
                                UpdateList.Remove(ListItem);
                            }
                        }
                    }

                    //Update the items in database
                    await vSQLConnection.UpdateAsync(TableEditItems);
                }
                else
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("Failed to mark item " + ActionType.ToLower(), "Please check your internet connection and try again.", messageAnswers);
                    EventProgressEnableUI();
                }

                return MarkStatus;
            }
            catch
            {
                Debug.WriteLine("Failed to un/read item.");
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
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Mark read till item");
                    messageAnswers.Add("Cancel");

                    string messageResult = await MessagePopup.Popup("Mark items read till item", "Do you want to mark all items for the selected feed till this item as read?", messageAnswers);
                    if (messageResult == "Cancel")
                    {
                        return false;
                    }
                }

                bool MarkStatus = false;
                string EndItemId = EndItem.item_id;

                //Check the selected items
                List<Items> TableEditItems = UpdateList.Where(x => x.item_id == EndItemId || x.item_read_status == false).ToList();

                //Check if internet is available
                if (Connectivity.NetworkAccess != NetworkAccess.Internet || ApiMessageError.StartsWith("(Off)"))
                {
                    if (!Silent) { EventProgressDisableUI("Off marking read till item...", true); }
                    Debug.WriteLine("Off marking read till item...");

                    //Add items to string list
                    List<string> ReadItemIds = new List<string>();
                    foreach (Items NewsItem in TableEditItems)
                    {
                        string NewsItemId = NewsItem.item_id;
                        ReadItemIds.Add(NewsItemId);

                        //Check if the end item has been reached
                        if (EndItemId == NewsItemId)
                        {
                            Debug.WriteLine("Added all news items to the string list.");
                            break;
                        }
                    }

                    await AddOfflineSync(ReadItemIds, "Read");
                    MarkStatus = true;
                }
                else
                {
                    if (!Silent) { EventProgressDisableUI("Marking read till item...", true); }
                    Debug.WriteLine("Marking read till item...");

                    //Add items to post string
                    string PostStringItemIds = string.Empty;
                    foreach (Items NewsItem in TableEditItems)
                    {
                        string NewsItemId = NewsItem.item_id;
                        PostStringItemIds += "&i=" + NewsItemId;

                        //Check if the end item has been reached
                        if (EndItemId == NewsItemId)
                        {
                            Debug.WriteLine("Added all news items to the post string.");
                            break;
                        }
                    }

                    string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };
                    StringContent PostContent = new StringContent("a=user/-/state/com.google/read" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded");
                    Uri PostUri = new Uri(ApiConnectionUrl + "edit-tag");

                    string PostHttp = await AVDownloader.SendPostRequestAsync(10000, "News Scroll", RequestHeader, PostUri, PostContent);
                    if (PostHttp != null && (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")))
                    {
                        MarkStatus = true;
                    }
                }

                if (MarkStatus)
                {
                    Debug.WriteLine("Marked items till this item as read on the server or offline sync list.");

                    //Add items to post string
                    string SqlStringItemIds = string.Empty;
                    foreach (Items NewsItem in TableEditItems)
                    {
                        string NewsItemId = NewsItem.item_id;
                        SqlStringItemIds += "'" + NewsItemId + "',";

                        //Check if the end item has been reached
                        if (EndItemId == NewsItemId)
                        {
                            Debug.WriteLine("Added all news items to the sql string.");
                            break;
                        }
                    }

                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    SqlStringItemIds = AVFunctions.StringRemoveEnd(SqlStringItemIds, ",");
                    await vSQLConnection.ExecuteAsync("UPDATE TableItems SET item_read_status = ('1') WHERE item_id IN (" + SqlStringItemIds + ") AND item_read_status = ('0')");

                    //Update current items list
                    foreach (Items NewsItem in UpdateList.ToList())
                    {
                        //Mark the item as read or remove it from list
                        if ((bool)AppSettingLoad("HideReadMarkedItem"))
                        {
                            UpdateList.Remove(NewsItem);
                        }
                        else
                        {
                            NewsItem.item_read_status = true;
                            NewsItem.item_read_icon = ImageSource.FromResource("NewsScroll.Assets.iconRead-Dark.png");
                        }

                        //Check if the end item has been reached
                        if (EndItemId == NewsItem.item_id)
                        {
                            Debug.WriteLine("Marked items till this item as read in the list and database.");
                            break;
                        }
                    }

                    if (EnableUI) { EventProgressEnableUI(); }
                }
                else
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("Failed to mark items read", "Please check your internet connection and try again.", messageAnswers);
                    EventProgressEnableUI();
                }

                return MarkStatus;
            }
            catch
            {
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Ok");

                await MessagePopup.Popup("Failed to mark items read", "Please check your internet connection and try again.", messageAnswers);
                EventProgressEnableUI();
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
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("Not logged in", "Marking all items read can only be done when you are logged in.", messageAnswers);
                    return false;
                }

                if (Confirm)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Mark all items read");
                    messageAnswers.Add("Cancel");

                    string messageResult = await MessagePopup.Popup("Mark all items read", "Do you want to mark all items for every feed as read?", messageAnswers);
                    if (messageResult == "Cancel")
                    {
                        return false;
                    }
                }

                bool MarkStatus = false;

                //Check if internet is available
                if (Connectivity.NetworkAccess != NetworkAccess.Internet || ApiMessageError.StartsWith("(Off)"))
                {
                    EventProgressDisableUI("Off marking all items as read...", true);
                    Debug.WriteLine("Off marking all items as read...");

                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    List<string> UnreadItemList = (await vSQLConnection.Table<TableItems>().Where(x => !x.item_read_status).ToListAsync()).Select(x => x.item_id).ToList();
                    await AddOfflineSync(UnreadItemList, "Read");
                    MarkStatus = true;
                }
                else
                {
                    EventProgressDisableUI("Marking all items as read...", true);
                    Debug.WriteLine("Marking all items as read...");

                    //Date time variables
                    long UnixTimeTicks = 0;
                    if (AppSettingLoad("LastItemsUpdate").ToString() != "Never")
                    {
                        UnixTimeTicks = (Convert.ToDateTime(AppSettingLoad("LastItemsUpdate"), AppVariables.CultureInfoEnglish).Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10; //Nanoseconds
                    }

                    //Mark all items as read on api server
                    string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };
                    StringContent PostContent = new StringContent("s=user/-/state/com.google/reading-list&ts=" + UnixTimeTicks, Encoding.UTF8, "application/x-www-form-urlencoded");
                    Uri PostUri = new Uri(ApiConnectionUrl + "mark-all-as-read");

                    string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, PostUri, PostContent);
                    if (PostHttp != null && (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")))
                    {
                        MarkStatus = true;
                    }
                }

                if (MarkStatus)
                {
                    Debug.WriteLine("Marked all items as read on the server or offline sync list.");

                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    //Update items in database
                    await vSQLConnection.ExecuteAsync("UPDATE TableItems SET item_read_status = ('1') WHERE item_read_status = ('0')");

                    //Update current items list
                    List<Items> ListItems = UpdateList.Where(x => x.item_read_status == false).ToList();
                    foreach (Items NewsItem in ListItems)
                    {
                        if ((bool)AppSettingLoad("HideReadMarkedItem"))
                        {
                            UpdateList.Remove(NewsItem);
                        }
                        else
                        {
                            NewsItem.item_read_status = true;
                            NewsItem.item_read_icon = ImageSource.FromResource("NewsScroll.Assets.iconRead-Dark.png");
                        }
                    }

                    EventProgressEnableUI();
                }
                else
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await MessagePopup.Popup("Failed to mark all items read", "Please check your internet connection and try again.", messageAnswers);
                    EventProgressEnableUI();
                }

                return MarkStatus;
            }
            catch
            {
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Ok");

                await MessagePopup.Popup("Failed to mark all items read", "Please check your internet connection and try again.", messageAnswers);
                EventProgressEnableUI();
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

                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };

                StringContent PostContent;
                if (MarkType)
                {
                    PostContent = new StringContent("a=user/-/state/com.google/read" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded");
                }
                else
                {
                    PostContent = new StringContent("r=user/-/state/com.google/read" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded");
                }
                Uri PostUri = new Uri(ApiConnectionUrl + "edit-tag");

                string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, PostUri, PostContent);
                if (PostHttp != null && (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}