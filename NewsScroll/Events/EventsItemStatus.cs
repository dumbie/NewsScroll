using ArnoldVinkMessageBox;
using System;
using System.Linq;
using System.Threading.Tasks;
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

                    Int32 MsgBoxResult = await AVMessageBox.Popup("Item scroller", "Would you like to scroll in the items?", "Scroll to beginning", "Scroll to the middle", "Scroll to the end", ReturnToPrevious, true);
                    if (MsgBoxResult == 1)
                    {
                        await Task.Delay(10);
                        TargetListView.ScrollIntoView(TargetListView.Items.First());
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
                        TargetListView.ScrollIntoView(TargetListView.Items.Last());
                        return true;
                    }
                    else if (MsgBoxResult == 4)
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