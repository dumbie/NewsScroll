using Xamarin.Forms;

namespace NewsScroll
{
    public partial class StarredPage
    {
        public void ChangeListViewStyle(int Style)
        {
            try
            {
                bool verticalList = true;
                Application.Current.Resources.TryGetValue("ListViewVertical", out object TargetStyle);
                if (listview_Items.Style != (Style)TargetStyle)
                {
                    verticalList = false;
                }
                //Debug.WriteLine("Vertical listview: " + verticalList);

                if (verticalList)
                {
                    Application.Current.Resources.TryGetValue("ListViewItemsVertical" + Style, out object TargetTemplate);
                    listview_Items.ItemTemplate = (DataTemplate)TargetTemplate;
                }
                else
                {
                    Application.Current.Resources.TryGetValue("ListViewItemsHorizontal" + Style, out object TargetTemplate);
                    listview_Items.ItemTemplate = (DataTemplate)TargetTemplate;
                }
            }
            catch { }
        }

        public void ChangeListViewDirection(int Direction)
        {
            try
            {
                //if (Direction == 0)
                //{
                //    Application.Current.Resources.TryGetValue("ListViewVertical", out object TargetStyle);
                //    listview_Items.Style = (Style)TargetStyle;
                //}
                //else
                //{
                //    Application.Current.Resources.TryGetValue("ListViewHorizontal", out object TargetStyle);
                //    listview_Items.Style = (Style)TargetStyle;
                //}
            }
            catch { }
        }
    }
}