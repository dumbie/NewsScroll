using ArnoldVinkCode;
using ArnoldVinkMessageBox;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
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
                if (ForceStar || ListItem.item_star_status == Visibility.Collapsed) { ActionType = "Star"; } else { ActionType = "Unstar"; }

                if (Confirm)
                {
                    int MsgBoxResult = await AVMessageBox.Popup(ActionType + " item", "Do you want to " + ActionType.ToLower() + " this item?", ActionType + " item", "", "", "", "", true);
                    if (MsgBoxResult == 0) { return false; }
                }

                await MarkStarSingle(ListItem, RemoveFromList, ActionType, Silent);

                if (EnableUI) { await EventProgressEnableUI(); }
                return true;
            }
            catch
            {
                await AVMessageBox.Popup("Failed to " + ActionType.ToLower() + " item", "Please check your internet connection and try again.", "Ok", "", "", "", "", false);
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
                    System.Diagnostics.Debug.WriteLine("Off " + ActionType.ToLower() + "ring the item...");

                    await AddOfflineSync(ItemId, ActionType);
                    MarkStatus = true;
                }
                else
                {
                    if (!Silent) { await EventProgressDisableUI(ActionType + "ring the item...", true); }
                    System.Diagnostics.Debug.WriteLine(ActionType + "ring the item...");

                    string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };

                    StringContent PostContent;
                    if (ActionType == "Star")
                    {
                        PostContent = new StringContent("i=" + ItemId + "&a=user/-/state/com.google/starred", Encoding.UTF8, "application/x-www-form-urlencoded");
                    }
                    else
                    {
                        PostContent = new StringContent("i=" + ItemId + "&r=user/-/state/com.google/starred", Encoding.UTF8, "application/x-www-form-urlencoded");
                    }

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
                        if (ActionType == "Star")
                        {
                            TableEditItems.item_star_status = true;
                            ListItem.item_star_status = Visibility.Visible;
                        }
                        else
                        {
                            TableEditItems.item_star_status = false;
                            ListItem.item_star_status = Visibility.Collapsed;
                            if (RemoveFromList) { List_StarredItems.Remove(ListItem); }
                        }
                    }

                    //Update the items in database
                    await SQLConnection.UpdateAsync(TableEditItems);
                }
                else
                {
                    await AVMessageBox.Popup("Failed to " + ActionType.ToLower() + " item", "Please check your internet connection and try again.", "Ok", "", "", "", "", false);
                    await EventProgressEnableUI();
                }

                return MarkStatus;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to un/star item.");
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

                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppVariables.ApplicationSettings["ConnectApiAuth"].ToString() } };

                StringContent PostContent;
                if (MarkType) { PostContent = new StringContent("a=user/-/state/com.google/starred" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded"); }
                else { PostContent = new StringContent("r=user/-/state/com.google/starred" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded"); }

                string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", RequestHeader, new Uri(ApiConnectionUrl + "edit-tag"), PostContent);
                if (PostHttp == "OK" || PostHttp.Contains("<error>Not found</error>")) { return true; } else { return false; }
            }
            catch { return false; }
        }
    }
}