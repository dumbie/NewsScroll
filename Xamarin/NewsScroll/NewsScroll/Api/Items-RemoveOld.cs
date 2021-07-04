using ArnoldVinkCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.Database.Database;

namespace NewsScroll.Api
{
    partial class Api
    {
        static public async Task<bool> ItemsRemoveOld(bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { EventProgressDisableUI("Checking older items...", true); }
                Debug.WriteLine("Checking older items...");

                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                //Date time calculations
                DateTime RemoveItemsRange = DateTime.UtcNow.AddDays(-Convert.ToDouble(AppSettingLoad("RemoveItemsRange")));
                List<string> DeleteItems = (await vSQLConnection.Table<TableItems>().Where(x => x.item_star_status == false && x.item_datetime < RemoveItemsRange).ToListAsync()).Select(x => x.item_id).ToList();

                int DeleteCount = DeleteItems.Count();
                if (DeleteCount > 0)
                {
                    if (!Silent) { EventProgressDisableUI("Removing " + DeleteCount + " older items...", true); }
                    Debug.WriteLine("Removing " + DeleteCount + " older items...");

                    string DeleteString = string.Empty;
                    foreach (string DeleteItem in DeleteItems) { DeleteString += "'" + DeleteItem + "',"; }
                    DeleteString = AVFunctions.StringRemoveEnd(DeleteString, ",");

                    int DeletedItems = await vSQLConnection.ExecuteAsync("DELETE FROM TableItems WHERE item_id IN (" + DeleteString + ")");
                    Debug.WriteLine("Removed " + DeletedItems + " older items...");
                }

                if (EnableUI) { EventProgressEnableUI(); }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to delete older items: " + ex.Message);
                EventProgressEnableUI();
                return false;
            }
        }
    }
}