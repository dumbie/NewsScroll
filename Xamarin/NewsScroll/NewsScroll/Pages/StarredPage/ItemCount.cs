using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using static NewsScroll.Api.Api;
using static NewsScroll.Database.Database;

namespace NewsScroll
{
    public partial class StarredPage
    {
        //Update the total items count
        private async Task UpdateTotalItemsCount(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, bool Silent, bool EnableUI)
        {
            try
            {
                //Set the total item count
                AppVariables.CurrentTotalItemsCount = await ProcessItemLoad.DatabaseToCount(LoadTableFeeds, LoadTableItems, Silent, EnableUI);
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        //Check the total item count
                        if (AppVariables.CurrentTotalItemsCount > 0)
                        {
                            txt_AppInfo.Text = ApiMessageError + AppVariables.CurrentTotalItemsCount + " items";
                            txt_NewsScrollInfo.IsVisible = false;

                            button_StatusCurrentItem.IsVisible = true;
                        }
                        else
                        {
                            txt_AppInfo.Text = ApiMessageError + "No items";
                            txt_NewsScrollInfo.Text = "It seems like you don't have any starred items added to your account, please add some starred items to start loading your items and click on the refresh button above.";
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
                });
            }
            catch { }
        }
    }
}