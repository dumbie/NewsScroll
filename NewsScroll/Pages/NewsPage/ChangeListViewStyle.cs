﻿using System;
using Windows.UI.Xaml;

namespace NewsScroll
{
    public partial class NewsPage
    {
        public void ChangeListViewStyle(Int32 Style)
        {
            try
            {
                bool VerticalList = true;
                Style TargetStyle = (Style)Application.Current.Resources["ListViewVertical"];
                if (ListView_Items.Style != TargetStyle) { VerticalList = false; }

                if (VerticalList)
                {
                    ListView_Items.ItemTemplate = (DataTemplate)Application.Current.Resources["ListViewItemsVertical" + Style];
                }
                else
                {
                    ListView_Items.ItemTemplate = (DataTemplate)Application.Current.Resources["ListViewItemsHorizontal" + Style];
                }
            }
            catch { }
        }
    }
}