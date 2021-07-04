using NewsScroll.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NewsScroll
{
    class EventsItemStatus
    {
        public static async Task<bool> ListViewScroller(ListView TargetListView, int CurrentItemId, int PreviousItemId)
        {
            try
            {
                ObservableCollection<Items> SelectedList = (ObservableCollection<Items>)TargetListView.ItemsSource;
                Int32 TargetListViewItems = SelectedList.Count;
                if (TargetListViewItems > 0)
                {
                    List<string> messageAnswers = new List<string>();
                    messageAnswers.Add("Scroll to beginning");
                    messageAnswers.Add("Scroll to the middle");
                    messageAnswers.Add("Scroll to the end");
                    messageAnswers.Add("Scroll to unread item");
                    string stringReturnToPrevious = String.Empty;
                    if (PreviousItemId != 0 && TargetListViewItems > PreviousItemId)
                    {
                        stringReturnToPrevious = "Scroll back to " + PreviousItemId;
                        messageAnswers.Add(stringReturnToPrevious);
                    }
                    messageAnswers.Add("Cancel");

                    string messageResult = await AVMessageBox.Popup("Item scroller", "Would you like to scroll in the items?", messageAnswers);
                    if (messageResult == "Scroll to beginning")
                    {
                        await Task.Delay(10);
                        TargetListView.ScrollTo(SelectedList.FirstOrDefault(), ScrollToPosition.MakeVisible, false);
                        return true;
                    }
                    else if (messageResult == "Scroll to the middle")
                    {
                        await Task.Delay(10);
                        TargetListView.ScrollTo(SelectedList[TargetListViewItems / 2], ScrollToPosition.MakeVisible, false);
                        return true;
                    }
                    else if (messageResult == "Scroll to the end")
                    {
                        await Task.Delay(10);
                        TargetListView.ScrollTo(SelectedList.LastOrDefault(), ScrollToPosition.MakeVisible, false);
                        return true;
                    }
                    else if (messageResult == "Scroll to unread item")
                    {
                        await Task.Delay(10);
                        Items scrollItem = SelectedList.Where(x => x.item_read_status == false).FirstOrDefault();
                        if (scrollItem != null)
                        {
                            TargetListView.ScrollTo(scrollItem, ScrollToPosition.MakeVisible, false);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (messageResult == stringReturnToPrevious)
                    {
                        await Task.Delay(10);
                        TargetListView.ScrollTo(SelectedList[PreviousItemId], ScrollToPosition.MakeVisible, false);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}