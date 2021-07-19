using ArnoldVinkCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NewsScroll.Database.Database;
using static NewsScroll.Events.Events;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> ItemsRemoveOld(bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { await EventProgressDisableUI("Checking older items...", true); }
                System.Diagnostics.Debug.WriteLine("Checking older items...");

                //Date time calculations
                DateTime RemoveItemsRange = DateTime.UtcNow.AddDays(-Convert.ToDouble(AppVariables.ApplicationSettings["RemoveItemsRange"]));
                List<string> DeleteItems = (await SQLConnection.Table<TableItems>().Where(x => x.item_star_status == false && x.item_datetime < RemoveItemsRange).ToListAsync()).Select(x => x.item_id).ToList();

                int DeleteCount = DeleteItems.Count();
                if (DeleteCount > 0)
                {
                    if (!Silent) { await EventProgressDisableUI("Removing " + DeleteCount + " older items...", true); }
                    System.Diagnostics.Debug.WriteLine("Removing " + DeleteCount + " older items...");

                    string DeleteString = string.Empty;
                    foreach (string DeleteItem in DeleteItems) { DeleteString += "'" + DeleteItem + "',"; }
                    DeleteString = AVFunctions.StringRemoveEnd(DeleteString, ",");

                    int DeletedItems = await SQLConnection.ExecuteAsync("DELETE FROM TableItems WHERE item_id IN (" + DeleteString + ")");
                    System.Diagnostics.Debug.WriteLine("Removed " + DeletedItems + " older items...");
                }

                if (EnableUI) { await EventProgressEnableUI(); }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to delete older items: " + ex.Message);
                await EventProgressEnableUI();
                return false;
            }
        }
    }
}