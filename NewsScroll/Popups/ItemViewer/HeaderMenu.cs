using ArnoldVinkCode;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static NewsScroll.Events.Events;

namespace NewsScroll
{
    public partial class ItemViewer
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
                await EventHideShowHeader(false);
            }
            catch { }
        }

        private async Task HideShowHeader(bool ForceClose)
        {
            try
            {
                Int32 HeaderTargetSize = Convert.ToInt32(stackpanel_Header.Tag);
                Int32 HeaderCurrentSize = Convert.ToInt32(stackpanel_Header.Height);
                if (ForceClose || HeaderCurrentSize == HeaderTargetSize)
                {
                    AVAnimations.Ani_Height(stackpanel_Header, 0, false, 0.075);
                    await HideShowMenu(true);

                    if (!AVFunctions.DevMobile()) { iconMenu.Margin = new Thickness(0, 0, 16, 0); }

                    image_iconMenu.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconMenu-Dark.png", false);
                    image_iconMenu.Opacity = 0.60;

                    image_iconBack.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconBack-Dark.png", false);
                    image_iconBack.Opacity = 0.60;

                    grid_StatusApplication.Margin = new Thickness(0, 0, 0, 0);
                    grid_StatusApplication.Background = new SolidColorBrush(Color.FromArgb(255, 88, 88, 88)) { Opacity = 0.60 };
                    border_StatusCurrentItem.Background = new SolidColorBrush(Color.FromArgb(255, 88, 88, 88)) { Opacity = 0.50 };

                    //Adjust the title bar color
                    await AppAdjust.AdjustTitleBarColor(this.RequestedTheme, false, false);

                    AppVariables.HeaderHidden = true;
                }
                else
                {
                    AVAnimations.Ani_Height(stackpanel_Header, HeaderTargetSize, true, 0.075);

                    iconMenu.Margin = new Thickness(0, 0, 0, 0);

                    image_iconMenu.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconMenu.png", false);
                    image_iconMenu.Opacity = 1;

                    image_iconBack.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconBack.png", false);
                    image_iconBack.Opacity = 1;

                    grid_StatusApplication.Margin = new Thickness(0, 65, 0, 0);
                    grid_StatusApplication.Background = new SolidColorBrush((Color)Resources["SystemAccentColor"]) { Opacity = 0.60 };
                    border_StatusCurrentItem.Background = new SolidColorBrush((Color)Resources["SystemAccentColor"]) { Opacity = 0.50 };

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
                Int32 MenuTargetSize = Convert.ToInt32(grid_PopupMenu.Tag);
                Int32 MenuCurrentSize = Convert.ToInt32(grid_PopupMenu.Height);
                if (ForceClose || MenuCurrentSize == MenuTargetSize) { AVAnimations.Ani_Height(grid_PopupMenu, 0, false, 0.075); }
                else
                {
                    AVAnimations.Ani_Height(grid_PopupMenu, MenuTargetSize, true, 0.075);

                    Int32 HeaderTargetSize = Convert.ToInt32(stackpanel_Header.Tag);
                    Int32 HeaderCurrentSize = Convert.ToInt32(stackpanel_Header.Height);
                    if (HeaderCurrentSize < HeaderTargetSize) { await EventHideShowHeader(false); }
                }
            }
            catch { }
        }
    }
}