using System.Diagnostics;
using Windows.System;
using Windows.UI.Xaml.Input;

namespace NewsScroll
{
    public partial class StarredPage
    {
        private async void Page_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Released key: " + e.Key);
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
                    if (AppVariables.BusyApplication) { Debug.WriteLine("Application is too busy to handle key shortcut."); return; }
                    try { await RefreshItems(); } catch { }
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