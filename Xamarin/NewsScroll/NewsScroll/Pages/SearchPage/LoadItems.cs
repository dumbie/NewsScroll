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
                Application.Current.Resources.TryGetValue("ApplicationAccentLightColor", out object ApplicationAccentLightColor);
                FormattedString formattedString = new FormattedString();
                formattedString.Spans.Add(new Span { Text = "Your search results for " });
                formattedString.Spans.Add(new Span { Text = vSearchTerm, TextColor = (Color)ApplicationAccentLightColor });
                formattedString.Spans.Add(new Span { Text = " in " });
                formattedString.Spans.Add(new Span { Text = vSearchFeedTitle, TextColor = (Color)ApplicationAccentLightColor });
                formattedString.Spans.Add(new Span { Text = " will be shown here shortly..." });
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

                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

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