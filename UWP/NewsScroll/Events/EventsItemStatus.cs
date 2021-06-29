using ArnoldVinkMessageBox;
using NewsScroll.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NewsScroll
{
    class EventsItemStatus
    {
        public static async Task<bool> ListViewScroller(ListView TargetListView, Int32 CurrentItemId, Int32 PreviousItemId)
        {
            try
            {
                Int32 TargetListViewItems = TargetListView.Items.Count();
                if (TargetListViewItems > 0)
                {
                    string ReturnToPrevious = String.Empty;
                    if (PreviousItemId != 0 && TargetListViewItems > PreviousItemId) { ReturnToPrevious = "Scroll back to " + PreviousItemId; }

                    Int32 MsgBoxResult = await AVMessageBox.Popup("Item scroller", "Would you like to scroll in the items?", "Scroll to beginning", "Scroll to the middle", "Scroll to the end", "Scroll to unread item", ReturnToPrevious, true);
                    if (MsgBoxResult == 1)
                    {
                        await Task.Delay(10);
                        TargetListView.ScrollIntoView(TargetListView.Items.FirstOrDefault());
                        return true;
                    }
                    else if (MsgBoxResult == 2)
                    {
                        await Task.Delay(10);
                        TargetListView.ScrollIntoView(TargetListView.Items[(TargetListView.Items.Count / 2)]);
                        return true;
                    }
                    else if (MsgBoxResult == 3)
                    {
                        await Task.Delay(10);
                        TargetListView.ScrollIntoView(TargetListView.Items.LastOrDefault());
                        return true;
                    }
                    else if (MsgBoxResult == 4)
                    {
                        await Task.Delay(10);
                        IEnumerable<Items> itemsList = TargetListView.Items.OfType<Items>();
                        Items scrollItem = itemsList.Where(x => x.item_read_status == Visibility.Collapsed).FirstOrDefault();
                        if (scrollItem != null)
                        {
                            TargetListView.ScrollIntoView(scrollItem);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (MsgBoxResult == 5)
                    {
                        await Task.Delay(10);
                        TargetListView.ScrollIntoView(TargetListView.Items[PreviousItemId]);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}