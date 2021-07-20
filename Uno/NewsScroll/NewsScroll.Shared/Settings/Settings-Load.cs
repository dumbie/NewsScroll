using System;

namespace NewsScroll
{
    public partial class SettingsPage
    {
        private void SettingsLoad()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading application settings...");

                //Account and password
                setting_ApiAccount.Text = AppVariables.ApplicationSettings["ApiAccount"].ToString();
                setting_ApiPassword.Password = AppVariables.ApplicationSettings["ApiPassword"].ToString();

                //Update Items on Startup
                setting_UpdateItemsStartup.IsChecked = (bool)AppVariables.ApplicationSettings["UpdateItemsStartup"];

                //Display items author
                setting_DisplayItemsAuthor.IsChecked = (bool)AppVariables.ApplicationSettings["DisplayItemsAuthor"];

                //Display images offline
                setting_DisplayImagesOffline.IsChecked = (bool)AppVariables.ApplicationSettings["DisplayImagesOffline"];

                //Display Read Marked items
                setting_DisplayReadMarkedItems.IsChecked = (bool)AppVariables.ApplicationSettings["DisplayReadMarkedItems"];

                //Hide Read Marked item
                setting_HideReadMarkedItem.IsChecked = (bool)AppVariables.ApplicationSettings["HideReadMarkedItem"];

                //News item content cutting
                setting_ContentCutting.IsChecked = (bool)AppVariables.ApplicationSettings["ContentCutting"];

                //News item content cutting length
                setting_ContentCuttingLength.Text = AppVariables.ApplicationSettings["ContentCuttingLength"].ToString();

                //Load all available items
                setting_LoadAllItems.IsChecked = (bool)AppVariables.ApplicationSettings["LoadAllItems"];

                //Low bandwidth mode
                setting_LowBandwidthMode.IsChecked = (bool)AppVariables.ApplicationSettings["LowBandwidthMode"];

                //Default item open method
                setting_ItemOpenMethod.SelectedIndex = Convert.ToInt32(AppVariables.ApplicationSettings["ItemOpenMethod"]);

                //Remove Items Range
                if (Convert.ToInt32(AppVariables.ApplicationSettings["RemoveItemsRange"]) == 2)
                {
                    setting_RemoveItemsRange.SelectedIndex = 0;
                }
                else if (Convert.ToInt32(AppVariables.ApplicationSettings["RemoveItemsRange"]) == 4)
                {
                    setting_RemoveItemsRange.SelectedIndex = 1;
                }
                else if (Convert.ToInt32(AppVariables.ApplicationSettings["RemoveItemsRange"]) == 7)
                {
                    setting_RemoveItemsRange.SelectedIndex = 2;
                }
                else if (Convert.ToInt32(AppVariables.ApplicationSettings["RemoveItemsRange"]) == 14)
                {
                    setting_RemoveItemsRange.SelectedIndex = 3;
                }
                else if (Convert.ToInt32(AppVariables.ApplicationSettings["RemoveItemsRange"]) == 28)
                {
                    setting_RemoveItemsRange.SelectedIndex = 4;
                }
                else if (Convert.ToInt32(AppVariables.ApplicationSettings["RemoveItemsRange"]) == 56)
                {
                    setting_RemoveItemsRange.SelectedIndex = 5;
                }
                else if (Convert.ToInt32(AppVariables.ApplicationSettings["RemoveItemsRange"]) == 84)
                {
                    setting_RemoveItemsRange.SelectedIndex = 6;
                }

                //Disable Swipe Action
                setting_DisableSwipeActions.IsChecked = (bool)AppVariables.ApplicationSettings["DisableSwipeActions"];

                //Swipe direction
                setting_SwipeDirection.SelectedIndex = Convert.ToInt32(AppVariables.ApplicationSettings["SwipeDirection"]);

                ////Maximum Update Items
                //if (Convert.ToInt32(AppVariables.ApplicationSettings["MaxUpdateItems"]) == 250)
                //{
                //    setting_MaxUpdateItems.SelectedIndex = 0;
                //}
                //else if (Convert.ToInt32(AppVariables.ApplicationSettings["MaxUpdateItems"]) == 500)
                //{
                //    setting_MaxUpdateItems.SelectedIndex = 1;
                //}
                //else if (Convert.ToInt32(AppVariables.ApplicationSettings["MaxUpdateItems"]) == 1000)
                //{
                //    setting_MaxUpdateItems.SelectedIndex = 2;
                //}
                //else if (Convert.ToInt32(AppVariables.ApplicationSettings["MaxUpdateItems"]) == 1500)
                //{
                //    setting_MaxUpdateItems.SelectedIndex = 3;
                //}
                //else if (Convert.ToInt32(AppVariables.ApplicationSettings["MaxUpdateItems"]) == 2000)
                //{
                //    setting_MaxUpdateItems.SelectedIndex = 4;
                //}
            }
            catch { }
        }
    }
}