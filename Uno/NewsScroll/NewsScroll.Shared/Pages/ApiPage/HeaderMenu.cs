using Windows.UI.Xaml;

namespace NewsScroll
{
    public partial class ApiPage
    {
        void iconMenu_Tap(object sender, RoutedEventArgs e)
        {
            try
            {
                HideShowMenu(false);
            }
            catch { }
        }

        void HideShowMenu(bool ForceClose)
        {
            try
            {
                if (ForceClose || grid_PopupMenu.Visibility == Visibility.Visible)
                {
                    grid_PopupMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grid_PopupMenu.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }
    }
}