using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NewsScroll
{
    class EventsScrollViewer
    {
        public static Items GetCurrentVisibleItem(ListView TargetListView, Int32 Offset)
        {
            try
            {
                if (TargetListView.Items.Any())
                {
                    VirtualizingStackPanel virtualStackPanel = AVFunctions.FindVisualChild<VirtualizingStackPanel>(TargetListView);
                    if (virtualStackPanel != null)
                    {
                        Int32 CurrentOffsetId = (virtualStackPanel.Orientation == Orientation.Horizontal) ? (Int32)virtualStackPanel.HorizontalOffset : (Int32)virtualStackPanel.VerticalOffset;
                        return TargetListView.Items[(CurrentOffsetId + Offset)] as Items;
                    }
                }
            }
            catch { }
            return null;
        }

        public static Double[] GetCurrentOffset(ListView TargetListView)
        {
            try
            {
                if (TargetListView.Items.Any())
                {
                    VirtualizingStackPanel virtualStackPanel = AVFunctions.FindVisualChild<VirtualizingStackPanel>(TargetListView);
                    if (virtualStackPanel != null) { return new double[] { virtualStackPanel.HorizontalOffset, virtualStackPanel.VerticalOffset }; }
                }
            }
            catch { }
            return new double[] { 0, 0 };
        }

        //Update the displayed item content
        public static async Task ScrollViewerUpdateContent(ListView TargetListView, Int32 CurrentOffsetId)
        {
            try
            {
                Int32 TargetItems = TargetListView.Items.Count();
                if (TargetItems > 0)
                {
                    //Update offset to the current position
                    Int32 OffsetIdNegative = (CurrentOffsetId - AppVariables.ContentToScrollLoad);
                    Int32 OffsetIdPositive = (CurrentOffsetId + AppVariables.ContentToScrollLoad);

                    //Cleanup item content
                    Int32 CheckIdRemove = 0;
                    //Debug.WriteLine("Clearing ListView item content...");
                    foreach (Items Item in TargetListView.Items)
                    {
                        if (!(CheckIdRemove >= OffsetIdNegative && CheckIdRemove <= OffsetIdPositive))
                        {
                            if (Item.item_image != null)
                            {
                                Item.item_image.UriSource = null;
                                Item.item_image = null;
                            }
                            if (Item.feed_icon != null)
                            {
                                Item.feed_icon.UriSource = null;
                                Item.feed_icon = null;
                            }
                        }
                        CheckIdRemove++;
                    }

                    //Load item content
                    //Debug.WriteLine("Loading ListView item content...");
                    for (Int32 i = OffsetIdNegative; i < OffsetIdPositive; i++)
                    {
                        if (i >= 0 && i < TargetItems)
                        {
                            Items AddItem = TargetListView.Items[i] as Items;
                            string ItemImageLink = AddItem.item_image_link;

                            if (AddItem.item_image == null && !String.IsNullOrWhiteSpace(ItemImageLink) && AddItem.item_image_visibility == Visibility.Visible)
                            {
                                AddItem.item_image = await AVImage.LoadBitmapImage(ItemImageLink, false);
                            }
                            if (AddItem.feed_icon == null)
                            {
                                //Load feed icon
                                if (AddItem.feed_id.StartsWith("user/")) { AddItem.feed_icon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconUser-Dark.png", false); } else { AddItem.feed_icon = await AVImage.LoadBitmapImage("ms-appdata:///local/" + AddItem.feed_id + ".png", false); }
                                if (AddItem.feed_icon == null) { AddItem.feed_icon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconRSS-Dark.png", false); }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        //Check if new items need to be loaded
        public static async Task ScrollViewerAddItems(ListView TargetListView, Int32 CurrentOffsetId)
        {
            try
            {
                if (!(bool)AppVariables.ApplicationSettings["LoadAllItems"])
                {
                    Int32 LoadedListCount = TargetListView.Items.Count();
                    if (!AppVariables.BusyApplication && (LoadedListCount - CurrentOffsetId) < AppVariables.ItemsToScrollLoad && LoadedListCount < AppVariables.CurrentTotalItemsCount)
                    {
                        AppVariables.BusyApplication = true;

                        //Debug.WriteLine("ItemsLoaded" + AppVariables.CurrentItemsLoaded + "/" + AppVariables.TotalItemsCount);
                        //Debug.WriteLine("Time to load new items because there are only " + AppVariables.ItemsToScrollLoad + " left.");
                        await ProcessItemLoad.DatabaseToList(null, null, LoadedListCount, AppVariables.ItemsToScrollLoad, true, false);

                        AppVariables.BusyApplication = false;
                    }
                }
            }
            catch { AppVariables.BusyApplication = false; }
        }
    }
}