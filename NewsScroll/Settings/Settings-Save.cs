using System;
using System.Text.RegularExpressions;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static NewsScroll.Api.Api;

namespace NewsScroll
{
    public partial class SettingsPage
    {
        private void SettingsSave()
        {
            //Account username
            setting_ApiAccount.KeyUp += (sender, e) =>
            {
                //Hide the keyboard
                if (e.Key == VirtualKey.Enter) { InputPane.GetForCurrentView().TryHide(); }

                //Reset the online status
                OnlineUpdateFeeds = true;
                OnlineUpdateNews = true;
                OnlineUpdateStarred = true;
                ApiMessageError = String.Empty;
                AppVariables.ApplicationSettings["ConnectApiAuth"] = String.Empty;
                AppVariables.ApplicationSettings["ApiAccount"] = setting_ApiAccount.Text;
            };

            //Account password
            setting_ApiPassword.KeyUp += (sender, e) =>
            {
                //Hide the keyboard
                if (e.Key == VirtualKey.Enter) { InputPane.GetForCurrentView().TryHide(); }

                //Reset the online status
                OnlineUpdateFeeds = true;
                OnlineUpdateNews = true;
                OnlineUpdateStarred = true;
                ApiMessageError = String.Empty;
                AppVariables.ApplicationSettings["ConnectApiAuth"] = String.Empty;
                AppVariables.ApplicationSettings["ApiPassword"] = setting_ApiPassword.Password;
            };

            //Update items on Startup
            setting_UpdateItemsStartup.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked) { AppVariables.ApplicationSettings["UpdateItemsStartup"] = true; }
                else { AppVariables.ApplicationSettings["UpdateItemsStartup"] = false; }
            };

            //Display items author
            setting_DisplayItemsAuthor.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked) { AppVariables.ApplicationSettings["DisplayItemsAuthor"] = true; }
                else { AppVariables.ApplicationSettings["DisplayItemsAuthor"] = false; }
            };

            //Display images offline
            setting_DisplayImagesOffline.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked) { AppVariables.ApplicationSettings["DisplayImagesOffline"] = true; }
                else { AppVariables.ApplicationSettings["DisplayImagesOffline"] = false; }
            };

            //Display Read Marked items
            setting_DisplayReadMarkedItems.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked) { AppVariables.ApplicationSettings["DisplayReadMarkedItems"] = true; }
                else { AppVariables.ApplicationSettings["DisplayReadMarkedItems"] = false; }
            };

            //Hide Read Marked item
            setting_HideReadMarkedItem.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked) { AppVariables.ApplicationSettings["HideReadMarkedItem"] = true; }
                else { AppVariables.ApplicationSettings["HideReadMarkedItem"] = false; }
            };

            //News item content cutting
            setting_ContentCutting.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked) { AppVariables.ApplicationSettings["ContentCutting"] = true; }
                else { AppVariables.ApplicationSettings["ContentCutting"] = false; }
            };

            //News item content cutting length
            setting_ContentCuttingLength.KeyUp += (sender, e) =>
            {
                //Hide the keyboard
                if (e.Key == VirtualKey.Enter) { InputPane.GetForCurrentView().TryHide(); }

                Color ColorInvalid = Color.FromArgb(255, 205, 25, 40);
                SolidColorBrush BrushInvalid = new SolidColorBrush(ColorInvalid);

                Color ColorValid = Color.FromArgb(255, 30, 185, 85);
                SolidColorBrush BrushValid = new SolidColorBrush(ColorValid);

                //Check for numbers
                if (!Regex.IsMatch(setting_ContentCuttingLength.Text, "^[0-9]+$"))
                {
                    setting_ContentCuttingLength.BorderBrush = BrushInvalid;
                    return;
                }

                //Check content length
                Int32 NewSetting = Convert.ToInt32(setting_ContentCuttingLength.Text);
                if (NewSetting < 10)
                {
                    setting_ContentCuttingLength.BorderBrush = BrushInvalid;
                    return;
                }
                else if (NewSetting > AppVariables.MaximumItemTextLength)
                {
                    setting_ContentCuttingLength.BorderBrush = BrushInvalid;
                    return;
                }

                AppVariables.ApplicationSettings["ContentCuttingLength"] = NewSetting;
                setting_ContentCuttingLength.BorderBrush = BrushValid;
            };

            //Load all available items
            setting_LoadAllItems.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked)
                {
                    AppVariables.ApplicationSettings["LoadAllItems"] = true;
                    AppVariables.ItemsToScrollLoad = 100000;
                }
                else
                {
                    AppVariables.ApplicationSettings["LoadAllItems"] = false;
                    AppVariables.ItemsToScrollLoad = 100;
                }
            };

            //Enable item text selection
            setting_ItemTextSelection.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked)
                {
                    AppVariables.ApplicationSettings["ItemTextSelection"] = true;
                }
                else
                {
                    AppVariables.ApplicationSettings["ItemTextSelection"] = false;
                }
            };

            //Low bandwidth mode
            setting_LowBandwidthMode.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked) { AppVariables.ApplicationSettings["LowBandwidthMode"] = true; }
                else { AppVariables.ApplicationSettings["LowBandwidthMode"] = false; }
            };

            //Disable Swipe Action
            setting_DisableSwipeActions.Click += (sender, e) =>
            {
                CheckBox CheckBox = (CheckBox)sender;
                if ((bool)CheckBox.IsChecked) { AppVariables.ApplicationSettings["DisableSwipeActions"] = true; }
                else { AppVariables.ApplicationSettings["DisableSwipeActions"] = false; }
            };

            //Swipe direction
            setting_SwipeDirection.SelectionChanged += (sender, e) =>
            {
                ComboBox ComboBox = (ComboBox)sender;
                AppVariables.ApplicationSettings["SwipeDirection"] = ComboBox.SelectedIndex;
            };

            //Default item open method
            setting_ItemOpenMethod.SelectionChanged += (sender, e) =>
            {
                ComboBox ComboBox = (ComboBox)sender;
                AppVariables.ApplicationSettings["ItemOpenMethod"] = ComboBox.SelectedIndex;
            };

            //Remove Items Range
            setting_RemoveItemsRange.SelectionChanged += (sender, e) =>
            {
                ComboBox ComboBox = (ComboBox)sender;
                if (ComboBox.SelectedIndex == 0) { AppVariables.ApplicationSettings["RemoveItemsRange"] = 2; }
                else if (ComboBox.SelectedIndex == 1) { AppVariables.ApplicationSettings["RemoveItemsRange"] = 4; }
                else if (ComboBox.SelectedIndex == 2) { AppVariables.ApplicationSettings["RemoveItemsRange"] = 7; }
                else if (ComboBox.SelectedIndex == 3) { AppVariables.ApplicationSettings["RemoveItemsRange"] = 14; }
                else if (ComboBox.SelectedIndex == 4) { AppVariables.ApplicationSettings["RemoveItemsRange"] = 28; }
                else if (ComboBox.SelectedIndex == 5) { AppVariables.ApplicationSettings["RemoveItemsRange"] = 56; }
                else if (ComboBox.SelectedIndex == 6) { AppVariables.ApplicationSettings["RemoveItemsRange"] = 84; }

                //Reset the online status
                OnlineUpdateFeeds = true;
                OnlineUpdateNews = true;
                OnlineUpdateStarred = true;
                ApiMessageError = String.Empty;

                //Reset the last update setting
                AppVariables.ApplicationSettings["LastItemsUpdate"] = "Never";
            };

            ////Maximum Update Items
            //setting_MaxUpdateItems.SelectionChanged += (sender, e) =>
            //{
            //    ComboBox ComboBox = (ComboBox)sender;
            //    if (ComboBox.SelectedIndex == 0) { AppVariables.ApplicationSettings["MaxUpdateItems"] = 250; }
            //    else if (ComboBox.SelectedIndex == 1) { AppVariables.ApplicationSettings["MaxUpdateItems"] = 500; }
            //    else if (ComboBox.SelectedIndex == 2) { AppVariables.ApplicationSettings["MaxUpdateItems"] = 1000; }
            //    else if (ComboBox.SelectedIndex == 3) { AppVariables.ApplicationSettings["MaxUpdateItems"] = 1500; }
            //    else if (ComboBox.SelectedIndex == 4) { AppVariables.ApplicationSettings["MaxUpdateItems"] = 2000; }
            //};
        }
    }
}