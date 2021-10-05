using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;

namespace NewsScroll.Styles
{
    public class DynamicStyle : INotifyPropertyChanged
    {
        //Font sizes
        private double priv_TextSizeSmall = 18;
        public double TextSizeSmall
        {
            get { return priv_TextSizeSmall; }
            set
            {
                priv_TextSizeSmall = value;
                NotifyPropertyChanged();
            }
        }

        private double priv_TextSizeMedium = 20;
        public double TextSizeMedium
        {
            get { return priv_TextSizeMedium; }
            set
            {
                priv_TextSizeMedium = value;
                NotifyPropertyChanged();
            }
        }

        private double priv_TextSizeLarge = 24;
        public double TextSizeLarge
        {
            get { return priv_TextSizeLarge; }
            set
            {
                priv_TextSizeLarge = value;
                NotifyPropertyChanged();
            }
        }

        private double priv_TextSizeHuge = 28;
        public double TextSizeHuge
        {
            get { return priv_TextSizeHuge; }
            set
            {
                priv_TextSizeHuge = value;
                NotifyPropertyChanged();
            }
        }

        //App colors
        private SolidColorBrush priv_ApplicationThemeForeground;
        public SolidColorBrush ApplicationThemeForeground
        {
            get { return priv_ApplicationThemeForeground; }
            set
            {
                priv_ApplicationThemeForeground = value;
                NotifyPropertyChanged();
            }
        }

        private SolidColorBrush priv_ApplicationThemeBackground;
        public SolidColorBrush ApplicationThemeBackground
        {
            get { return priv_ApplicationThemeBackground; }
            set
            {
                priv_ApplicationThemeBackground = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}