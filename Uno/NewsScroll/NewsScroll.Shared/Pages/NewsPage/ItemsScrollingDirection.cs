using ArnoldVinkCode;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NewsScroll
{
    public partial class NewsPage
    {
        //Adjust the news items scrolling direction
        public async Task AdjustItemsScrollingDirection(int Direction)
        {
            try
            {
                if (Direction == 0)
                {
                    Style TargetStyle = (Style)Application.Current.Resources["ListViewVertical"];
                    if (ListView_Items.Style != TargetStyle)
                    {
                        ScrollViewer virtualScrollViewer = AVFunctions.FindVisualChild<ScrollViewer>(ListView_Items);
                        if (virtualScrollViewer != null)
                        {
                            object CurrentOffset = EventsScrollViewer.GetCurrentOffsetItem(ListView_Items);
                            ListView_Items.Style = TargetStyle;
                            ListView_Items.ItemTemplate = (DataTemplate)Application.Current.Resources["ListViewItemsVertical" + Convert.ToInt32(AppVariables.ApplicationSettings["ListViewStyle"])];
                            ListView_Items.ItemContainerStyle = (Style)Application.Current.Resources["ListViewItemStretchedVertical"];

                            await Task.Delay(100);
                            ListView_Items.ScrollIntoView(CurrentOffset);
                            System.Diagnostics.Debug.WriteLine("Changed listview to vertical style x.");
                        }

                        //Adjust Status Current Item margin
                        button_StatusCurrentItem.Margin = new Thickness(6, 0, 0, 6);
                    }
                }
                else if (Direction == 1)
                {
                    Style TargetStyle = (Style)Application.Current.Resources["ListViewHorizontal"];
                    if (ListView_Items.Style != TargetStyle)
                    {
                        ScrollViewer virtualScrollViewer = AVFunctions.FindVisualChild<ScrollViewer>(ListView_Items);
                        if (virtualScrollViewer != null)
                        {
                            object CurrentOffset = EventsScrollViewer.GetCurrentOffsetItem(ListView_Items);
                            ListView_Items.Style = TargetStyle;
                            ListView_Items.ItemTemplate = (DataTemplate)Application.Current.Resources["ListViewItemsHorizontal" + Convert.ToInt32(AppVariables.ApplicationSettings["ListViewStyle"])];
                            ListView_Items.ItemContainerStyle = (Style)Application.Current.Resources["ListViewItemStretchedHorizontal"];

                            await Task.Delay(100);
                            ListView_Items.ScrollIntoView(CurrentOffset);
                            System.Diagnostics.Debug.WriteLine("Changed listview to horizontal style x.");
                        }

                        //Adjust Status Current Item margin
                        if (AVFunctions.DevMobile())
                        {
                            button_StatusCurrentItem.Margin = new Thickness(6, 0, 0, 6);
                        }
                        else
                        {
                            button_StatusCurrentItem.Margin = new Thickness(16, 0, 0, 16);
                        }
                    }
                }
                else if (Direction == 2)
                {
                    Rect ScreenSize = AVFunctions.AppWindowResolution();
                    if (ScreenSize.Width > ScreenSize.Height)
                    {
                        await AdjustItemsScrollingDirection(1);
                    }
                    else
                    {
                        await AdjustItemsScrollingDirection(0);
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to adjust item scrolling direction.");
            }
        }
    }
}