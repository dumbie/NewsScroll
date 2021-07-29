using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NewsScroll
{
    public partial class ImageContainer : Grid
    {
        public static readonly DependencyProperty item_image_Property = DependencyProperty.Register(nameof(item_image_Value), typeof(string), typeof(ImageContainer), new PropertyMetadata(null, item_image_Changed));
        public string item_image_Value
        {
            get { return (string)GetValue(item_image_Property); }
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