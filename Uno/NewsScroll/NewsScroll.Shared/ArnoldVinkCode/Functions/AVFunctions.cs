using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ArnoldVinkCode
{
    class AVFunctions
    {
        //Convert String To Title Case
        public static string ToTitleCase(string ToTitleCase)
        {
            char[] TitleCase = ToTitleCase.ToLower().ToCharArray();
            for (int i = 0; i < TitleCase.Count(); i++) { TitleCase[i] = i == 0 || TitleCase[i - 1] == ' ' ? char.ToUpper(TitleCase[i]) : TitleCase[i]; }
            return new string(TitleCase);
        }

        //Replace first text occurence in string
        public static string StringReplaceFirst(string StringText, string SearchFor, string ReplaceWith, bool StartsWith)
        {
            if (StartsWith) { if (!StringText.ToLower().StartsWith(SearchFor.ToLower())) { return StringText; } }
            int Position = StringText.IndexOf(SearchFor, StringComparison.CurrentCultureIgnoreCase);
            if (Position < 0) { return StringText; }
            return StringText.Substring(0, Position) + ReplaceWith + StringText.Substring(Position + SearchFor.Length);
        }

        //Remove starting text occurence in string
        public static string StringRemoveStart(string String, string toRemove)
        {
            try
            {
                while (String.StartsWith(toRemove))
                {
                    String = String.Substring(toRemove.Length);
                }
            }
            catch { }
            return String;
        }

        //Remove multiple starting text occurence in string
        public static string StringRemoveMultiStart(string String, string[] toRemove)
        {
            try
            {
                while (toRemove.Any(String.StartsWith))
                {
                    foreach (string Remove in toRemove)
                    {
                        if (String.StartsWith(Remove)) { String = String.Substring(Remove.Length); }
                    }
                }
            }
            catch { }
            return String;
        }

        //Remove ending text occurence in string
        public static string StringRemoveEnd(string String, string toRemove)
        {
            try
            {
                while (String.EndsWith(toRemove))
                {
                    String = String.Substring(0, String.Length - toRemove.Length);
                }
            }
            catch { }
            return String;
        }

        //Remove multiple ending text occurence in string
        public static string StringRemoveMultiEnd(string String, string[] toRemove)
        {
            try
            {
                while (toRemove.Any(String.EndsWith))
                {
                    foreach (string Remove in toRemove)
                    {
                        if (String.EndsWith(Remove)) { String = String.Substring(0, String.Length - Remove.Length); }
                    }
                }
            }
            catch { }
            return String;
        }

        //Replace last text occurence in string
        public static string StringReplaceLast(string String, string ReplaceWith)
        {
            try
            {
                return String.Remove(String.Length - 1, 1) + ReplaceWith;
            }
            catch { return String; }
        }

        //Add string to string with character
        public static string StringAdd(string OldString, string AddString, string Character)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(OldString)) { OldString = OldString + Character + " " + AddString; }
                else { OldString = AddString; }
                return OldString;
            }
            catch { return OldString; }
        }

        //Remove text after certain character
        public static string StringRemoveAfter(string String, string RemoveCharacter, int RemoveAfter)
        {
            try
            {
                String = String.Substring(0, String.IndexOf(RemoveCharacter) + RemoveAfter);
            }
            catch { }
            return String;
        }

        //Convert String To Cutted String
        public static string StringCut(string CutString, int CutAt, string AddString)
        {
            if (CutString.Length > CutAt) { return CutString.Substring(0, CutAt) + AddString; }
            else { return CutString; }
        }

        //Convert Number To Text
        public static string NumberToText(string StrNumber)
        {
            try
            {
                string[] ones = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
                string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                string[] tens = { "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                int IntNumber = Convert.ToInt32(StrNumber);
                if (IntNumber < 10) { StrNumber = ones[IntNumber]; }
                else if (IntNumber < 20) { StrNumber = teens[IntNumber - 10]; }
                else if (IntNumber < 100)
                {
                    if (IntNumber % 10 != 0) { StrNumber = tens[IntNumber / 10 - 2] + "-" + ones[IntNumber % 10]; }
                    else { StrNumber = tens[IntNumber / 10 - 2]; }
                }

                return StrNumber;
            }
            catch { return "Unknown"; }
        }

        //Check if device is mobile
        public static bool DevMobile()
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("Device family: " + AnalyticsInfo.VersionInfo.DeviceFamily);
                if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile") { return true; }
            }
            catch { }
            return false;
        }

        //Check device os version
        public static int DevOsVersion()
        {
            try
            {
                long OsVersion = long.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
                long BuildNumber = (OsVersion & 0x00000000FFFF0000L) >> 16;
                return Convert.ToInt32(BuildNumber);
            }
            catch { return 999999999; }
        }

        //Get device free memory in MB
        public static ulong DevMemoryAvailableMB()
        {
            try
            {
                ulong memoryUsage = MemoryManager.AppMemoryUsage;
                ulong memoryLimit = MemoryManager.AppMemoryUsageLimit;
                return (memoryLimit - memoryUsage) / 1024 / 1024;
            }
            catch { return 0; }
        }

        //Get device memory usage in percentage
        public static ulong DevMemoryUsePercentage()
        {
            try
            {
                ulong memoryUsage = MemoryManager.AppMemoryUsage;
                ulong memoryLimit = MemoryManager.AppMemoryUsageLimit;
                return (memoryUsage * 100) / memoryLimit;
            }
            catch { return 0; }
        }

        //Get the device screen resolution
        public static Size DevScreenResolution()
        {
            try
            {
                DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
                return new Size(displayInformation.ScreenWidthInRawPixels, displayInformation.ScreenHeightInRawPixels);
            }
            catch { return new Size(0, 0); }
        }

        //Get the window resolution
        public static Rect AppWindowResolution()
        {
            try
            {
                if (DevMobile()) { return Window.Current.Bounds; }
                else { return ApplicationView.GetForCurrentView().VisibleBounds; }
            }
            catch { return new Rect(new Point(0, 0), new Point(0, 0)); }
        }

        //Get network connection type
        public static string GetNetworkType()
        {
            try
            {
                ConnectionProfile ConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (ConnectionProfile == null) { return "NoConnection"; }
                else if (ConnectionProfile.IsWlanConnectionProfile) { return "Wireless"; }
                else if (ConnectionProfile.IsWwanConnectionProfile) { return "Mobile"; }
                else { return "Wired"; }
            }
            catch { return "NoConnection"; }
        }

        //Check if datetime is between dates
        public static bool TimeBetween(DateTime NowTime, DateTime StartTime, DateTime EndTime, bool Inclusive)
        {
            if (Inclusive) { return (NowTime >= StartTime) && (NowTime <= EndTime); }
            else { return (NowTime > StartTime) && (NowTime < EndTime); }
        }

        //Reset a timer tick estimate
        public static void ResetTimer(DispatcherTimer ResetTimer)
        {
            try
            {
                ResetTimer.Stop();
                ResetTimer.Start();
            }
            catch { }
        }

        //Get the current application frame
        //Usage: if (GetCurrentAppFrame is FrameHere)
        public static object GetCurrentAppFrame()
        {
            try
            {
                Frame currentFrame = (Frame)Window.Current.Content;
                return currentFrame.Content;
            }
            catch { return null; }
        }

        //Check if element is visible
        public static bool ElementIsVisible(FrameworkElement element, FrameworkElement container)
        {
            try
            {
                if (element == null || container == null)
                {
                    return false;
                }

                Rect containerBounds = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
                Rect elementBounds = element.TransformToVisual(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
                return (elementBounds.Left < containerBounds.Right && elementBounds.Right > containerBounds.Left);
            }
            catch
            {
                return false;
            }
        }

        //Scroll to certain ui element
        public static async Task ScrollViewToElement(ScrollViewer ScrollViewer, FrameworkElement element, bool VertScrolling, bool InstantScroll)
        {
            try
            {
                GeneralTransform GenTransform = element.TransformToVisual((UIElement)ScrollViewer.Content);
                Point TransPoint = GenTransform.TransformPoint(new Point(0, 0));

                await Task.Delay(10);
                if (VertScrolling) { ScrollViewer.ChangeView(null, TransPoint.Y, null, InstantScroll); }
                else { ScrollViewer.ChangeView(TransPoint.X, null, null, InstantScroll); }
                ScrollViewer.UpdateLayout();
            }
            catch { }
        }

        //Find and return a visual child
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            try
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child is T) { return (T)child; }
                    else
                    {
                        child = FindVisualChild<T>(child);
                        if (child != null) { return (T)child; }
                    }
                }
            }
            catch { }
            return default(T);
        }

        //Check if a file exists in app
        public static async Task<bool> AppFileExists(string FileName)
        {
            FileName = FileName.Replace("ms-appx:///", string.Empty);
            try
            {
                await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///" + FileName));
                return true;
            }
            catch (FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("Could not find a requested app file: " + FileName);
                return false;
            }
        }

        //Check if a file exists in local
        public static async Task<bool> LocalFileExists(string FileName)
        {
            FileName = FileName.Replace("ms-appdata:///local/", string.Empty);
            try { return await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName) != null; }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Could not find a requested local file: " + FileName);
                return false;
            }
        }

        //Convert Degrees to Cardinal string
        public static string DegreesToCardinal(double degrees)
        {
            try
            {
                string[] CardinalList = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
                return CardinalList[(int)Math.Round((degrees % 360) / 45)];
            }
            catch { return "N/A"; }
        }
    }
}