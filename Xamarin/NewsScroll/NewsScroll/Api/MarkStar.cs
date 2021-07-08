using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;
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

                    string messageResult = await MessagePopup.Popup(ActionType + " item", "Do you want to " + ActionType.ToLower() + " this item?", messageAnswers);
                    if (messageResult == "Cancel")
                    {
                        return false;
                    }
                }

                await MarkStarSingle(ListItem, RemoveFromList, ActionType, Silent);

                if (EnableUI) { EventProgressEnableUI(); }
                return true;
            }
            catch
            {
                List<string> messageAnswers = new List<string>();
                messageAnswers.Add("Ok");

                await MessagePopup.Popup("Failed to " + ActionType.ToLower() + " item", "Please check your internet connection and try again.", messageAnswers);
                EventProgressEnableUI();
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
                    if (!Silent) { EventProgressDisableUI("Off " + ActionType.ToLower() + "ring the item...", true); }
                    Debug.WriteLine("Off " + ActionType.ToLower() + "ring the item...");

                    await AddOfflineSync(ItemId, ActionType);
                    MarkStatus = true;
                }
                else
                {
                    if (!Silent) { EventProgressDisableUI(ActionType + "ring the item...", true); }
                    Debug.WriteLine(ActionType + "ring the item...");

                    string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };

                    StringContent PostContent;
                    if (ActionType == "Star")
                    {
                        PostContent = new StringContent("i=" + ItemId + "&a=user/-/state/com.google/starred", Encoding.UTF8, "application/x-www-form-urlencoded");
                    }
                    else
                    {
                        PostContent = new StringContent("i=" + ItemId + "&r=user/-/state/com.google/starred", Encoding.UTF8, "application/x-www-form-urlencoded");
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

                    await MessagePopup.Popup("Failed to " + ActionType.ToLower() + " item", "Please check your internet connection and try again.", messageAnswers);
                    EventProgressEnableUI();
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

                string[][] RequestHeader = new string[][] { new[] { "Authorization", "GoogleLogin auth=" + AppSettingLoad("ConnectApiAuth").ToString() } };

                StringContent PostContent;
                if (MarkType)
                {
                    PostContent = new StringContent("a=user/-/state/com.google/starred" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded");
                }
                else
                {
                    PostContent = new StringContent("r=user/-/state/com.google/starred" + PostStringItemIds, Encoding.UTF8, "application/x-www-form-urlencoded");
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