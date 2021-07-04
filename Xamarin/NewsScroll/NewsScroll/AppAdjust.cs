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
                    //Application.Current.Resources["ApplicationLightColor"] = Color.Red;
                }
                else if (SelectedTheme == 1)
                {
                    //Application.Current.Resources["ApplicationLightColor"] = Color.Blue;
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

                Application.Current.Resources.TryGetValue("TextSizeSmall", out object TextSizeSmallGet);
                double SmallSize = (double)TextSizeSmallGet + FontSize;
                Application.Current.Resources["TextSizeSmall"] = SmallSize;

                Application.Current.Resources.TryGetValue("TextSizeMedium", out object TextSizeMediumGet);
                double MediumSize = (double)TextSizeMediumGet + FontSize;
                Application.Current.Resources["TextSizeMedium"] = MediumSize;

                Application.Current.Resources.TryGetValue("TextSizeLarge", out object TextSizeLargeGet);
                double LargeSize = (double)TextSizeLargeGet + FontSize;
                Application.Current.Resources["TextSizeLarge"] = LargeSize;

                Application.Current.Resources.TryGetValue("TextSizeHuge", out object TextSizeHugeGet);
                double HugeSize = (double)TextSizeHugeGet + FontSize;
                Application.Current.Resources["TextSizeHuge"] = HugeSize;
            }
            catch { }
        }
    }
}