using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace NewsScroll
{
    public partial class ImageContainer : Grid
    {
        public static readonly DependencyProperty item_image_Property = DependencyProperty.Register(nameof(item_image_Value), typeof(BitmapImage), typeof(ImageContainer), new PropertyMetadata(null, item_image_Changed));
        public BitmapImage item_image_Value
        {
            get { return (BitmapImage)GetValue(item_image_Property); }
            set { SetValue(item_image_Property, value); }
        }
        private static void item_image_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                ImageContainer sender = dependencyObject as ImageContainer;
                if (sender != null)
                {
                    sender.item_image_Update(args);
                }
            }
            catch { }
        }
    }
}