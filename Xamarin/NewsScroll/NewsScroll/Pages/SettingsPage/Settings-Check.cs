using System.Diagnostics;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;

namespace NewsScroll
{
    public partial class SettingsPage
    {
        public static async Task SettingsCheck()
        {
            try
            {
                Debug.WriteLine("Checking application settings...");

                //Account
                if (!AppSettingCheck("ApiAccount"))
                {
                    await AppSettingSave("ApiAccount", string.Empty);
                }

                //Password
                if (!AppSettingCheck("ApiPassword"))
                {
                    await AppSettingSave("ApiPassword", string.Empty);
                }

                //Display items author
                if (!AppSettingCheck("DisplayItemsAuthor"))
                {
                    await AppSettingSave("DisplayItemsAuthor", true);
                }

                //Display images offline
                if (!AppSettingCheck("DisplayImagesOffline"))
                {
                    await AppSettingSave("DisplayImagesOffline", false);
                }

                //Display Read Marked items
                if (!AppSettingCheck("DisplayReadMarkedItems"))
                {
                    await AppSettingSave("DisplayReadMarkedItems", false);
                }

                //Hide Read Marked item
                if (!AppSettingCheck("HideReadMarkedItem"))
                {
                    await AppSettingSave("HideReadMarkedItem", false);
                }

                //News item content cutting
                if (!AppSettingCheck("ContentCutting"))
                {
                    await AppSettingSave("ContentCutting", true);
                }

                //News item content cutting length
                if (!AppSettingCheck("ContentCuttingLength"))
                {
                    await AppSettingSave("ContentCuttingLength", 200);
                }

                //Enable item text selection
                if (!AppSettingCheck("ItemTextSelection"))
                {
                    await AppSettingSave("ItemTextSelection", false);
                }

                //Low bandwidth mode
                if (!AppSettingCheck("LowBandwidthMode"))
                {
                    await AppSettingSave("LowBandwidthMode", false);
                }

                //Disable Swipe Action
                if (!AppSettingCheck("DisableSwipeActions"))
                {
                    await AppSettingSave("DisableSwipeActions", false);
                }

                //Swipe direction
                if (!AppSettingCheck("SwipeDirection"))
                {
                    await AppSettingSave("SwipeDirection", "0");
                }

                //Default item open method
                if (!AppSettingCheck("ItemOpenMethod"))
                {
                    await AppSettingSave("ItemOpenMethod", "0");
                }

                //Item list view style
                if (!AppSettingCheck("ListViewStyle"))
                {
                    await AppSettingSave("ListViewStyle", "0");
                }

                //Remove Items Range
                if (!AppSettingCheck("RemoveItemsRange"))
                {
                    await AppSettingSave("RemoveItemsRange", "4");
                }

                //Color Theme
                if (!AppSettingCheck("ColorTheme"))
                {
                    await AppSettingSave("ColorTheme", "2");
                }

                //Item Scroll Direction
                if (!AppSettingCheck("ItemScrollDirection"))
                {
                    await AppSettingSave("ItemScrollDirection", "0");
                }

                //Adjust font size
                if (!AppSettingCheck("AdjustFontSize"))
                {
                    await AppSettingSave("AdjustFontSize", 0);
                }

                //Last Api Login Auth
                if (!AppSettingCheck("ConnectApiAuth"))
                {
                    await AppSettingSave("ConnectApiAuth", string.Empty);
                }

                //Last items update time
                if (!AppSettingCheck("LastItemsUpdate"))
                {
                    await AppSettingSave("LastItemsUpdate", "Never");
                }
            }
            catch { }
        }
    }
}