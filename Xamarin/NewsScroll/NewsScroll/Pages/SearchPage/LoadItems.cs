using NewsScroll.Classes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.AppVariables;
using static NewsScroll.Database.Database;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class SearchPage
    {
        //Search items from database
        async Task LoadItems()
        {
            try
            {
                //Clear all items and reset load count
                await ClearObservableCollection(List_SearchItems);

                //Get the currently selected feed
                vSearchFeed = combobox_FeedSelection.SelectedItem as Feeds;
                if (vSearchFeed.feed_title != null) { vSearchFeedTitle = vSearchFeed.feed_title; }
                if (vSearchFeed.feed_folder_title != null) { vSearchFeedTitle = vSearchFeed.feed_folder_title; }

                //Update the items count placer
                txt_AppInfo.Text = "Searching items";

                Span text1 = new Span { Text = "Your search results for " };
                Span text2 = new Span { Text = vSearchTerm };
                text2.SetDynamicResource(Span.TextColorProperty, "ApplicationAccentLightColor");
                Span text3 = new Span { Text = " in " };
                Span text4 = new Span { Text = vSearchFeedTitle };
                text4.SetDynamicResource(Span.TextColorProperty, "ApplicationAccentLightColor");
                Span text5 = new Span { Text = " will be shown here shortly..." };

                FormattedString formattedString = new FormattedString();
                formattedString.Spans.Add(text1);
                formattedString.Spans.Add(text2);
                formattedString.Spans.Add(text3);
                formattedString.Spans.Add(text4);
                formattedString.Spans.Add(text5);
                txt_NewsScrollInfo.FormattedText = formattedString;
                txt_NewsScrollInfo.IsVisible = true;

                EventProgressDisableUI("Searching for: " + vSearchTerm, true);
                Debug.WriteLine("Searching for: " + vSearchTerm);

                //Add search history to database
                await AddSearchHistory(vSearchTerm);

                //Load items from api/database
                AppVariables.LoadNews = false;
                AppVariables.LoadStarred = false;
                AppVariables.LoadSearch = true;
                AppVariables.LoadFeeds = false;
                int Result = await ApiUpdate.PageApiUpdate();

                //Check the api update result
                if (Result == 2)
                {
                    await CleanupPageResources();
                    App.NavigateToPage(new SettingsPage(), true, false);
                    return;
                }

                //Set all items to list
                List<TableItems> LoadTableItems = await vSQLConnection.Table<TableItems>().ToListAsync();

                //Load items into the list
                await ProcessItemLoad.DatabaseToList(null, LoadTableItems, AppVariables.CurrentItemsLoaded, AppVariables.ItemsToLoadMax, false, false);

                //Update the total items count
                await UpdateTotalItemsCount(null, LoadTableItems, false, true);
            }
            catch { }
        }
    }
}