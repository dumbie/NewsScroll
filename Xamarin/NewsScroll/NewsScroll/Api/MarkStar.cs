using NewsScroll.Classes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;
using static NewsScroll.Lists.Lists;

namespace NewsScroll.Api
{
    partial class Api
    {
        //Mark item as star
        static public async Task<bool> MarkItemAsStarPrompt(Items ListItem, bool Confirm, bool RemoveFromList, bool ForceStar, bool Silent, bool EnableUI)
        {
            string ActionType = "Un/star";
            try
            {
                //Set the action string text
                if (ForceStar || ListItem.item_star_status == false) { ActionType = "Star"; } else { ActionType = "Unstar"; }

                if (Confirm)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add(ActionType + " item");
                    messageAnswers.Add("Cancel");

                    string messageResult = await AVMessageBox.Popup(ActionType + " item", "Do you want to " + ActionType.ToLower() + " this item?", messageAnswers);
                    if (messageResult == "Cancel")
                    {
                        return false;
                    }
                }

                await MarkStarSingle(ListItem, RemoveFromList, ActionType, Silent);

                if (EnableUI) { await EventProgressEnableUI(); }
                return true;
            }
            catch
            {
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Ok");

                await AVMessageBox.Popup("Failed to " + ActionType.ToLower() + " item", "Please check your internet connection and try again.", messageAnswers);
                await EventProgressEnableUI();
                return false;
            }
        }

        private static async Task<bool> MarkStarSingle(Items ListItem, bool RemoveFromList, string ActionType, bool Silent)
        {
            try
            {
                bool MarkStatus = false;
                string ItemId = ListItem.item_id;

                //Check if internet is available
                if (!NetworkInterface.GetIsNetworkAvailable() || ApiMessageError.StartsWith("(Off)"))
                {
                    if (!Silent) { await EventProgressDisableUI("Off " + ActionType.ToLower() + "ring the item...", true); }
                    Debug.WriteLine("Off " + ActionType.ToLower() + "ring the item...");

                    await AddOfflineSync(ItemId, ActionType);
                    MarkStatus = true;
                }
                else
                {
                    if (!Silent) { await EventProgressDisableUI(ActionType + "ring the item...", true); }
                    Debug.WriteLine(ActionType + "ring the item...");

                    //string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };

                    //HttpStringContent PostContent;
                    //if (ActionType == "Star") { PostContent = new HttpStringContent("i=" + ItemId + "&a=user/-/state/com.google/starred", Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"); }
                    //else { PostContent = new HttpStringContent("i=" + ItemId + "&r=user/-/state/com.google/starred", Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"); }

                    //HttpResponseMessage PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "edit-tag"), PostContent);
                    //if (PostHttp != null && (PostHttp.Content.ToString() == "OK" || PostHttp.Content.ToString().Contains("<error>Not found</error>"))) { MarkStatus = true; }
                }

                if (MarkStatus)
                {
                    //Wait for busy database
                    await ApiUpdate.WaitForBusyDatabase();

                    //Mark item in database and list
                    TableItems TableEditItems = await vSQLConnection.Table<TableItems>().Where(x => x.item_id == ItemId).FirstOrDefaultAsync();
                    if (TableEditItems != null)
                    {
                        if (ActionType == "Star")
                        {
                            TableEditItems.item_star_status = true;
                            ListItem.item_star_status = true;
                        }
                        else
                        {
                            TableEditItems.item_star_status = false;
                            ListItem.item_star_status = false;
                            if (RemoveFromList) { List_StarredItems.Remove(ListItem); }
                        }
                    }

                    //Update the items in database
                    await vSQLConnection.UpdateAsync(TableEditItems);
                }
                else
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await AVMessageBox.Popup("Failed to " + ActionType.ToLower() + " item", "Please check your internet connection and try again.", messageAnswers);
                    await EventProgressEnableUI();
                }

                return MarkStatus;
            }
            catch
            {
                Debug.WriteLine("Failed to un/star item.");
                return false;
            }
        }

        //Mark item as un/star from string list
        static public async Task<bool> MarkItemStarStringList(List<string> MarkIds, bool MarkType)
        {
            try
            {
                //Add items to post string
                string PostStringItemIds = string.Empty;
                foreach (string ItemId in MarkIds) { PostStringItemIds += "&i=" + ItemId; }

                //string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };

                //HttpStringContent PostContent;
                //if (MarkType) { PostContent = new HttpStringContent("a=user/-/state/com.google/starred" + PostStringItemIds, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"); }
                //else { PostContent = new HttpStringContent("r=user/-/state/com.google/starred" + PostStringItemIds, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"); }

                //HttpResponseMessage PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "edit-tag"), PostContent);
                //if (PostHttp != null && (PostHttp.Content.ToString() == "OK" || PostHttp.Content.ToString().Contains("<error>Not found</error>"))) { return true; } else { return false; }
                return false;
            }
            catch { return false; }
        }
    }
}