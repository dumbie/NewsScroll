using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using static NewsScroll.Api.Api;
using static NewsScroll.Events.Events;

namespace NewsScroll.Database
{
    public class Database
    {
        //Load the News Scroll database size
        public static async Task<string> GetDatabaseSize()
        {
            string databaseSize = "empty";
            try
            {
                string databaseFilename = "Database.sqlite";
                string databaseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string databasePath = Path.Combine(databaseFolder, databaseFilename);

                StorageFile StorageFile = await StorageFile.GetFileFromPathAsync(databasePath);
                BasicProperties BasicProperties = await StorageFile.GetBasicPropertiesAsync();

                databaseSize = string.Format("{0:0.00}", +decimal.Divide(BasicProperties.Size, 1048576)) + "MB";
            }
            catch { }
            return databaseSize;
        }

        //Database Connection
        public static SQLiteAsyncConnection SQLConnection = null;
        public static bool DatabaseConnect()
        {
            try
            {
                string databaseFilename = "Database.sqlite";
                string databaseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string databasePath = Path.Combine(databaseFolder, databaseFilename);

                SQLiteConnectionString connectionString = new SQLiteConnectionString(databasePath, true);
                SQLConnection = new SQLiteAsyncConnection(connectionString);

                System.Diagnostics.Debug.WriteLine("Successfully connected to the database.");
                return true;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to connect to the database.");
                return false;
            }
        }

        //Database Create
        public static async Task DatabaseCreate()
        {
            try
            {
                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                await SQLConnection.CreateTableAsync<TableFeeds>();
                await SQLConnection.CreateTableAsync<TableOffline>();
                await SQLConnection.CreateTableAsync<TableItems>();
                await SQLConnection.CreateTableAsync<TableSearchHistory>();

                System.Diagnostics.Debug.WriteLine("Created the database tables.");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed creating the database tables.");
            }
        }

        //Database Reset
        public static async Task DatabaseReset()
        {
            try
            {
                await EventProgressDisableUI("Resetting the database.", true);
                System.Diagnostics.Debug.WriteLine("Resetting the database.");

                //Delete all files from local storage
                foreach (IStorageItem LocalFile in await ApplicationData.Current.LocalFolder.GetItemsAsync())
                {
                    try { await LocalFile.DeleteAsync(StorageDeleteOption.PermanentDelete); } catch { }
                }

                //Reset the online status
                OnlineUpdateFeeds = true;
                OnlineUpdateNews = true;
                OnlineUpdateStarred = true;
                ApiMessageError = string.Empty;

                //Reset the last update setting
                AppVariables.ApplicationSettings["LastItemsUpdate"] = "Never";

                System.Diagnostics.Debug.WriteLine("Resetted the database.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed resetting the database: " + ex.Message);
            }
        }

        //Database Tables
        //Feeds Table
        public class TableFeeds
        {
            [PrimaryKey]
            public string feed_id { get; set; }
            public string feed_folder { get; set; }
            public string feed_title { get; set; }
            public string feed_link { get; set; }
            public bool feed_ignore_status { get; set; }
        }

        //OfflineSync Table
        public class TableOffline
        {
            public string item_read_status { get; set; }
            public string item_unread_status { get; set; }
            public string item_star_status { get; set; }
            public string item_unstar_status { get; set; }
        }

        //Items Table
        public class TableItems
        {
            [PrimaryKey]
            public string item_id { get; set; }
            public string item_feed_id { get; set; }
            public string item_feed_title { get; set; }
            public string item_link { get; set; }
            public string item_title { get; set; }
            public string item_author { get; set; }
            public DateTime item_datetime { get; set; }
            public string item_image { get; set; }
            public string item_content { get; set; }
            public string item_content_full { get; set; }
            public bool item_read_status { get; set; }
            public bool item_star_status { get; set; }
        }

        //SearchHistory Table
        public class TableSearchHistory
        {
            public string search_term { get; set; }
        }
    }
}