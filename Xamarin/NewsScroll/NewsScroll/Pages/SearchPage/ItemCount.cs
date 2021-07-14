using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using static NewsScroll.Api.Api;
using static NewsScroll.AppVariables;
using static NewsScroll.Database.Database;

namespace NewsScroll
{
    public partial class SearchPage
    {
        //Update the total item count
        private async Task UpdateTotalItemsCount(List<TableFeeds> LoadTableFeeds, List<TableItems> LoadTableItems, bool Silent, bool EnableUI)
        {
            try
            {
                //Set the total item count
                AppVariables.CurrentTotalItemsCount = await ProcessItemLoad.DatabaseToCount(LoadTableFeeds, LoadTableItems, Silent, EnableUI);
                if (AppVariables.CurrentTotalItemsCount > 0)
                {
                    txt_AppInfo.Text = ApiMessageError + AppVariables.CurrentTotalItemsCount + " items";
                    txt_NewsScrollInfo.IsVisible = false;

                    button_StatusCurrentItem.IsVisible = true;
                }
                else
                {
                    txt_AppInfo.Text = "No results";

                    Span text1 = new Span { Text = "No search results could be found for " };
                    Span text2 = new Span { Text = vSearchTerm };
                    text2.SetDynamicResource(Span.TextColorProperty, "ApplicationAccentLightColor");
                    Span text3 = new Span { Text = " in " };
                    Span text4 = new Span { Text = vSearchFeedTitle };
                    text4.SetDynamicResource(Span.TextColorProperty, "ApplicationAccentLightColor");

                    FormattedString formattedString = new FormattedString();
                    formattedString.Spans.Add(text1);
                    formattedString.Spans.Add(text2);
                    formattedString.Spans.Add(text3);
                    formattedString.Spans.Add(text4);
                    txt_NewsScrollInfo.FormattedText = formattedString;
                    txt_NewsScrollInfo.IsVisible = true;

                    button_StatusCurrentItem.IsVisible = false;

                    //Focus on the text box to open keyboard
                    txtbox_Search.IsEnabled = false;
                    txtbox_Search.IsEnabled = true;
                    txtbox_Search.Focus();
                }

                //Update the current item count
                if (stackpanel_Header.IsVisible || AppVariables.CurrentTotalItemsCount == 0)
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount.ToString();
                }
                else
                {
                    label_StatusCurrentItem.Text = AppVariables.CurrentViewItemsCount + "/" + AppVariables.CurrentTotalItemsCount;
                }
            }
            catch { }
        }
    }
}