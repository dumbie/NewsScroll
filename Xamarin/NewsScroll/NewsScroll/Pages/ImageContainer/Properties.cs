using Xamarin.Forms;

namespace NewsScroll
{
    public partial class ImageContainer
    {
        public static readonly BindableProperty image_linkProperty = BindableProperty.Create("image_link", typeof(string), typeof(ImageContainer), null, BindingMode.TwoWay, null, image_linkChanged);
        public string image_link
        {
            get { return (string)GetValue(image_linkProperty); }
            set { SetValue(image_linkProperty, value); }
        }
        private static async void image_linkChanged(BindableObject bindableObject, object oldValue, object newValue)
        {
            try
            {
                ImageContainer sender = bindableObject as ImageContainer;
                if (sender != null)
                {
                    await sender.SetImageLink(newValue.ToString());
                }
            }
            catch { }
        }
    }
}