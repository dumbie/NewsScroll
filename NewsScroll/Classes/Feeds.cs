using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace NewsScroll.Classes
{
    public class Feeds : INotifyPropertyChanged
    {
        //Feed
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

        private string priv_feed_link;
        public string feed_link
        {
            get { return priv_feed_link; }
            set
            {
                priv_feed_link = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility priv_feed_ignore_status;
        public Visibility feed_ignore_status
        {
            get { return priv_feed_ignore_status; }
            set
            {
                priv_feed_ignore_status = value;
                NotifyPropertyChanged();
            }
        }

        private Int32 priv_feed_item_count;
        public Int32 feed_item_count
        {
            get { return priv_feed_item_count; }
            set
            {
                priv_feed_item_count = value;
                NotifyPropertyChanged();
            }
        }

        //Collection
        private Boolean priv_feed_collection_status = false;
        public Boolean feed_collection_status
        {
            get { return priv_feed_collection_status; }
            set
            {
                priv_feed_collection_status = value;
                NotifyPropertyChanged();
            }
        }

        //Folder
        private string priv_feed_folder_title;
        public string feed_folder_title
        {
            get { return priv_feed_folder_title; }
            set
            {
                priv_feed_folder_title = value;
                NotifyPropertyChanged();
            }
        }

        private Boolean priv_feed_folder_status = false;
        public Boolean feed_folder_status
        {
            get { return priv_feed_folder_status; }
            set
            {
                priv_feed_folder_status = value;
                NotifyPropertyChanged();
            }
        }

        private List<String> priv_feed_folder_ids = new List<String>();
        public List<String> feed_folder_ids
        {
            get { return priv_feed_folder_ids; }
            set
            {
                priv_feed_folder_ids = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}