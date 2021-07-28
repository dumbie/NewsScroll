using ArnoldVinkCode;
using NewsScroll.Classes;
using System.Collections.Generic;
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
                //Get total items count
                int targetItems = TargetListView.Items.Count();

                //Check total items count
                if (targetItems == 0) { return; }

                //Update offset to the current position
                int OffsetIdNegative = (CurrentOffsetId - AppVariables.ContentToScrollLoad);
                int OffsetIdPositive = (CurrentOffsetId + AppVariables.ContentToScrollLoad);

                //Load and clean item content
                for (int itemIndex = 0; itemIndex < targetItems; itemIndex++)
                {
                    Items updateItem = TargetListView.Items[itemIndex] as Items;
                    if (itemIndex >= OffsetIdNegative && itemIndex <= OffsetIdPositive)
                    {
                        string ItemImageLink = updateItem.item_image_link;
                        if (updateItem.item_image == null && !string.IsNullOrWhiteSpace(ItemImageLink) && updateItem.item_image_visibility == Visibility.Visible && AppVariables.LoadMedia)
                        {
                            updateItem.item_image = await AVImage.LoadBitmapImage(ItemImageLink, false);
                            System.Diagnostics.Debug.WriteLine("Loaded item image: " + ItemImageLink);
                        }
                        if (updateItem.feed_icon == null)
                        {
                            if (updateItem.feed_id.StartsWith("user/"))
                            {
                                updateItem.feed_icon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconUser-Dark.png", false);
                            }
                            else
                            {
                                updateItem.feed_icon = await AVImage.LoadBitmapImage("ms-appdata:///local/" + updateItem.feed_id + ".png", false);
                            }
                            if (updateItem.feed_icon == null)
                            {
                                updateItem.feed_icon = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconRSS-Dark.png", false);
                            }
                        }
                    }
                    else
                    {
                        if (updateItem.item_image != null)
                        {
                            updateItem.item_image = null;
                        }
                        if (updateItem.feed_icon != null)
                        {
                            updateItem.feed_icon = null;
                        }
                    }
                }
            }
            catch { }
        }
    }
}