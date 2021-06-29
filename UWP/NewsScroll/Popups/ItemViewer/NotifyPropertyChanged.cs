using NewsScroll.Classes;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NewsScroll
{
    public partial class ItemViewer : INotifyPropertyChanged
    {
        //Webviewer Variables
        private Items priv_vCurrentWebSource;
        public Items vCurrentWebSource
        {
            get { return priv_vCurrentWebSource; }
            set
            {
                priv_vCurrentWebSource = value;
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