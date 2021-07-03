using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;
using static NewsScroll.AppVariables;

namespace NewsScroll
{
    class EventsListView
    {
        public static async void ListView_Items_Tapped(object sender, EventArgs e)
        {
            try
            {
                ListView SendListView = sender as ListView;
                Items SelectedItem = SendListView.SelectedItem as Items;
                ObservableCollection<Items> SelectedList = (ObservableCollection<Items>)SendListView.ItemsSource;
                if (SelectedItem != null)
                {
                    Debug.WriteLine(SelectedItem.item_title);



                    int doubleClickCount = await DoubleClickCheck();
                  if (doubleClickCount == 1)
                    {
                        await ListView_SingleTap(SelectedItem, SelectedList);
                    }
                    else if (doubleClickCount == 2)
                    {
                        await ListView_DoubleTap(SendListView, SelectedItem, SelectedList);
                    }

     

                    //Reset the item selection
                    SendListView.SelectedItem = -1;
                }
            }
            catch { }
        }

        private static async Task ListView_SingleTap(Items SelectedItem, ObservableCollection<Items> SelectedList)
        {
            try
            {
                bool IsNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
                if (AppSettingLoad("ItemOpenMethod").ToString() == "1" && IsNetworkAvailable)
                {
                    ////Open item in webviewer
                    //WebView webViewer = new WebView();
                    //await webViewer.OpenPopup(null, SelectedItem);

                    //Mark the item as read
                    bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, true, true);
                    //if (MarkedRead && (bool)AppSettingLoad("HideReadMarkedItem"]) { await EventUpdateTotalItemsCount(null, null, false, true); }
                }
                else if (AppSettingLoad("ItemOpenMethod").ToString() == "2" && IsNetworkAvailable)
                {
                    //Mark the item as read
                    bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, true, false);
                    //if (MarkedRead && (bool)AppSettingLoad("HideReadMarkedItem"]) { await EventUpdateTotalItemsCount(null, null, false, true); }

                    //Open item in webbrowser
                    await Browser.OpenAsync(new Uri(SelectedItem.item_link), BrowserLaunchMode.SystemPreferred);
                }
                else
                {
                    //Open item in itemviewer
                    //ItemViewer itemViewer = new ItemViewer();
                    //await itemViewer.OpenPopup(SelectedItem);

                    //Mark the item as read
                    bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, true, true);
                    //if (MarkedRead && (bool)AppSettingLoad("HideReadMarkedItem"]) { await EventUpdateTotalItemsCount(null, null, false, true); }
                }
            }
            catch { }
        }

        private static async Task ListView_DoubleTap(ListView SendListView, Items SelectedItem, ObservableCollection<Items> SelectedList)
        {
            try
            {
                    //Set the action string text
                    string ActionStarItem = "Un/star";
                    if (SelectedItem.item_star_status == true) { ActionStarItem = "Unstar"; } else { ActionStarItem = "Star"; }

                    //Set the action string text
                    string ActionReadItem = "Un/read";
                    if (SelectedItem.item_read_status == true) { ActionReadItem = "Unread"; } else { ActionReadItem = "Read"; }

                    //Get and set the current page name
                    string CurrentPageName = App.Current.MainPage.ToString();

                    //Check if mark read till is enabled
                    string ActionMarkReadTill = string.Empty;
                    if (CurrentPageName.EndsWith("NewsPage") && NewsPage.vCurrentLoadingFeedFolder.feed_id != "1") { ActionMarkReadTill = "Mark read till item"; }

                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Open in browser");
                    messageAnswers.Add("Share this item");
                    messageAnswers.Add(ActionStarItem + " this item");
                    messageAnswers.Add("Mark item as " + ActionReadItem.ToLower());
                    messageAnswers.Add(ActionMarkReadTill);
                    messageAnswers.Add("Cancel");

                    string messageResult = await AVMessageBox.Popup("News item actions", "What would you like to do with this item?", messageAnswers);
                    if (messageResult == "Open in browser")
                    {
                        await ListItemOpenBrowser(SendListView, SelectedItem, SelectedList, CurrentPageName);
                    }
                    else if (messageResult == "Share this item")
                    {
                        await ShareItem(SelectedItem);
                    }
                    else if (messageResult == ActionStarItem + " this item")
                    {
                        if (CurrentPageName.EndsWith("StarredPage"))
                        {
                            bool MarkedAsStar = await MarkItemAsStarPrompt(SelectedItem, false, true, false, false, true);
                            //if (MarkedAsStar) { await EventUpdateTotalItemsCount(null, null, false, true); }
                        }
                        else
                        {
                            await MarkItemAsStarPrompt(SelectedItem, false, false, false, false, true);
                        }
                    }
                    else if (messageResult == "Mark item as " + ActionReadItem.ToLower())
                    {
                        await ListItemMarkRead(false, SendListView, SelectedItem, SelectedList, CurrentPageName);
                    }
                    else if (messageResult == ActionMarkReadTill)
                    {
                        bool MarkedRead = await MarkReadTill(SelectedList, SelectedItem, true, false, true);
                        if (MarkedRead && CurrentPageName.EndsWith("NewsPage"))
                        {
                            if ((bool)AppSettingLoad("HideReadMarkedItem"))
                            {
                                //fix
                                //if (SendListView.Items.Any())
                                //{
                                //    //Scroll to the ListView top
                                //    await Task.Delay(10);
                                //    SendListView.ScrollIntoView(SendListView.Items.First());

                                //    //Update the header and selection feeds
                                //    await EventUpdateTotalItemsCount(null, null, false, true);
                                //}
                                //else
                                //{
                                //    await EventRefreshPageItems(true);
                                //}
                            }
                            else
                            {
                                //fix
                                //if (!SelectedList.Any(x => x.item_read_status == false))
                                //{
                                //    await EventRefreshPageItems(true);
                                //}
                            }
                        }
                    }
            }
            catch { }
        }

        //Mark item as read or unread
        private static async Task ListItemMarkRead(bool ForceRead, ListView SendListView, Items SelectedItem, ObservableCollection<Items> SelectedList, string CurrentPageName)
        {
            try
            {
                bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, ForceRead, false);
                if (MarkedRead && CurrentPageName.EndsWith("NewsPage"))
                {
                    if ((bool)AppSettingLoad("HideReadMarkedItem"))
                    {
                        //fix
                        //if (SendListView.Items.Any())
                        //{
                        //    //Update the header and selection feeds
                        //    await EventUpdateTotalItemsCount(null, null, false, true);
                        //}
                        //else
                        //{
                        //    await EventRefreshPageItems(true);
                        //}
                    }
                    else
                    {
                        //fix
                        //if (!SelectedList.Any(x => x.item_read_status == false))
                        //{
                        //    await EventRefreshPageItems(true);
                        //}
                    }
                }
            }
            catch { }
        }

        //Open item in browser
        private static async Task ListItemOpenBrowser(ListView SendListView, Items SelectedItem, ObservableCollection<Items> SelectedList, string CurrentPageName)
        {
            try
            {
                string messageResult = string.Empty;

                //Check internet connection
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Ok");

                    await AVMessageBox.Popup("No internet connection", "You currently don't have an internet connection available to open this item or link in the webviewer or your webbrowser.", messageAnswers);
                    return;
                }

                //Get the target uri
                Uri targetUri = new Uri(SelectedItem.item_link, UriKind.RelativeOrAbsolute);

                //Check webbrowser only links
                if (targetUri != null)
                {
                    string TargetString = targetUri.ToString();
                    if (!TargetString.StartsWith("http"))
                    {
                        messageResult = "Webbrowser (Device)";
                    }
                }

                if (messageResult != "Webbrowser (Device)")
                {
                    string LowMemoryWarning = string.Empty;
                    //fix if (AVFunctions.DevMemoryAvailableMB() < 200) { LowMemoryWarning = "\n\n* Your device is currently low on available memory and may cause issues when you open this link or item in the webviewer."; }

                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Webviewer (In-app)");
                    messageAnswers.Add("Webbrowser (Device)");
                    messageAnswers.Add("Cancel");

                    messageResult = await AVMessageBox.Popup("Open this item or link", "Do you want to open this item or link in the webviewer or your webbrowser?" + LowMemoryWarning, messageAnswers);
                }

                if (messageResult == "Webviewer (In-app)")
                {
                    // fix
                    //Open item in webviewer
                    //WebViewer webViewer = new WebViewer();
                    // await webViewer.OpenPopup(targetUri, SelectedItem);

                    //Mark the item as read
                    await ListItemMarkRead(true, SendListView, SelectedItem, SelectedList, CurrentPageName);
                }
                else if (messageResult == "Webbrowser (Device)")
                {
                    //Mark the item as read
                    await ListItemMarkRead(true, SendListView, SelectedItem, SelectedList, CurrentPageName);

                    //Open item in webbrowser
                    await Browser.OpenAsync(targetUri, BrowserLaunchMode.SystemPreferred);
                }
            }
            catch { }
        }
    }
}