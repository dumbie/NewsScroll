using ArnoldVinkCode;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NewsScroll
{
    public partial class StarredPage
    {
        //Adjust the news items scrolling direction
        public static bool AdjustingItemsScrollingDirection = false;
        public async Task AdjustItemsScrollingDirection(Int32 Direction)
        {
            try
            {
                if (!AdjustingItemsScrollingDirection)
                {
                    if (Direction == 0)
                    {
                        Style TargetStyle = (Style)Application.Current.Resources["ListViewVertical"];
                        if (ListView_Items.Style != TargetStyle)
                        {
                            AdjustingItemsScrollingDirection = true;
                            Double[] CurrentOffset = EventsScrollViewer.GetCurrentOffset(ListView_Items);

                            ScrollViewer virtualScrollViewer = AVFunctions.FindVisualChild<ScrollViewer>(ListView_Items);
                            if (virtualScrollViewer != null)
                            {
                                ListView_Items.Style = TargetStyle;
                                ListView_Items.ItemTemplate = (DataTemplate)Application.Current.Resources["ListViewItemsVertical" + Convert.ToInt32(AppVariables.ApplicationSettings["ListViewStyle"])];
                                ListView_Items.ItemContainerStyle = (Style)Application.Current.Resources["ListViewItemStretchedVertical"];

                                await Task.Delay(10);
                                virtualScrollViewer.ChangeView(CurrentOffset[1], (CurrentOffset[0] + 2), null);
                                //Debug.WriteLine("Scrolling to vertical:" + (CurrentOffset[0] + 2));
                            }

                            //Adjust Status Current Item margin
                            button_StatusCurrentItem.Margin = new Thickness(6, 0, 0, 6);

                            AdjustingItemsScrollingDirection = false;
                        }
                    }
                    else if (Direction == 1)
                    {
                        Style TargetStyle = (Style)Application.Current.Resources["ListViewHorizontal"];
                        if (ListView_Items.Style != TargetStyle)
                        {
                            AdjustingItemsScrollingDirection = true;
                            Double[] CurrentOffset = EventsScrollViewer.GetCurrentOffset(ListView_Items);

                            ScrollViewer virtualScrollViewer = AVFunctions.FindVisualChild<ScrollViewer>(ListView_Items);
                            if (virtualScrollViewer != null)
                            {
                                ListView_Items.Style = TargetStyle;
                                ListView_Items.ItemTemplate = (DataTemplate)Application.Current.Resources["ListViewItemsHorizontal" + Convert.ToInt32(AppVariables.ApplicationSettings["ListViewStyle"])];
                                ListView_Items.ItemContainerStyle = (Style)Application.Current.Resources["ListViewItemStretchedHorizontal"];

                                await Task.Delay(10);
                                virtualScrollViewer.ChangeView((CurrentOffset[1] + 2), CurrentOffset[0], null);
                                //Debug.WriteLine("Scrolling to horizontal:" + (CurrentOffset[1] + 2));
                            }

                            //Adjust Status Current Item margin
                            if (AVFunctions.DevMobile()) { button_StatusCurrentItem.Margin = new Thickness(6, 0, 0, 6); }
                            else { button_StatusCurrentItem.Margin = new Thickness(16, 0, 0, 16); }

                            AdjustingItemsScrollingDirection = false;
                        }
                    }
                    else if (Direction == 2)
                    {
                        Rect ScreenSize = AVFunctions.AppWindowResolution();
                        if (ScreenSize.Width > ScreenSize.Height) { await AdjustItemsScrollingDirection(1); }
                        else { await AdjustItemsScrollingDirection(0); }
                    }
                }
            }
            catch { AdjustingItemsScrollingDirection = false; }
        }
    }
}