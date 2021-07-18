using ArnoldVinkCode;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NewsScroll
{
    public partial class StarredPage
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
                int HeaderTargetSize = Convert.ToInt32(stackpanel_Header.Tag);
                int HeaderCurrentSize = Convert.ToInt32(stackpanel_Header.Height);
                if (ForceClose || HeaderCurrentSize == HeaderTargetSize)
                {
                    stackpanel_Header.Height = 0;
                    await HideShowMenu(true);

                    if (!AVFunctions.DevMobile()) { iconMenu.Margin = new Thickness(0, 0, 16, 0); }

                    image_iconMenu.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconMenu-Dark.png", false);
                    image_iconMenu.Opacity = 0.60;

                    grid_StatusApplication.Margin = new Thickness(0, 0, 0, 0);
                    grid_StatusApplication.Background = new SolidColorBrush(Color.FromArgb(255, 88, 88, 88)) { Opacity = 0.60 };
                    border_StatusCurrentItem.Background = new SolidColorBrush(Color.FromArgb(255, 88, 88, 88)) { Opacity = 0.50 };

                    //Update the current item status text
                    if (AppVariables.CurrentTotalItemsCount == 0) { textblock_StatusCurrentItem.Text = textblock_StatusCurrentItem.Tag.ToString(); }
                    else { textblock_StatusCurrentItem.Text = textblock_StatusCurrentItem.Tag.ToString() + "/" + AppVariables.CurrentTotalItemsCount; }

                    //Adjust the title bar color
                    await AppAdjust.AdjustTitleBarColor(this.RequestedTheme, false, false);

                    AppVariables.HeaderHidden = true;
                }
                else
                {
                    stackpanel_Header.Height = HeaderTargetSize;

                    iconMenu.Margin = new Thickness(0, 0, 0, 0);

                    image_iconMenu.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconMenu.png", false);
                    image_iconMenu.Opacity = 1;

                    grid_StatusApplication.Margin = new Thickness(0, 65, 0, 0);
                    grid_StatusApplication.Background = new SolidColorBrush((Color)Resources["SystemAccentColor"]) { Opacity = 0.60 };
                    border_StatusCurrentItem.Background = new SolidColorBrush((Color)Resources["SystemAccentColor"]) { Opacity = 0.50 };

                    //Update the current item status text
                    textblock_StatusCurrentItem.Text = textblock_StatusCurrentItem.Tag.ToString();

                    //Adjust the title bar color
                    await AppAdjust.AdjustTitleBarColor(this.RequestedTheme, true, false);

                    AppVariables.HeaderHidden = false;
                }
            }
            catch { }
        }

        private async Task HideShowMenu(bool ForceClose)
        {
            try
            {
                int MenuTargetSize = Convert.ToInt32(grid_PopupMenu.Tag);
                int MenuCurrentSize = Convert.ToInt32(grid_PopupMenu.Height);
                if (ForceClose || MenuCurrentSize == MenuTargetSize) { grid_PopupMenu.Height = 0; }
                else
                {
                    grid_PopupMenu.Height = MenuTargetSize;

                    int HeaderTargetSize = Convert.ToInt32(stackpanel_Header.Tag);
                    int HeaderCurrentSize = Convert.ToInt32(stackpanel_Header.Height);
                    if (HeaderCurrentSize < HeaderTargetSize) { await HideShowHeader(false); }
                }
            }
            catch { }
        }
    }
}