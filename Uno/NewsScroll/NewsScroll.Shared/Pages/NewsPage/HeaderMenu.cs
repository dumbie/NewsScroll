﻿using ArnoldVinkCode;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NewsScroll
{
    public partial class NewsPage
    {
        async void iconMenu_Tapped(object sender, RoutedEventArgs e)
        {
            try
            {
                AppVariables.SingleTappedEvent = true;
                await Task.Delay(300);
                if (AppVariables.SingleTappedEvent)
                {
                    await HideShowMenu(false);
                }
            }
            catch { }
        }

        async void iconMenu_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                AppVariables.SingleTappedEvent = false;
                await HideShowHeader(false);
            }
            catch { }
        }

        private async Task HideShowHeader(bool ForceClose)
        {
            try
            {
                if (ForceClose || stackpanel_Header.Visibility == Visibility.Visible)
                {
                    stackpanel_Header.Visibility = Visibility.Collapsed;
                    await HideShowMenu(true);

                    if (!AVFunctions.DevMobile()) { iconMenu.Margin = new Thickness(0, 0, 16, 0); }

                    image_iconMenu.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconMenu-Dark.png", false);
                    image_iconMenu.Opacity = 0.60;

                    grid_StatusApplication.Margin = new Thickness(0, 0, 0, 0);
                    grid_StatusApplication.Background = new SolidColorBrush(Color.FromArgb(255, 88, 88, 88)) { Opacity = 0.60 };
                    border_StatusCurrentItem.Background = new SolidColorBrush(Color.FromArgb(255, 88, 88, 88)) { Opacity = 0.50 };

                    //Update the current item status text
                    if (AppVariables.CurrentTotalItemsCount == 0)
                    {
                        textblock_StatusCurrentItem.Text = AppVariables.CurrentShownItemCount.ToString();
                    }
                    else
                    {
                        textblock_StatusCurrentItem.Text = AppVariables.CurrentShownItemCount + "/" + AppVariables.CurrentTotalItemsCount;
                    }

                    AppVariables.HeaderHidden = true;
                }
                else
                {
                    stackpanel_Header.Visibility = Visibility.Visible;

                    iconMenu.Margin = new Thickness(0, 0, 0, 0);

                    image_iconMenu.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconMenu.png", false);
                    image_iconMenu.Opacity = 1;

                    grid_StatusApplication.Margin = new Thickness(0, 65, 0, 0);
                    grid_StatusApplication.Background = new SolidColorBrush((Color)Application.Current.Resources["ApplicationAccentLightColor"]) { Opacity = 0.60 };
                    border_StatusCurrentItem.Background = new SolidColorBrush((Color)Application.Current.Resources["ApplicationAccentLightColor"]) { Opacity = 0.50 };

                    //Update the current item status text
                    textblock_StatusCurrentItem.Text = AppVariables.CurrentShownItemCount.ToString();

                    AppVariables.HeaderHidden = false;
                }
            }
            catch { }
        }

        private async Task HideShowMenu(bool ForceClose)
        {
            try
            {
                if (ForceClose || grid_PopupMenu.Visibility == Visibility.Visible)
                {
                    grid_PopupMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grid_PopupMenu.Visibility = Visibility.Visible;
                    if (stackpanel_Header.Visibility != Visibility.Visible)
                    {
                        await HideShowHeader(false);
                    }
                }
            }
            catch { }
        }
    }
}