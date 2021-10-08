using ArnoldVinkCode;
using NewsScroll.Classes;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static ArnoldVinkCode.AVFunctions;

namespace NewsScroll
{
    class EventsScrollViewer
    {
        //Get current offset item
        public static object GetCurrentOffsetItem(ListView TargetListView)
        {
            try
            {
                for (int i = 0; i < TargetListView.Items.Count; i++)
                {
                    if (ElementIsVisible(TargetListView.ContainerFromItem(TargetListView.Items[i]) as ListViewItem, TargetListView))
                    {
                        return TargetListView.Items[i];
                    }
                }
            }
            catch { }
            return null;
        }

        //Get current offset index
        public static int GetCurrentOffsetIndex(ListView TargetListView)
        {
            try
            {
                for (int i = 0; i < TargetListView.Items.Count; i++)
                {
                    if (ElementIsVisible(TargetListView.ContainerFromItem(TargetListView.Items[i]) as ListViewItem, TargetListView))
                    {
                        return i;
                    }
                }
            }
            catch { }
            return -1;
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
                int OffsetIdNegative = CurrentOffsetId - AppVariables.ContentToScrollLoad;
                int OffsetIdPositive = CurrentOffsetId + AppVariables.ContentToScrollLoad;

                //Load and clean item content
                for (int itemIndex = 0; itemIndex < targetItems; itemIndex++)
                {
                    Items updateItem = TargetListView.Items[itemIndex] as Items;
                    if (itemIndex >= OffsetIdNegative && itemIndex <= OffsetIdPositive)
                    {
                        string ItemImageLink = updateItem.item_image_link;
                        if (updateItem.item_image == null && !string.IsNullOrWhiteSpace(ItemImageLink) && updateItem.item_image_visibility == Visibility.Visible && AppVariables.LoadMedia)
                        {
                            updateItem.item_image = ItemImageLink;
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