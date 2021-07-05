using NewsScroll.Classes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NewsScroll
{
    public partial class ItemViewer : INotifyPropertyChanged
    {
        //Webviewer Variables
        private Items priv_ItemViewerItem;
        public Items vItemViewerItem
        {
            get { return priv_ItemViewerItem; }
            set
            {
                priv_ItemViewerItem = value;
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