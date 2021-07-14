using System;
using System.Diagnostics;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;

namespace NewsScroll
{
    class AppAdjust
    {
        //Adjust the Color Theme
        public static void AdjustColorTheme()
        {
            try
            {
                int SelectedTheme = Convert.ToInt32(AppSettingLoad("ColorTheme"));
                Debug.WriteLine("Adjusting the application theme to: " + SelectedTheme);

                if (SelectedTheme == 0)
                {
                    //Light mode
                    Application.Current.UserAppTheme = OSAppTheme.Light;
                    Application.Current.Resources["ApplicationLightColor"] = Color.FromHex("F1F1F1");
                    Application.Current.Resources["ApplicationDarkColor"] = Color.FromHex("000000");
                    Application.Current.Resources["ApplicationLightGrayColor"] = Color.FromHex("DBDBDB");
                    Application.Current.Resources["ApplicationDarkGrayColor"] = Color.FromHex("505050");
                }
                else if (SelectedTheme == 1)
                {
                    //Dark mode
                    Application.Current.UserAppTheme = OSAppTheme.Dark;
                    Application.Current.Resources["ApplicationLightColor"] = Color.FromHex("000000");
                    Application.Current.Resources["ApplicationDarkColor"] = Color.FromHex("F1F1F1");
                    Application.Current.Resources["ApplicationLightGrayColor"] = Color.FromHex("505050");
                    Application.Current.Resources["ApplicationDarkGrayColor"] = Color.FromHex("DBDBDB");
                }
            }
            catch { }
        }

        //Adjust the Font Sizes
        public static void AdjustFontSizes()
        {
            try
            {
                int FontSize = Convert.ToInt32(AppSettingLoad("AdjustFontSize"));
                Debug.WriteLine("Adjusting the font size to: " + FontSize);

                Application.Current.Resources.TryGetValue("TextSizeSmallDefault", out object TextSizeSmallGet);
                double SmallSize = (double)TextSizeSmallGet + FontSize;
                Application.Current.Resources["TextSizeSmall"] = SmallSize;

                Application.Current.Resources.TryGetValue("TextSizeMediumDefault", out object TextSizeMediumGet);
                double MediumSize = (double)TextSizeMediumGet + FontSize;
                Application.Current.Resources["TextSizeMedium"] = MediumSize;

                Application.Current.Resources.TryGetValue("TextSizeLargeDefault", out object TextSizeLargeGet);
                double LargeSize = (double)TextSizeLargeGet + FontSize;
                Application.Current.Resources["TextSizeLarge"] = LargeSize;

                Application.Current.Resources.TryGetValue("TextSizeHugeDefault", out object TextSizeHugeGet);
                double HugeSize = (double)TextSizeHugeGet + FontSize;
                Application.Current.Resources["TextSizeHuge"] = HugeSize;
            }
            catch { }
        }
    }
}