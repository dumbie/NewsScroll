using System;
using Xamarin.Forms;
using static NewsScroll.AppVariables;

namespace NewsScroll
{
    public partial class ItemViewer
    {
        async void iconMenu_Tapped(object sender, EventArgs e)
        {
            try
            {
                int doubleClick = await DoubleClickCheck();
                if (doubleClick == 1)
                {
                    HideShowMenu(false);
                }
                else if (doubleClick == 2)
                {
                    HideShowHeader(false);
                }
            }
            catch { }
        }

        private void HideShowHeader(bool ForceClose)
        {
            try
            {
                if (ForceClose || StackLayout_Header.IsVisible)
                {
                    StackLayout_Header.IsVisible = false;
                    grid_PopupMenu.IsVisible = false;

                    iconBack.Source = ImageSource.FromResource("NewsScroll.Assets.iconBack-Dark.png");
                    iconBack.Opacity = 0.60;
                    iconMenu.Source = ImageSource.FromResource("NewsScroll.Assets.iconMenu-Dark.png");
                    iconMenu.Opacity = 0.60;

                    grid_StatusApplication.Margin = new Thickness(0, 0, 0, 0);

                    AppVariables.HeaderHidden = true;
                }
                else
                {
                    StackLayout_Header.IsVisible = true;

                    iconBack.Source = ImageSource.FromResource("NewsScroll.Assets.iconBack.png");
                    iconBack.Opacity = 1;
                    iconMenu.Source = ImageSource.FromResource("NewsScroll.Assets.iconMenu.png");
                    iconMenu.Opacity = 1;

                    grid_StatusApplication.Margin = new Thickness(0, 65, 0, 0);

                    AppVariables.HeaderHidden = false;
                }
            }
            catch { }
        }

        private void HideShowMenu(bool ForceClose)
        {
            try
            {
                if (ForceClose || grid_PopupMenu.IsVisible)
                {
                    grid_PopupMenu.IsVisible = false;
                }
                else
                {
                    if (!StackLayout_Header.IsVisible) { HideShowHeader(false); }
                    grid_PopupMenu.IsVisible = true;
                }
            }
            catch { }
        }
    }
}