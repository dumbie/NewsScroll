using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;

namespace NewsScroll
{
    public partial class ItemViewer
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
                        if (AppVariables.BusyApplication) { System.Diagnostics.Debug.WriteLine("Application is too busy to handle mouse shortcut."); return; }
                        System.Diagnostics.Debug.WriteLine("Released pointer: Back");
                        try { ClosePopup(); } catch { }
                    }
                }
            }
            catch { }
        }

        private async void Page_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Released key: " + e.Key);
                if (e.Key == VirtualKey.F2)
                {
                    try
                    {
                        PersonalizePopup personalizePopup = new PersonalizePopup();
                        await personalizePopup.OpenPopup();
                    }
                    catch { }
                }
                else if (e.Key == VirtualKey.F5)
                {
                    if (AppVariables.BusyApplication) { System.Diagnostics.Debug.WriteLine("Application is too busy to handle key shortcut."); return; }
                    try { await LoadFullItem(); } catch { }
                }
                else if (e.Key == VirtualKey.Escape)
                {
                    if (AppVariables.BusyApplication) { System.Diagnostics.Debug.WriteLine("Application is too busy to handle key shortcut."); return; }
                    try { ClosePopup(); } catch { }
                }
            }
            catch { }
        }
    }
}