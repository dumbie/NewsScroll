using System;

namespace NewsScroll
{
    public partial class ApiPage
    {
        void iconMenu_Tapped(object sender, EventArgs e)
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
                if (ForceClose || grid_PopupMenu.IsVisible)
                {
                    grid_PopupMenu.IsVisible = false;
                }
                else
                {
                    grid_PopupMenu.IsVisible = true;
                }
            }
            catch { }
        }
    }
}