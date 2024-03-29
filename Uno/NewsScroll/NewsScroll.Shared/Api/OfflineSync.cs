﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;

namespace NewsScroll.Api
{
    partial class Api
    {
        //Sync offline changes
        public static async Task SyncOfflineChanges(bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { await EventProgressDisableUI("Syncing offline changes...", true); }
                System.Diagnostics.Debug.WriteLine("Syncing offline changes...");

                //Get current offline sync items
                List<TableOffline> OfflineSyncItemList = await SQLConnection.Table<TableOffline>().ToListAsync();

                //Sync read items
                List<string> ReadStringList = OfflineSyncItemList.Where(x => !string.IsNullOrWhiteSpace(x.item_read_status)).Select(x => x.item_read_status).ToList();
                if (ReadStringList.Any())
                {
                    bool Result = await MarkItemReadStringList(ReadStringList, true);
                    //Remove from list when succeeded
                    if (Result)
                    {
                        int DeletedItems = await SQLConnection.ExecuteAsync("DELETE FROM TableOffline WHERE item_read_status");
                        System.Diagnostics.Debug.WriteLine("Removed " + DeletedItems + " Read offline synced items...");
                    }
                    else { System.Diagnostics.Debug.WriteLine("Failed to sync offline read items."); }
                }

                //Sync unread items
                List<string> UnreadStringList = OfflineSyncItemList.Where(x => !string.IsNullOrWhiteSpace(x.item_unread_status)).Select(x => x.item_unread_status).ToList();
                if (UnreadStringList.Any())
                {
                    bool Result = await MarkItemReadStringList(UnreadStringList, false);
                    //Remove from list when succeeded
                    if (Result)
                    {
                        int DeletedItems = await SQLConnection.ExecuteAsync("DELETE FROM TableOffline WHERE item_unread_status");
                        System.Diagnostics.Debug.WriteLine("Removed " + DeletedItems + " Unread offline synced items...");
                    }
                    else { System.Diagnostics.Debug.WriteLine("Failed to sync offline unread items."); }
                }

                //Sync starred items
                List<string> StarStringList = OfflineSyncItemList.Where(x => !string.IsNullOrWhiteSpace(x.item_star_status)).Select(x => x.item_star_status).ToList();
                if (StarStringList.Any())
                {
                    bool Result = await MarkItemStarStringList(StarStringList, true);
                    //Remove from list when succeeded
                    if (Result)
                    {
                        int DeletedItems = await SQLConnection.ExecuteAsync("DELETE FROM TableOffline WHERE item_star_status");
                        System.Diagnostics.Debug.WriteLine("Removed " + DeletedItems + " Star offline synced items...");
                    }
                    else { System.Diagnostics.Debug.WriteLine("Failed to sync offline star items."); }
                }

                //Sync Unstarred items
                List<string> UnstarStringList = OfflineSyncItemList.Where(x => !string.IsNullOrWhiteSpace(x.item_unstar_status)).Select(x => x.item_unstar_status).ToList();
                if (UnstarStringList.Any())
                {
                    bool Result = await MarkItemStarStringList(UnstarStringList, false);
                    //Remove from list when succeeded
                    if (Result)
                    {
                        int DeletedItems = await SQLConnection.ExecuteAsync("DELETE FROM TableOffline WHERE item_unstar_status");
                        System.Diagnostics.Debug.WriteLine("Removed " + DeletedItems + " unstar offline synced items...");
                    }
                    else { System.Diagnostics.Debug.WriteLine("Failed to sync offline unstar items."); }
                }

                if (EnableUI) { await EventProgressEnableUI(); }
            }
            catch { await EventProgressEnableUI(); }
        }

        //Add offline sync items
        //Types: Read, Unread, Star, Unstar
        public static async Task AddOfflineSync(List<string> AddIds, string AddType)
        {
            try
            {
                //Sync variables
                List<TableOffline> AddSyncItems = new List<TableOffline>();
                foreach (string AddId in AddIds)
                {
                    //Add sync item
                    TableOffline SyncItem = new TableOffline();
                    if (AddType == "Star") { SyncItem.item_star_status = AddId; }
                    else if (AddType == "Unstar") { SyncItem.item_unstar_status = AddId; }
                    else if (AddType == "Read") { SyncItem.item_read_status = AddId; }
                    else if (AddType == "Unread") { SyncItem.item_unread_status = AddId; }
                    AddSyncItems.Add(SyncItem);

                    //Delete opposite
                    await DeleteOppositeOfflineSync(AddId, AddType);
                }

                await SQLConnection.InsertAllAsync(AddSyncItems);
            }
            catch { }
        }

        //Add offline sync item
        //Types: Read, Unread, Star, Unstar
        public static async Task AddOfflineSync(string AddId, string AddType)
        {
            try
            {
                //Add sync item
                TableOffline SyncItem = new TableOffline();
                if (AddType == "Star") { SyncItem.item_star_status = AddId; }
                else if (AddType == "Unstar") { SyncItem.item_unstar_status = AddId; }
                else if (AddType == "Read") { SyncItem.item_read_status = AddId; }
                else if (AddType == "Unread") { SyncItem.item_unread_status = AddId; }
                await SQLConnection.InsertAsync(SyncItem);

                //Delete opposite
                await DeleteOppositeOfflineSync(AddId, AddType);
            }
            catch { }
        }

        //Delete offline sync item
        //Types: Read, Unread, Star, Unstar
        public static async Task DeleteOppositeOfflineSync(string DeleteId, string DeleteType)
        {
            try
            {
                if (DeleteType == "Star") { await SQLConnection.ExecuteAsync("DELETE FROM TableOffline WHERE item_unstar_status = ('" + DeleteId + "')"); }
                else if (DeleteType == "Unstar") { await SQLConnection.ExecuteAsync("DELETE FROM TableOffline WHERE item_star_status = ('" + DeleteId + "')"); }
                else if (DeleteType == "Read") { await SQLConnection.ExecuteAsync("DELETE FROM TableOffline WHERE item_unread_status = ('" + DeleteId + "')"); }
                else if (DeleteType == "Unread") { await SQLConnection.ExecuteAsync("DELETE FROM TableOffline WHERE item_read_status = ('" + DeleteId + "')"); }

                System.Diagnostics.Debug.WriteLine("Removed " + DeleteId + " / " + DeleteType + " offline sync item...");
            }
            catch { }
        }
    }
}