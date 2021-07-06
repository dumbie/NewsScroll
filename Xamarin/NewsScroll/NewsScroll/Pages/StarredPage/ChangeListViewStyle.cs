using Xamarin.Forms;

namespace NewsScroll
{
    public partial class StarredPage
    {
        public void ChangeListViewStyle(int Style)
        {
            try
            {
                bool VerticalList = true;
                Style TargetStyle = (Style)Application.Current.Resources["ListViewVertical"];
                if (listview_Items.Style != TargetStyle) { VerticalList = false; }

                if (VerticalList)
                {
                    listview_Items.ItemTemplate = (DataTemplate)Application.Current.Resources["ListViewItemsVertical" + Style];
                }
                else
                {
                    listview_Items.ItemTemplate = (DataTemplate)Application.Current.Resources["ListViewItemsHorizontal" + Style];
                }
            }
            catch { }
        }
    }
}