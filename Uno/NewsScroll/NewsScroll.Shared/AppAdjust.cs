﻿using NewsScroll.Styles;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

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

                if (SelectedTheme == 0)
                {
                    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationThemeForeground = (SolidColorBrush)Application.Current.Resources["ApplicationBlackBrush"];
                    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationThemeBackground = (SolidColorBrush)Application.Current.Resources["ApplicationWhiteBrush"];
                }
                else if (SelectedTheme == 1)
                {
                    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationThemeForeground = (SolidColorBrush)Application.Current.Resources["ApplicationWhiteBrush"];
                    ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationThemeBackground = (SolidColorBrush)Application.Current.Resources["ApplicationBlackBrush"];
                }
                else
                {
                    if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                    {
                        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationThemeForeground = (SolidColorBrush)Application.Current.Resources["ApplicationWhiteBrush"];
                        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationThemeBackground = (SolidColorBrush)Application.Current.Resources["ApplicationBlackBrush"];
                    }
                    else
                    {
                        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationThemeForeground = (SolidColorBrush)Application.Current.Resources["ApplicationBlackBrush"];
                        ((DynamicStyle)Application.Current.Resources["DynamicStyle"]).ApplicationThemeBackground = (SolidColorBrush)Application.Current.Resources["ApplicationWhiteBrush"];
                    }
                }

                System.Diagnostics.Debug.WriteLine("Adjusted the application theme.");
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