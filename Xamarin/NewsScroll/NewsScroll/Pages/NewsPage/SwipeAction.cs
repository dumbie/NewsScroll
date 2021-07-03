using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;

namespace NewsScroll
{
    public partial class NewsPage
    {
        //Adjust the swipe bar
        void SwipeBarAdjust()
        {
            try
            {
                int SwipeBarDirection = Convert.ToInt32(AppSettingLoad("SwipeDirection"));
                if (SwipeBarDirection == 0)
                {
                    //Left to right
                    grid_SwipeBar.HeightRequest = double.NaN;
                    grid_SwipeBar.WidthRequest = 15;
                    grid_SwipeBar.Margin = new Thickness(0, 100, 0, 80);
                    grid_SwipeBar.HorizontalOptions = LayoutOptions.Start;
                    grid_SwipeBar.VerticalOptions = LayoutOptions.Fill;
                    //fix set gestureswipe
                }
                else if (SwipeBarDirection == 1)
                {
                    //Top to bottom
                    grid_SwipeBar.HeightRequest = 15;
                    grid_SwipeBar.WidthRequest = double.NaN;
                    grid_SwipeBar.Margin = new Thickness(40, 0, 40, 0);
                    //grid_SwipeBar.HorizontalOptions = HorizontalAlignment.Stretch;
                    //grid_SwipeBar.VerticalOptions = VerticalAlignment.Top;
                }
                else if (SwipeBarDirection == 2)
                {
                    //Right to left
                    grid_SwipeBar.HeightRequest = double.NaN;
                    grid_SwipeBar.WidthRequest = 15;
                    grid_SwipeBar.Margin = new Thickness(0, 100, 0, 80);
                    //grid_SwipeBar.HorizontalOptions = HorizontalAlignment.Right;
                    //grid_SwipeBar.VerticalOptions = VerticalAlignment.Stretch;
                }
                else if (SwipeBarDirection == 3)
                {
                    //Bottom to top
                    grid_SwipeBar.HeightRequest = 15;
                    grid_SwipeBar.WidthRequest = double.NaN;
                    grid_SwipeBar.Margin = new Thickness(40, 0, 40, 0);
                    //grid_SwipeBar.HorizontalOptions = HorizontalOptions.
                    //grid_SwipeBar.VerticalOptions = VerticalAlignment.Bottom;
                }
            }
            catch { }
        }

        //Handle user touch swipe
        void Page_ManipulationCompleted(object sender, SwipedEventArgs e)
        {
            try
            {
                HideShowHeader(false);

                //Reset the item selection
                ListView_Items.SelectedItem = -1;
            }
            catch { }
        }
    }
}