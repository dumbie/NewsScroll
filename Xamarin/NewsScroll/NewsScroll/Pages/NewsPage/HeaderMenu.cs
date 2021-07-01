using System;
using System.Diagnostics;
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
                    Debug.WriteLine("Single click menu.");
                    HideShowMenu(false);
                }
                else if (doubleClick == 2)
                {
                    Debug.WriteLine("Double click menu.");
                    HideShowHeader(false);
                }
            }
            catch { }
        }

        private void HideShowHeader(bool ForceClose)
        {
            try
            {
                Debug.WriteLine("Current menu visible: " + grid_PopupMenu.IsVisible);
                Debug.WriteLine("Current header visible: " + stacklayout_Header.IsVisible);
                if (ForceClose || stacklayout_Header.IsVisible)
                {
                    stacklayout_Header.IsVisible = false;
                    grid_PopupMenu.IsVisible = false;

                    // iconMenu.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconMenu-Dark.png", false);
                    iconMenu.Opacity = 0.60;

                    grid_StatusApplication.Margin = new Thickness(0, 0, 0, 0);
                    //grid_StatusApplication.Background = new SolidColorBrush(Color.FromArgb(255, 88, 88, 88)) { Opacity = 0.60 };
                    //border_StatusCurrentItem.Background = new SolidColorBrush(Color.FromArgb(255, 88, 88, 88)) { Opacity = 0.50 };

                    //Update the current item status text
                    if (AppVariables.CurrentTotalItemsCount == 0)
                    {
                        // label_StatusCurrentItem.Text = label_StatusCurrentItem.Tag.ToString();
                    }
                    else
                    {
                        // label_StatusCurrentItem.Text = label_StatusCurrentItem.Tag.ToString() + "/" + AppVariables.CurrentTotalItemsCount;
                    }

                    //Adjust the title bar color
                    //await AppAdjust.AdjustTitleBarColor(this.RequestedTheme, false, false);

                    AppVariables.HeaderHidden = true;
                }
                else
                {
                    stacklayout_Header.IsVisible = true;

                    iconMenu.Margin = new Thickness(0, 0, 0, 0);

                    //iconMenu.Source = await AVImage.LoadBitmapImage("ms-appx:///Assets/iconMenu.png", false);
                    iconMenu.Opacity = 1;

                    grid_StatusApplication.Margin = new Thickness(0, 65, 0, 0);
                    //grid_StatusApplication.Background = new SolidColorBrush((Color)Resources["SystemAccentColor"]) { Opacity = 0.60 };
                    //border_StatusCurrentItem.Background = new SolidColorBrush((Color)Resources["SystemAccentColor"]) { Opacity = 0.50 };

                    //Update the current item status text
                    //label_StatusCurrentItem.Text = label_StatusCurrentItem.Tag.ToString();

                    //Adjust the title bar color
                    //await AppAdjust.AdjustTitleBarColor(this.RequestedTheme, true, false);

                    AppVariables.HeaderHidden = false;
                }
            }
            catch { }
        }

        private void HideShowMenu(bool ForceClose)
        {
            try
            {
                //Debug.WriteLine("Current menu visible: " + grid_PopupMenu.IsVisible);
                //Debug.WriteLine("Current header visible: " + stacklayout_Header.IsVisible);
                if (ForceClose || grid_PopupMenu.IsVisible)
                {
                    grid_PopupMenu.IsVisible = false;
                }
                else
                {
                    if (!stacklayout_Header.IsVisible) { HideShowHeader(false); }
                    grid_PopupMenu.IsVisible = true;
                }
            }
            catch { }
        }
    }
}