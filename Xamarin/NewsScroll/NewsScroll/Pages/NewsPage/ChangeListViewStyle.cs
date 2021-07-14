using Xamarin.Forms;

namespace NewsScroll
{
    public partial class NewsPage
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
                    listview_Items.SetDynamicResource(ListView.ItemTemplateProperty, "ListViewItemsVertical" + Style);
                }
                else
                {
                    listview_Items.SetDynamicResource(ListView.ItemTemplateProperty, "ListViewItemsHorizontal" + Style);
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