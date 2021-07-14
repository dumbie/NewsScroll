using ArnoldVinkCode;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;
using static NewsScroll.AppEvents.AppEvents;

namespace NewsScroll.Database
{
    public class Database
    {
        //Load the News Scroll database size
        public static string GetDatabaseSize()
        {
            string DatabaseSize = "empty";
            try
            {
                long fileSize = AVFiles.File_Size("Database.sqlite", true);
                DatabaseSize = string.Format("{0:0.00}", +decimal.Divide(fileSize, 1048576)) + "MB";
            }
            catch { }
            return DatabaseSize;
        }

        //Database Connection
        public static SQLiteAsyncConnection vSQLConnection = null;
        public static bool DatabaseConnect()
        {
            try
            {
                string databaseFilename = "Database.sqlite";
                string databaseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string databasePath = Path.Combine(databaseFolder, databaseFilename);

                SQLiteConnectionString connectionString = new SQLiteConnectionString(databasePath, true);
                vSQLConnection = new SQLiteAsyncConnection(connectionString);

                Debug.WriteLine("Successfully connected to the database.");
                return true;
            }
            catch
            {
                Debug.WriteLine("Failed to connect to the database.");
                return false;
            }
        }

        //Database Create
        public static async Task DatabaseCreate()
        {
            try
            {
                await vSQLConnection.CreateTableAsync<TableFeeds>();
                await vSQLConnection.CreateTableAsync<TableOffline>();
                await vSQLConnection.CreateTableAsync<TableItems>();
                await vSQLConnection.CreateTableAsync<TableSearchHistory>();

                Debug.WriteLine("Created the database tables.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed creating the database tables: " + ex.Message);
            }
        }

        //Database Reset
        public static async Task DatabaseReset()
        {
            try
            {
                if (EventProgressDisableUI != null)
                {
                    EventProgressDisableUI("Resetting the database.", true);
                }
                Debug.WriteLine("Resetting the database.");

                //Delete all files from local storage
                string[] localFiles = AVFiles.Directory_ListFiles(string.Empty, true);
                foreach (string localFile in localFiles)
                {
                    AVFiles.File_Delete(localFile, false);
                }

                //Reset the online status
                OnlineUpdateFeeds = true;
                OnlineUpdateNews = true;
                OnlineUpdateStarred = true;
                ApiMessageError = string.Empty;

                //Reset the last update setting
                await AppSettingSave("LastItemsUpdate", "Never");

                Debug.WriteLine("Resetted the database.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed resetting the database: " + ex.Message);
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