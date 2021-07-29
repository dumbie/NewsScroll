using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace NewsScroll.Classes
{
    public class Items : INotifyPropertyChanged
    {
        private string priv_feed_id;
        public string feed_id
        {
            get { return priv_feed_id; }
            set
            {
                priv_feed_id = value;
                NotifyPropertyChanged();
            }
        }

        private BitmapImage priv_feed_icon;
        public BitmapImage feed_icon
        {
            get { return priv_feed_icon; }
            set
            {
                priv_feed_icon = value;
                NotifyPropertyChanged();
            }
        }

        private string priv_feed_title;
        public string feed_title
        {
            get { return priv_feed_title; }
            set
            {
                priv_feed_title = value;
                NotifyPropertyChanged();
            }
        }

        private string priv_item_id;
        public string item_id
        {
            get { return priv_item_id; }
            set
            {
                priv_item_id = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility priv_item_read_status;
        public Visibility item_read_status
        {
            get { return priv_item_read_status; }
            set
            {
                priv_item_read_status = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility priv_item_star_status;
        public Visibility item_star_status
        {
            get { return priv_item_star_status; }
            set
            {
                priv_item_star_status = value;
                NotifyPropertyChanged();
            }
        }

        private string priv_item_link;
        public string item_link
        {
            get { return priv_item_link; }
            set
            {
                priv_item_link = value;
                NotifyPropertyChanged();
            }
        }

        private string priv_item_datestring;
        public string item_datestring
        {
            get { return priv_item_datestring; }
            set
            {
                priv_item_datestring = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime priv_item_datetime;
        public DateTime item_datetime
        {
            get { return priv_item_datetime; }
            set
            {
                priv_item_datetime = value;
                NotifyPropertyChanged();
            }
        }

        private string priv_item_title;
        public string item_title
        {
            get { return priv_item_title; }
            set
            {
                priv_item_title = value;
                NotifyPropertyChanged();
            }
        }

        private string priv_item_image;
        public string item_image
        {
            get { return priv_item_image; }
            set
            {
                priv_item_image = value;
                NotifyPropertyChanged();
            }
        }

        private string priv_item_image_link;
        public string item_image_link
        {
            get { return priv_item_image_link; }
            set
            {
                priv_item_image_link = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility priv_item_image_visibility;
        public Visibility item_image_visibility
        {
            get { return priv_item_image_visibility; }
            set
            {
                priv_item_image_visibility = value;
                NotifyPropertyChanged();
            }
        }

        private string priv_item_content;
        public string item_content
        {
            get { return priv_item_content; }
            set
            {
                priv_item_content = value;
                NotifyPropertyChanged();
            }
        }

        private string priv_item_content_full;
        public string item_content_full
        {
            get { return priv_item_content_full; }
            set
            {
                priv_item_content_full = value;
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