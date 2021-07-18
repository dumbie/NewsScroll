using NewsScroll.Classes;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NewsScroll.Lists
{
    public class Lists
    {
        public static ObservableCollection<Feeds> List_Feeds = new ObservableCollection<Feeds>();
        public static ObservableCollection<Feeds> List_FeedSelect = new ObservableCollection<Feeds>();
        public static ObservableCollection<Items> List_NewsItems = new ObservableCollection<Items>();
        public static ObservableCollection<Items> List_SearchItems = new ObservableCollection<Items>();
        public static ObservableCollection<Items> List_StarredItems = new ObservableCollection<Items>();

        public static async Task ClearObservableCollection<T>(ObservableCollection<T> ClearList)
        {
            try
            {
                ClearList.Clear();
                while (ClearList.Any()) { await Task.Delay(10); }
            }
            catch { }
        }
    }
}