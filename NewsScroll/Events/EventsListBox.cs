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
                AppVariables.SingleTappedEvent = true;
                await Task.Delay(300);
                if (AppVariables.SingleTappedEvent)
                {
                    ListView SendListView = sender as ListView;
                    Items SelectedItem = ((e.OriginalSource as FrameworkElement).DataContext) as Items;
                    ObservableCollection<Items> SelectedList = (ObservableCollection<Items>)SendListView.ItemsSource;
                    if (SelectedItem != null)
                    {
                        bool IsNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
                        if (AppVariables.ApplicationSettings["ItemOpenMethod"].ToString() == "1" && IsNetworkAvailable)
                        {
                            WebViewer webViewer = new WebViewer();
                            await webViewer.OpenPopup(null, SelectedItem);

                            bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, true, true, false);
                            if (MarkedRead && (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"]) { await EventUpdateTotalItemsCount(null, null, false, true); }
                        }
                        else if (AppVariables.ApplicationSettings["ItemOpenMethod"].ToString() == "2" && IsNetworkAvailable)
                        {
                            await Launcher.LaunchUriAsync(new Uri(SelectedItem.item_link));

                            bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, true, true, false);
                            if (MarkedRead && (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"]) { await EventUpdateTotalItemsCount(null, null, false, true); }
                        }
                        else
                        {
                            ItemViewer itemViewer = new ItemViewer();
                            await itemViewer.OpenPopup(SelectedItem);

                            bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, true, true, false);
                            if (MarkedRead && (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"]) { await EventUpdateTotalItemsCount(null, null, false, true); }
                        }

                        //Reset the item selection
                        SendListView.SelectedIndex = -1;
                    }
                }
            }
            catch { }
        }

        public static async void ListView_Items_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                AppVariables.SingleTappedEvent = false;

                ListView SendListView = sender as ListView;
                Items SelectedItem = ((e.OriginalSource as FrameworkElement).DataContext) as Items;
                ObservableCollection<Items> SelectedList = (ObservableCollection<Items>)SendListView.ItemsSource;
                if (SelectedItem != null)
                {
                    //Get and set the current page name
                    string CurrentPageName = App.vApplicationFrame.SourcePageType.ToString();

                    bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, true, false, false, true);
                    if (MarkedRead && (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"] && CurrentPageName.EndsWith("NewsPage"))
                    {
                        if (SendListView.Items.Any())
                        {
                            await EventUpdateTotalItemsCount(null, null, false, true);
                        }
                        else
                        {
                            await EventRefreshPageItems(true);
                        }
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
                    string ActionMarkReadTill = String.Empty;
                    if (CurrentPageName.EndsWith("NewsPage") && NewsPage.vCurrentLoadingFeedFolder.feed_id != "1") { ActionMarkReadTill = "Mark read till item"; }

                    Int32 MsgBoxResult = await AVMessageBox.Popup("News item actions", "What would you like to do with this item?", "Share this item", ActionStarItem + " this item", "Mark item as " + ActionReadItem.ToLower(), ActionMarkReadTill, true);
                    if (MsgBoxResult == 1)
                    {
                        ShareItem(SelectedItem);
                    }
                    else if (MsgBoxResult == 2)
                    {
                        if (CurrentPageName.EndsWith("StarredPage"))
                        {
                            bool MarkedAsStar = await MarkItemAsStarPrompt(SelectedItem, false, true, false, false, true);
                            if (MarkedAsStar) { await EventUpdateTotalItemsCount(null, null, false, true); }
                        }
                        else { await MarkItemAsStarPrompt(SelectedItem, false, false, false, false, true); }
                    }
                    else if (MsgBoxResult == 3)
                    {
                        bool MarkedRead = await MarkItemAsReadPrompt(SelectedList, SelectedItem, false, false, false, true);
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
                    else if (MsgBoxResult == 4)
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
    }
}