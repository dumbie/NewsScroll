using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;

namespace NewsScroll
{
    public partial class ImageViewer
    {
        private void Page_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                {
                    PointerPointProperties PointerProperties = e.GetCurrentPoint(this).Properties;
                    if (PointerProperties.IsXButton1Pressed)
                    {
                        System.Diagnostics.Debug.WriteLine("Released pointer: Back");
                        try { ClosePopup(); } catch { }
                    }
                }
            }
            catch { }
        }

        private void Page_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Released key: " + e.Key);
                if (e.Key == VirtualKey.Escape)
                {
                    try { ClosePopup(); } catch { }
                }
            }
            catch { }
        }
    }
}