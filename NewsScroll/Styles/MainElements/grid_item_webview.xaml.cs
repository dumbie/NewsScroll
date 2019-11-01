using ArnoldVinkCode;
using Windows.UI.Xaml;

namespace NewsScroll.MainElements
{
    public partial class grid_item_webview
    {
        public grid_item_webview()
        {
            this.InitializeComponent();
            this.Loaded += delegate
            {
                if (AVFunctions.DevMobile())
                {
                    grid_MobileScrollLeft.Visibility = Visibility.Visible;
                    grid_MobileScrollRight.Visibility = Visibility.Visible;
                }
            };
        }
    }
}