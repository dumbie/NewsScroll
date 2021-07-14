using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;

namespace NewsScroll
{
    public partial class SettingsPage
    {
        private void SettingsSave()
        {
            try
            {
                //Api account
                setting_ApiAccount.TextChanged += async (sender, e) =>
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateNews = true;
                    OnlineUpdateStarred = true;
                    ApiMessageError = string.Empty;

                    //Save settings
                    await AppSettingSave("ConnectApiAuth", string.Empty);
                    await AppSettingSave("ApiAccount", setting_ApiAccount.Text);
                };

                //Api password
                setting_ApiPassword.TextChanged += async (sender, e) =>
                {
                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateNews = true;
                    OnlineUpdateStarred = true;
                    ApiMessageError = string.Empty;

                    //Save settings
                    await AppSettingSave("ConnectApiAuth", string.Empty);
                    await AppSettingSave("ApiPassword", setting_ApiPassword.Text);
                };

                //Display items author
                setting_DisplayItemsAuthor.CheckedChanged += async (sender, e) =>
                {
                    CheckBox CheckBox = (CheckBox)sender;
                    await AppSettingSave("DisplayItemsAuthor", CheckBox.IsChecked);
                };

                //Display images offline
                setting_DisplayImagesOffline.CheckedChanged += async (sender, e) =>
                {
                    CheckBox CheckBox = (CheckBox)sender;
                    await AppSettingSave("DisplayImagesOffline", CheckBox.IsChecked);
                };

                //Display Read Marked items
                setting_DisplayReadMarkedItems.CheckedChanged += async (sender, e) =>
                {
                    CheckBox CheckBox = (CheckBox)sender;
                    await AppSettingSave("DisplayReadMarkedItems", CheckBox.IsChecked);
                };

                //News item content cutting
                setting_ContentCutting.CheckedChanged += async (sender, e) =>
                {
                    CheckBox CheckBox = (CheckBox)sender;
                    await AppSettingSave("ContentCutting", CheckBox.IsChecked);
                };

                //News item content cutting length
                setting_ContentCuttingLength.TextChanged += async (sender, e) =>
                {
                    //Get color from resource
                    Application.Current.Resources.TryGetValue("InvalidColor", out object ColorInvalid);
                    Application.Current.Resources.TryGetValue("ValidColor", out object ColorValid);

                    //Check for numbers
                    if (!Regex.IsMatch(setting_ContentCuttingLength.Text, "^[0-9]+$"))
                    {
                        setting_ContentCuttingLength.TextColor = (Color)ColorInvalid;
                        return;
                    }

                    //Check content length
                    int NewSetting = Convert.ToInt32(setting_ContentCuttingLength.Text);
                    if (NewSetting < 10)
                    {
                        setting_ContentCuttingLength.TextColor = (Color)ColorInvalid;
                        return;
                    }
                    else if (NewSetting > AppVariables.MaximumItemTextLength)
                    {
                        setting_ContentCuttingLength.TextColor = (Color)ColorInvalid;
                        return;
                    }

                    await AppSettingSave("ContentCuttingLength", NewSetting);
                    setting_ContentCuttingLength.TextColor = (Color)ColorValid;
                };

                //Enable item text selection
                setting_ItemTextSelection.CheckedChanged += async (sender, e) =>
                {
                    CheckBox CheckBox = (CheckBox)sender;
                    await AppSettingSave("ItemTextSelection", CheckBox.IsChecked);
                };

                //Low bandwidth mode
                setting_LowBandwidthMode.CheckedChanged += async (sender, e) =>
                {
                    CheckBox CheckBox = (CheckBox)sender;
                    await AppSettingSave("LowBandwidthMode", CheckBox.IsChecked);
                };

                //Disable Swipe Action
                setting_DisableSwipeActions.CheckedChanged += async (sender, e) =>
                {
                    CheckBox CheckBox = (CheckBox)sender;
                    await AppSettingSave("DisableSwipeActions", CheckBox.IsChecked);
                };

                //Swipe direction
                setting_SwipeDirection.SelectedIndexChanged += async (sender, e) =>
                {
                    Picker Picker = (Picker)sender;
                    await AppSettingSave("SwipeDirection", Picker.SelectedIndex);
                };

                //Default item open method
                setting_ItemOpenMethod.SelectedIndexChanged += async (sender, e) =>
                {
                    Picker Picker = (Picker)sender;
                    await AppSettingSave("ItemOpenMethod", Picker.SelectedIndex);
                };

                //Remove Items Range
                setting_RemoveItemsRange.SelectedIndexChanged += async (sender, e) =>
                {
                    Picker Picker = (Picker)sender;
                    if (Picker.SelectedIndex == 0) { await AppSettingSave("RemoveItemsRange", 2); }
                    else if (Picker.SelectedIndex == 1) { await AppSettingSave("RemoveItemsRange", 4); }
                    else if (Picker.SelectedIndex == 2) { await AppSettingSave("RemoveItemsRange", 7); }
                    else if (Picker.SelectedIndex == 3) { await AppSettingSave("RemoveItemsRange", 14); }
                    else if (Picker.SelectedIndex == 4) { await AppSettingSave("RemoveItemsRange", 28); }
                    else if (Picker.SelectedIndex == 5) { await AppSettingSave("RemoveItemsRange", 56); }
                    else if (Picker.SelectedIndex == 6) { await AppSettingSave("RemoveItemsRange", 84); }

                    //Reset the online status
                    OnlineUpdateFeeds = true;
                    OnlineUpdateNews = true;
                    OnlineUpdateStarred = true;
                    ApiMessageError = string.Empty;

                    //Reset the last update setting
                    await AppSettingSave("LastItemsUpdate", "Never");
                };
            }
            catch { }
        }
    }
}