using NewsScroll.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppVariables;
using static NewsScroll.Database.Database;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class NewsPage
    {
        //Update Api Start
        async Task LoadItems(bool LoadSelectFeeds, bool UpdateSelectFeeds)
        {
            try
            {
                //Clear all items and reset load count
                await ClearObservableCollection(List_NewsItems);

                //Get the currently selected feed
                string SelectedFeedTitle = "All news items";
                if (!(bool)AppSettingLoad("DisplayReadMarkedItems")) { SelectedFeedTitle = "Current unread items"; }
                if (vNewsFeed != null)
                {
                    if (vNewsFeed.feed_title != null) { SelectedFeedTitle = vNewsFeed.feed_title; }
                    if (vNewsFeed.feed_folder_title != null) { SelectedFeedTitle = vNewsFeed.feed_folder_title; }
                }

                //Update the loading information
                txt_AppInfo.Text = "Loading items";
                Application.Current.Resources.TryGetValue("ApplicationAccentLightColor", out object ApplicationAccentLightColor);
                FormattedString formattedString = new FormattedString();
                formattedString.Spans.Add(new Span { Text = "Your news items from " });
                formattedString.Spans.Add(new Span { Text = SelectedFeedTitle, TextColor = (Color)ApplicationAccentLightColor });
                formattedString.Spans.Add(new Span { Text = " will be shown here shortly..." });
                txt_NewsScrollInfo.FormattedText = formattedString;
                txt_NewsScrollInfo.IsVisible = true;

                //Check the loading feed
                if (LoadSelectFeeds)
                {
                    if ((bool)AppSettingLoad("DisplayReadMarkedItems"))
                    {
                        Feeds TempFeed = new Feeds();
                        TempFeed.feed_id = "0";
                        vNewsFeed = TempFeed;
                        vPreviousScrollItem = 0;
                    }
                    else
                    {
                        Feeds TempFeed = new Feeds();
                        TempFeed.feed_id = "2";
                        vNewsFeed = TempFeed;
                        vPreviousScrollItem = 0;
                    }
                }

                //Load items from api/database
                AppVariables.LoadNews = true;
                AppVariables.LoadStarred = false;
                AppVariables.LoadSearch = false;
                AppVariables.LoadFeeds = true;
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
                List<TableFeeds> LoadTableFeeds = await vSQLConnection.Table<TableFeeds>().OrderBy(x => x.feed_folder).ToListAsync();
                List<TableItems> LoadTableItems = await vSQLConnection.Table<TableItems>().ToListAsync();

                //Load items into the list
                await ProcessItemLoad.DatabaseToList(LoadTableFeeds, LoadTableItems, AppVariables.CurrentItemsLoaded, AppVariables.ItemsToLoadMax, false, false);

                //Load feeds into selector
                if (LoadSelectFeeds)
                {
                    await LoadSelectionFeeds(LoadTableFeeds, LoadTableItems, false, true);
                }

                //Update feeds in selector
                if (UpdateSelectFeeds)
                {
                    await UpdateSelectionFeeds(LoadTableFeeds, LoadTableItems, false, true);
                }

                //Change the selection feed
                ChangeSelectionFeed(vNewsFeed, false);

                //Update the total item count
                UpdateTotalItemsCount();

                //Enable the interface manually
                if (!LoadSelectFeeds && !UpdateSelectFeeds) { ProgressEnableUI(); }
            }
            catch { }
        }
    }
}