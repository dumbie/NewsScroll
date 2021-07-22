using Windows.System;
using Windows.UI.Xaml.Input;

namespace NewsScroll
{
    public partial class SearchPage
    {
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
                        await personalizePopup.Popup();
                    }
                    catch { }
                }
                else if (e.Key == VirtualKey.Escape)
                {
                    try { await HideShowHeader(false); } catch { }
                }
            }
            catch { }
        }
    }
}