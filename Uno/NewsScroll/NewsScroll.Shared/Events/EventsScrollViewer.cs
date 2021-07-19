using ArnoldVinkCode;
using NewsScroll.Classes;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NewsScroll
{
    class EventsScrollViewer
    {
        public static double[] GetCurrentOffset(ListView TargetListView)
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
        public static async Task ScrollViewerUpdateContent(ListView TargetListView, int CurrentOffsetId)
        {
            try
            {
                int TargetItems = TargetListView.Items.Count();
                if (TargetItems > 0)
                {
                    //Update offset to the current position
                    int OffsetIdNegative = (CurrentOffsetId - AppVariables.ContentToScrollLoad);
                    int OffsetIdPositive = (CurrentOffsetId + AppVariables.ContentToScrollLoad);

                    //Cleanup item content
                    int CheckIdRemove = 0;
                    //System.Diagnostics.Debug.WriteLine("Clearing ListView item content...");
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
                    //System.Diagnostics.Debug.WriteLine("Loading ListView item content...");
                    for (int i = OffsetIdNegative; i < OffsetIdPositive; i++)
                    {
                        if (i >= 0 && i < TargetItems)
                        {
                            Items AddItem = TargetListView.Items[i] as Items;
                            string ItemImageLink = AddItem.item_image_link;

                            if (AddItem.item_image == null && !string.IsNullOrWhiteSpace(ItemImageLink) && AddItem.item_image_visibility == Visibility.Visible)
                            {
                                AddItem.item_image = await AVImage.LoadBitmapImage(ItemImageLink, false);
                                System.Diagnostics.Debug.WriteLine("Loaded item image: " + ItemImageLink);
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
        public static async Task ScrollViewerAddItems(ListView TargetListView, int CurrentOffsetId)
        {
            try
            {
                if (!(bool)AppVariables.ApplicationSettings["LoadAllItems"])
                {
                    int LoadedListCount = TargetListView.Items.Count();
                    if (!AppVariables.BusyApplication && (LoadedListCount - CurrentOffsetId) < AppVariables.ItemsToScrollLoad && LoadedListCount < AppVariables.CurrentTotalItemsCount)
                    {
                        AppVariables.BusyApplication = true;

                        //System.Diagnostics.Debug.WriteLine("ItemsLoaded" + AppVariables.CurrentItemsLoaded + "/" + AppVariables.TotalItemsCount);
                        //System.Diagnostics.Debug.WriteLine("Time to load new items because there are only " + AppVariables.ItemsToScrollLoad + " left.");
                        await ProcessItemLoad.DatabaseToList(null, null, LoadedListCount, AppVariables.ItemsToScrollLoad, true, false);

                        AppVariables.BusyApplication = false;
                    }
                }
            }
            catch { AppVariables.BusyApplication = false; }
        }
    }
}