﻿using ArnoldVinkCode;
using ArnoldVinkMessageBox;
using NewsScroll.Classes;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static NewsScroll.Api.Api;
using static NewsScroll.Events.Events;

namespace NewsScroll
{
    class EventsListView
    {
        public static async void ListView_Items_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                ListView SendListView = sender as ListView;
                Items SelectedItem = ((e.OriginalSource as FrameworkElement).DataContext) as Items;
                ObservableCollection<Items> SelectedList = (ObservableCollection<Items>)SendListView.ItemsSource;
                if (SelectedItem != null)
                {
                    bool IsNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
                    if (AppVariables.ApplicationSettings["ItemOpenMethod"].ToString() == "1" && IsNetworkAvailable)
                    {
                        //Open item in webviewer
                        WebViewer webViewer = new WebViewer();
                        await webViewer.OpenPopup(null, SelectedItem);

                        //Mark the item as read
                        bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, true, true);
                        if (MarkedRead && (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"]) { await EventUpdateTotalItemsCount(null, null, false, true); }
                    }
                    else if (AppVariables.ApplicationSettings["ItemOpenMethod"].ToString() == "2" && IsNetworkAvailable)
                    {
                        //Mark the item as read
                        bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, true, false);
                        if (MarkedRead && (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"]) { await EventUpdateTotalItemsCount(null, null, false, true); }

                        //Open item in webbrowser
                        await Launcher.LaunchUriAsync(new Uri(SelectedItem.item_link, UriKind.RelativeOrAbsolute));
                    }
                    else
                    {
                        //Open item in itemviewer
                        ItemViewer itemViewer = new ItemViewer();
                        await itemViewer.OpenPopup(SelectedItem);

                        //Mark the item as read
                        bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, true, true);
                        if (MarkedRead && (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"]) { await EventUpdateTotalItemsCount(null, null, false, true); }
                    }

                    //Reset the item selection
                    SendListView.SelectedIndex = -1;
                }
            }
            catch { }
        }

        public static async void ListView_Items_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                ListView SendListView = sender as ListView;
                Items SelectedItem = ((e.OriginalSource as FrameworkElement).DataContext) as Items;
                ObservableCollection<Items> SelectedList = (ObservableCollection<Items>)SendListView.ItemsSource;
                if (SelectedItem != null)
                {
                    //Set the action string text
                    string ActionStarItem = "Un/star";
                    if (SelectedItem.item_star_status == Visibility.Visible) { ActionStarItem = "Unstar"; } else { ActionStarItem = "Star"; }

                    //Set the action string text
                    string ActionReadItem = "Un/read";
                    if (SelectedItem.item_read_status == Visibility.Visible) { ActionReadItem = "Unread"; } else { ActionReadItem = "Read"; }

                    //Get and set the current page name
                    string CurrentPageName = App.vApplicationFrame.SourcePageType.ToString();

                    //Check if mark read till is enabled
                    string ActionMarkReadTill = string.Empty;
                    if (CurrentPageName.EndsWith("NewsPage") && NewsPage.vCurrentLoadingFeedFolder.feed_id != "1") { ActionMarkReadTill = "Mark read till item"; }

                    int MsgBoxResult = await AVMessageBox.Popup("News item actions", "What would you like to do with this item?", "Open in browser", "Share this item", ActionStarItem + " this item", "Mark item as " + ActionReadItem.ToLower(), ActionMarkReadTill, true);
                    if (MsgBoxResult == 1)
                    {
                        await ListItemOpenBrowser(SendListView, SelectedItem, SelectedList, CurrentPageName);
                    }
                    else if (MsgBoxResult == 2)
                    {
                        ShareItem(SelectedItem);
                    }
                    else if (MsgBoxResult == 3)
                    {
                        if (CurrentPageName.EndsWith("StarredPage"))
                        {
                            bool MarkedAsStar = await MarkItemAsStarPrompt(SelectedItem, false, true, false, false, true);
                            if (MarkedAsStar) { await EventUpdateTotalItemsCount(null, null, false, true); }
                        }
                        else
                        {
                            await MarkItemAsStarPrompt(SelectedItem, false, false, false, false, true);
                        }
                    }
                    else if (MsgBoxResult == 4)
                    {
                        await ListItemMarkRead(false, SendListView, SelectedItem, SelectedList, CurrentPageName);
                    }
                    else if (MsgBoxResult == 5)
                    {
                        bool MarkedRead = await MarkReadTill(SelectedList, SelectedItem, true, false, true);
                        if (MarkedRead && CurrentPageName.EndsWith("NewsPage"))
                        {
                            if ((bool)AppVariables.ApplicationSettings["HideReadMarkedItem"])
                            {
                                if (SendListView.Items.Any())
                                {
                                    //Scroll to the ListView top
                                    await Task.Delay(10);
                                    SendListView.ScrollIntoView(SendListView.Items.First());

                                    //Update the header and selection feeds
                                    await EventUpdateTotalItemsCount(null, null, false, true);
                                }
                                else
                                {
                                    await EventRefreshPageItems(true);
                                }
                            }
                            else
                            {
                                if (!SelectedList.Any(x => x.item_read_status == Visibility.Collapsed))
                                {
                                    await EventRefreshPageItems(true);
                                }
                            }
                        }
                    }

                    //Reset the item selection
                    SendListView.SelectedIndex = -1;
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
                    if ((bool)AppVariables.ApplicationSettings["HideReadMarkedItem"])
                    {
                        if (SendListView.Items.Any())
                        {
                            //Update the header and selection feeds
                            await EventUpdateTotalItemsCount(null, null, false, true);
                        }
                        else
                        {
                            await EventRefreshPageItems(true);
                        }
                    }
                    else
                    {
                        if (!SelectedList.Any(x => x.item_read_status == Visibility.Collapsed))
                        {
                            await EventRefreshPageItems(true);
                        }
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
                int MsgBoxResult = 0;

                //Check internet connection
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    await AVMessageBox.Popup("No internet connection", "You currently don't have an internet connection available to open this item or link in the webviewer or your webbrowser.", "Ok", "", "", "", "", false);
                    return;
                }

                //Get the target uri
                Uri targetUri = new Uri(SelectedItem.item_link, UriKind.RelativeOrAbsolute);

                //Check webbrowser only links
                if (targetUri != null)
                {
                    string TargetString = targetUri.ToString();
                    if (!TargetString.StartsWith("http")) { MsgBoxResult = 2; }
                }

                if (MsgBoxResult != 2)
                {
                    string LowMemoryWarning = string.Empty;
                    if (AVFunctions.DevMemoryAvailableMB() < 200) { LowMemoryWarning = "\n\n* Your device is currently low on available memory and may cause issues when you open this link or item in the webviewer."; }
                    MsgBoxResult = await AVMessageBox.Popup("Open this item or link", "Do you want to open this item or link in the webviewer or your webbrowser?" + LowMemoryWarning, "Webviewer (In-app)", "Webbrowser (Device)", "", "", "", true);
                }

                if (MsgBoxResult == 1)
                {
                    //Open item in webviewer
                    WebViewer webViewer = new WebViewer();
                    await webViewer.OpenPopup(targetUri, SelectedItem);

                    //Mark the item as read
                    await ListItemMarkRead(true, SendListView, SelectedItem, SelectedList, CurrentPageName);
                }
                else if (MsgBoxResult == 2)
                {
                    //Mark the item as read
                    await ListItemMarkRead(true, SendListView, SelectedItem, SelectedList, CurrentPageName);

                    //Open item in webbrowser
                    await Launcher.LaunchUriAsync(targetUri);
                }
            }
            catch { }
        }
    }
}