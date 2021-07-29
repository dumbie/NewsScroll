using NewsScroll.Styles;
using System;
using Windows.UI.Xaml;

namespace NewsScroll
{
    class AppAdjust
    {
        //Adjust the Color Theme
        public static void AdjustColorTheme()
        {
            try
            {
                int SelectedTheme = Convert.ToInt32(AppVariables.ApplicationSettings["ColorTheme"]);
                System.Diagnostics.Debug.WriteLine("Adjusting the application theme to: " + SelectedTheme);

                //Fix
                //if (SelectedTheme == 0)
                //{
                //    SolidColorBrush ColorEnabled = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                //    SolidColorBrush ColorDisabled = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                //    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationLightGrayBrush = ColorEnabled;
                //    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationDarkGrayBrush = ColorDisabled;
                //    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ElementRequestedTheme = ElementTheme.Light;
                //}
                //else if (SelectedTheme == 1)
                //{
                //    SolidColorBrush ColorEnabled = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                //    SolidColorBrush ColorDisabled = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                //    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationLightGrayBrush = ColorEnabled;
                //    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationDarkGrayBrush = ColorDisabled;
                //    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ElementRequestedTheme = ElementTheme.Dark;
                //}
                //else
                //{
                //    if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                //    {
                //        SolidColorBrush ColorEnabled = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                //        SolidColorBrush ColorDisabled = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                //        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationLightGrayBrush = ColorEnabled;
                //        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationDarkGrayBrush = ColorDisabled;
                //        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ElementRequestedTheme = ElementTheme.Dark;
                //    }
                //    else
                //    {
                //        SolidColorBrush ColorEnabled = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                //        SolidColorBrush ColorDisabled = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                //        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationLightGrayBrush = ColorEnabled;
                //        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationDarkGrayBrush = ColorDisabled;
                //        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ElementRequestedTheme = ElementTheme.Light;
                //    }
                //}
            }
            catch { }
        }

        //Adjust the Font Sizes
        public static void AdjustFontSizes()
        {
            try
            {
                int FontSize = Convert.ToInt32(AppVariables.ApplicationSettings["AdjustFontSize"]);
                System.Diagnostics.Debug.WriteLine("Adjusting the font size to: " + FontSize);

                double SmallSize = (double)Application.Current.Resources["TextSizeSmall"] + FontSize;
                ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).TextSizeSmall = SmallSize;

                double MediumSize = (double)Application.Current.Resources["TextSizeMedium"] + FontSize;
                ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).TextSizeMedium = MediumSize;

                double LargeSize = (double)Application.Current.Resources["TextSizeLarge"] + FontSize;
                ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).TextSizeLarge = LargeSize;

                double HugeSize = (double)Application.Current.Resources["TextSizeHuge"] + FontSize;
                ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).TextSizeHuge = HugeSize;
            }
            catch { }
        }
    }
}