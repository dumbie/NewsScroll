using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static ArnoldVinkCode.AVFunctions;

namespace NewsScroll
{
    public partial class SearchPage
    {
        //Monitor and handle the scroll viewer
        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                //Get current scroll item
                int CurrentOffSetId = -1;
                for (int i = 0; i < ListView_Items.Items.Count; i++)
                {
                    if (ElementIsVisible(ListView_Items.ContainerFromItem(ListView_Items.Items[i]) as ListViewItem, ListView_Items))
                    {
                        CurrentOffSetId = i;
                        break;
                    }
                }
                if (CurrentOffSetId < 0) { return; }

                //Update the current item count
                AppVariables.CurrentShownItemCount = CurrentOffSetId + 1;
                if (stackpanel_Header.Visibility == Visibility.Visible || AppVariables.CurrentTotalItemsCount == 0)
                {
                    textblock_StatusCurrentItem.Text = AppVariables.CurrentShownItemCount.ToString();
                }
                else
                {
                    textblock_StatusCurrentItem.Text = AppVariables.CurrentShownItemCount + "/" + AppVariables.CurrentTotalItemsCount;
                }

                //Update the shown item content
                await EventsScrollViewer.ScrollViewerUpdateContent(ListView_Items, CurrentOffSetId);
            }
            catch { }
        }
    }
}