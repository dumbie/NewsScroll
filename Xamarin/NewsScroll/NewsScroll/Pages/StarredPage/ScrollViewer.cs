using ArnoldVinkCode;
using Xamarin.Forms;
using static NewsScroll.Process;

namespace NewsScroll
{
    public partial class StarredPage
    {
        //Listview item appearing
        private void ScrollViewer_ItemAppearing(object sender, ItemVisibilityEventArgs e)
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
                async void TaskAction()
                {
                    await ItemUpdateImages(e.Item, false);
                }
                AVActions.TaskStart(TaskAction);
            }
            catch { }
        }

        //Listview item disappearing
        private void ScrollViewer_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
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
                async void TaskAction()
                {
                    await ItemUpdateImages(e.Item, true);
                }
                AVActions.TaskStart(TaskAction);
            }
            catch { }
        }
    }
}