using Xamarin.Forms;
using static NewsScroll.Process;

namespace NewsScroll
{
    public partial class NewsPage
    {
        //Listview item appearing
        private async void ScrollViewer_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            try
            {
                //Update the current item count
                int CurrentOffSetId = e.ItemIndex;
                AppVariables.CurrentViewItemsCount = CurrentOffSetId;

                if (stackpanel_Header.IsVisible || AppVariables.CurrentTotalItemsCount == 0)
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount.ToString();
                }
                else
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount + "/" + AppVariables.CurrentTotalItemsCount;
                }

                //Update the shown item content
                await ItemUpdateImages(e.Item, false);
            }
            catch { }
        }

        //Listview item disappearing
        private async void ScrollViewer_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
            try
            {
                //Update the current item count
                int CurrentOffSetId = e.ItemIndex;
                AppVariables.CurrentViewItemsCount = CurrentOffSetId;

                if (stackpanel_Header.IsVisible || AppVariables.CurrentTotalItemsCount == 0)
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount.ToString();
                }
                else
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount + "/" + AppVariables.CurrentTotalItemsCount;
                }

                //Update the shown item content
                await ItemUpdateImages(e.Item, true);
            }
            catch { }
        }
    }
}