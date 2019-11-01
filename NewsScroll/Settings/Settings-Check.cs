using ArnoldVinkCode;
using System;
using System.Diagnostics;
using Windows.Graphics.Display;
using Windows.System;

namespace NewsScroll
{
    public partial class SettingsPage
    {
        public static void SettingsCheck()
        {
            try
            {
                Debug.WriteLine("Checking application settings...");

                //Check allowed memory usage
                UInt64 AllowedMemory = (MemoryManager.AppMemoryUsageLimit / 1024 / 1024);
                Debug.WriteLine("Current memory limit: " + AllowedMemory);

                //Account
                if (!AppVariables.ApplicationSettings.ContainsKey("ApiAccount"))
                {
                    AppVariables.ApplicationSettings["ApiAccount"] = String.Empty;
                }

                //Password
                if (!AppVariables.ApplicationSettings.ContainsKey("ApiPassword"))
                {
                    AppVariables.ApplicationSettings["ApiPassword"] = String.Empty;
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

                //Load all available items
                if (!AppVariables.ApplicationSettings.ContainsKey("LoadAllItems"))
                {
                    if (AllowedMemory <= 420) { AppVariables.ApplicationSettings["LoadAllItems"] = false; }
                    else { AppVariables.ApplicationSettings["LoadAllItems"] = true; }
                }

                //Enable item text selection
                if (!AppVariables.ApplicationSettings.ContainsKey("ItemTextSelection"))
                {
                    AppVariables.ApplicationSettings["ItemTextSelection"] = false;
                }

                //Disable Landscape Display
                if (!AppVariables.ApplicationSettings.ContainsKey("DisableLandscapeDisplay"))
                {
                    AppVariables.ApplicationSettings["DisableLandscapeDisplay"] = false;
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape;
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
                    AppVariables.ApplicationSettings["AdjustFontSize"] = -1;
                }

                //Last Api Login Auth
                if (!AppVariables.ApplicationSettings.ContainsKey("ConnectApiAuth"))
                {
                    AppVariables.ApplicationSettings["ConnectApiAuth"] = String.Empty;
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