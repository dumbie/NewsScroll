﻿using Windows.System;
using Windows.UI.Xaml.Input;

namespace NewsScroll
{
    public partial class ApiPage
    {
        private async void Page_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Released key: " + e.Key);
                if (e.Key == VirtualKey.F5)
                {
                    if (AppVariables.BusyApplication) { System.Diagnostics.Debug.WriteLine("Application is too busy to handle key shortcut."); return; }
                    try { await RefreshFeeds(); } catch { }
                }
            }
            catch { }
        }
    }
}