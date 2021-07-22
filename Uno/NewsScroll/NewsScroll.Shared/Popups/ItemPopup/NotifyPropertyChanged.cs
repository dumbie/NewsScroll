using NewsScroll.Classes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NewsScroll
{
    public partial class ItemPopup : INotifyPropertyChanged
    {
        //Item Variables
        private Items priv_vCurrentItem;
        public Items vCurrentItem
        {
            get { return priv_vCurrentItem; }
            set
            {
                priv_vCurrentItem = value;
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