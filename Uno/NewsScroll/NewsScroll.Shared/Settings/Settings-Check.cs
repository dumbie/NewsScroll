using ArnoldVinkCode;

namespace NewsScroll
{
    public partial class SettingsPage
    {
        public static void SettingsCheck()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Checking application settings...");

                //Check allowed memory usage
                uint AllowedMemory = 999999999; //Fix(MemoryManager.AppMemoryUsageLimit / 1024 / 1024);
                System.Diagnostics.Debug.WriteLine("Current memory limit: " + AllowedMemory);

                //Account
                if (!AppVariables.ApplicationSettings.ContainsKey("ApiAccount"))
                {
                    AppVariables.ApplicationSettings["ApiAccount"] = string.Empty;
                }

                //Password
                if (!AppVariables.ApplicationSettings.ContainsKey("ApiPassword"))
                {
                    AppVariables.ApplicationSettings["ApiPassword"] = string.Empty;
                }

                //Update Items on Startup
                if (!AppVariables.ApplicationSettings.ContainsKey("UpdateItemsStartup"))
                {
                    AppVariables.ApplicationSettings["UpdateItemsStartup"] = true;
                }

                //Display items author
                if (!AppVariables.ApplicationSettings.ContainsKey("DisplayItemsAuthor"))
                {
                    AppVariables.ApplicationSettings["DisplayItemsAuthor"] = true;
                }

                //Display images offline
                if (!AppVariables.ApplicationSettings.ContainsKey("DisplayImagesOffline"))
                {
                    AppVariables.ApplicationSettings["DisplayImagesOffline"] = false;
                }

                //Display Read Marked items
                if (!AppVariables.ApplicationSettings.ContainsKey("DisplayReadMarkedItems"))
                {
                    AppVariables.ApplicationSettings["DisplayReadMarkedItems"] = false;
                }

                //Hide Read Marked item
                if (!AppVariables.ApplicationSettings.ContainsKey("HideReadMarkedItem"))
                {
                    AppVariables.ApplicationSettings["HideReadMarkedItem"] = false;
                }

                //News item content cutting
                if (!AppVariables.ApplicationSettings.ContainsKey("ContentCutting"))
                {
                    AppVariables.ApplicationSettings["ContentCutting"] = true;
                }

                //News item content cutting length
                if (!AppVariables.ApplicationSettings.ContainsKey("ContentCuttingLength"))
                {
                    AppVariables.ApplicationSettings["ContentCuttingLength"] = 200;
                }

                //Disable Landscape Display
                if (!AppVariables.ApplicationSettings.ContainsKey("DisableLandscapeDisplay"))
                {
                    AppVariables.ApplicationSettings["DisableLandscapeDisplay"] = false;
                }

                //Low bandwidth mode
                if (!AppVariables.ApplicationSettings.ContainsKey("LowBandwidthMode"))
                {
                    AppVariables.ApplicationSettings["LowBandwidthMode"] = false;
                }

                //Disable Swipe Action
                if (!AppVariables.ApplicationSettings.ContainsKey("DisableSwipeActions"))
                {
                    AppVariables.ApplicationSettings["DisableSwipeActions"] = false;
                }

                //Swipe direction
                if (!AppVariables.ApplicationSettings.ContainsKey("SwipeDirection"))
                {
                    AppVariables.ApplicationSettings["SwipeDirection"] = "0";
                }

                //Default item open method
                if (!AppVariables.ApplicationSettings.ContainsKey("ItemOpenMethod"))
                {
                    AppVariables.ApplicationSettings["ItemOpenMethod"] = "0";
                    //if (AllowedMemory <= 420) { AppVariables.ApplicationSettings["ItemOpenMethod"] = "0"; }
                    //else { AppVariables.ApplicationSettings["ItemOpenMethod"] = "1"; }
                }

                //Item list view style
                if (!AppVariables.ApplicationSettings.ContainsKey("ListViewStyle"))
                {
                    AppVariables.ApplicationSettings["ListViewStyle"] = "0";
                }

                //Remove Items Range
                if (!AppVariables.ApplicationSettings.ContainsKey("RemoveItemsRange"))
                {
                    AppVariables.ApplicationSettings["RemoveItemsRange"] = "4";
                }

                //Color Theme
                if (!AppVariables.ApplicationSettings.ContainsKey("ColorTheme"))
                {
                    AppVariables.ApplicationSettings["ColorTheme"] = "2";
                }

                //Item Scroll Direction
                if (!AppVariables.ApplicationSettings.ContainsKey("ItemScrollDirection"))
                {
                    if (AVFunctions.DevMobile()) { AppVariables.ApplicationSettings["ItemScrollDirection"] = "0"; }
                    else { AppVariables.ApplicationSettings["ItemScrollDirection"] = "2"; }
                }

                ////Maximum Update Items
                //if (!AppVariables.ApplicationSettings.ContainsKey("MaxUpdateItems"))
                //{
                //    AppVariables.ApplicationSettings["MaxUpdateItems"] = "1000";
                //}

                //Adjust font size
                if (!AppVariables.ApplicationSettings.ContainsKey("AdjustFontSize"))
                {
                    AppVariables.ApplicationSettings["AdjustFontSize"] = 0;
                }

                //Last Api Login Auth
                if (!AppVariables.ApplicationSettings.ContainsKey("ConnectApiAuth"))
                {
                    AppVariables.ApplicationSettings["ConnectApiAuth"] = string.Empty;
                }

                //Last items update time
                if (!AppVariables.ApplicationSettings.ContainsKey("LastItemsUpdate"))
                {
                    AppVariables.ApplicationSettings["LastItemsUpdate"] = "Never";
                }
            }
            catch { }
        }
    }
}