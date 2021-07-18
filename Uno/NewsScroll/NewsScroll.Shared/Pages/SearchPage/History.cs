using System;
using System.Linq;
using System.Threading.Tasks;
using static NewsScroll.Database.Database;

namespace NewsScroll
{
    public partial class SearchPage
    {
        //Load the search history
        private async Task LoadSearchHistory()
        {
            try
            {
                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                //Load and add search history
                txtbox_Search.ItemsSource = (await SQLConnection.Table<TableSearchHistory>().ToListAsync()).Select(x => x.search_term).Reverse();

                //Set the autosuggestbox list height
                txtbox_Search.MaxSuggestionListHeight = 140;
            }
            catch { }
        }

        //Add search history to database
        private async Task AddSearchHistory(string SearchTerm)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Adding " + SearchTerm + " to the search list and database...");

                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                //Clear search history from database
                await SQLConnection.ExecuteAsync("DELETE FROM TableSearchHistory WHERE lower(search_term) = ('" + SearchTerm.ToLower() + "')");

                //Add search term to database
                await SQLConnection.ExecuteAsync("INSERT INTO TableSearchHistory(search_term) VALUES ('" + SearchTerm + "')");

                //Count the current search terms
                int CountItems = await SQLConnection.ExecuteScalarAsync<int>("SELECT count(search_term) FROM TableSearchHistory");

                //Cleanup older search terms
                if (CountItems > 20)
                {
                    int DeletedItems = await SQLConnection.ExecuteAsync("DELETE FROM TableSearchHistory WHERE search_term IN(SELECT search_term FROM TableSearchHistory LIMIT 1)");
                    System.Diagnostics.Debug.WriteLine("Removed " + DeletedItems + " older search history...");
                }

                //Update the search history
                await LoadSearchHistory();
            }
            catch { }
        }
    }
}