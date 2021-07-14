using System;
using System.Diagnostics;
using static ArnoldVinkCode.ArnoldVinkSettings;

namespace NewsScroll
{
    public partial class SettingsPage
    {
        private void SettingsLoad()
        {
            try
            {
                Debug.WriteLine("Loading application settings...");

                //Account and password
                setting_ApiAccount.Text = AppSettingLoad("ApiAccount").ToString();
                setting_ApiPassword.Text = AppSettingLoad("ApiPassword").ToString();

                //Display items author
                setting_DisplayItemsAuthor.IsChecked = (bool)AppSettingLoad("DisplayItemsAuthor");

                //Display images offline
                setting_DisplayImagesOffline.IsChecked = (bool)AppSettingLoad("DisplayImagesOffline");

                //Display Read Marked items
                setting_DisplayReadMarkedItems.IsChecked = (bool)AppSettingLoad("DisplayReadMarkedItems");

                //News item content cutting
                setting_ContentCutting.IsChecked = (bool)AppSettingLoad("ContentCutting");

                //News item content cutting length
                setting_ContentCuttingLength.Text = AppSettingLoad("ContentCuttingLength").ToString();

                //Enable item text selection
                setting_ItemTextSelection.IsChecked = (bool)AppSettingLoad("ItemTextSelection");

                //Low bandwidth mode
                setting_LowBandwidthMode.IsChecked = (bool)AppSettingLoad("LowBandwidthMode");

                //Default item open method
                setting_ItemOpenMethod.SelectedIndex = Convert.ToInt32(AppSettingLoad("ItemOpenMethod"));

                //Remove Items Range
                if (Convert.ToInt32(AppSettingLoad("RemoveItemsRange")) == 2)
                {
                    setting_RemoveItemsRange.SelectedIndex = 0;
                }
                else if (Convert.ToInt32(AppSettingLoad("RemoveItemsRange")) == 4)
                {
                    setting_RemoveItemsRange.SelectedIndex = 1;
                }
                else if (Convert.ToInt32(AppSettingLoad("RemoveItemsRange")) == 7)
                {
                    setting_RemoveItemsRange.SelectedIndex = 2;
                }
                else if (Convert.ToInt32(AppSettingLoad("RemoveItemsRange")) == 14)
                {
                    setting_RemoveItemsRange.SelectedIndex = 3;
                }
                else if (Convert.ToInt32(AppSettingLoad("RemoveItemsRange")) == 28)
                {
                    setting_RemoveItemsRange.SelectedIndex = 4;
                }
                else if (Convert.ToInt32(AppSettingLoad("RemoveItemsRange")) == 56)
                {
                    setting_RemoveItemsRange.SelectedIndex = 5;
                }
                else if (Convert.ToInt32(AppSettingLoad("RemoveItemsRange")) == 84)
                {
                    setting_RemoveItemsRange.SelectedIndex = 6;
                }

                //Disable Swipe Action
                setting_DisableSwipeActions.IsChecked = (bool)AppSettingLoad("DisableSwipeActions");

                //Swipe direction
                setting_SwipeDirection.SelectedIndex = Convert.ToInt32(AppSettingLoad("SwipeDirection"));
            }
            catch { }
        }
    }
}