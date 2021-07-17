using System;
using Xamarin.Forms;
using static NewsScroll.AppVariables;

namespace NewsScroll
{
    public partial class NewsPage
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
                if (ForceClose || stackpanel_Header.IsVisible)
                {
                    stackpanel_Header.IsVisible = false;
                    grid_PopupMenu.IsVisible = false;

                    iconMenu.Source = ImageSource.FromResource("NewsScroll.Assets.iconMenu-Dark.png");
                    iconMenu.Opacity = 0.60;

                    //Update the current item status text
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount + "/" + AppVariables.CurrentTotalItemsCount;

                    grid_StatusApplication.Margin = new Thickness(0, 0, 0, 0);

                    AppVariables.HeaderHidden = true;
                }
                else
                {
                    stackpanel_Header.IsVisible = true;

                    iconMenu.Source = ImageSource.FromResource("NewsScroll.Assets.iconMenu.png");
                    iconMenu.Opacity = 1;

                    //Update the current item status text
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount.ToString();

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
                    if (!stackpanel_Header.IsVisible) { HideShowHeader(false); }
                    grid_PopupMenu.IsVisible = true;
                }
            }
            catch { }
        }
    }
}