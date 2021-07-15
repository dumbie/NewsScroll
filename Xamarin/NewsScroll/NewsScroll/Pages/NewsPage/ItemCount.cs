using static NewsScroll.Api.Api;
using static NewsScroll.AppVariables;

namespace NewsScroll
{
    public partial class NewsPage
    {
        //Update the total item count
        private void UpdateTotalItemsCount()
        {
            try
            {
                AppVariables.CurrentTotalItemsCount = vNewsFeed.feed_item_count;
                if (AppVariables.CurrentTotalItemsCount > 0)
                {
                    txt_AppInfo.Text = ApiMessageError + AppVariables.CurrentTotalItemsCount + " items";
                    txt_NewsScrollInfo.IsVisible = false;

                    button_StatusCurrentItem.IsVisible = true;
                }
                else
                {
                    txt_AppInfo.Text = ApiMessageError + "No items";
                    txt_NewsScrollInfo.Text = "No news items are displayed because some feeds might currently be ignored or the selected feed or folder does not contain any (unread) news items at the moment.";
                    txt_NewsScrollInfo.IsVisible = true;

                    button_StatusCurrentItem.IsVisible = false;
                }

                //Update the current item count
                if (stackpanel_Header.IsVisible || AppVariables.CurrentTotalItemsCount == 0)
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount.ToString();
                }
                else
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount + "/" + AppVariables.CurrentTotalItemsCount;
                }
            }
            catch { }
        }
    }
}