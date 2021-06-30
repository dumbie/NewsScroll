using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
                Debug.WriteLine("Checking older items...");

                //Wait for busy database
                await ApiUpdate.WaitForBusyDatabase();

                //Date time calculations
                //DateTime RemoveItemsRange = DateTime.UtcNow.AddDays(-Convert.ToDouble(AppVariables.ApplicationSettings["RemoveItemsRange"]));
                //List<String> DeleteItems = (await vSQLConnection.Table<TableItems>().Where(x => x.item_star_status == false && x.item_datetime < RemoveItemsRange).ToListAsync()).Select(x => x.item_id).ToList();

                //Int32 DeleteCount = DeleteItems.Count();
                //if (DeleteCount > 0)
                //{
                //    if (!Silent) { await EventProgressDisableUI("Removing " + DeleteCount + " older items...", true); }
                //    Debug.WriteLine("Removing " + DeleteCount + " older items...");

                //    string DeleteString = String.Empty;
                //    foreach (String DeleteItem in DeleteItems) { DeleteString += "'" + DeleteItem + "',"; }
                //    DeleteString = AVFunctions.StringRemoveEnd(DeleteString, ",");

                //    Int32 DeletedItems = await vSQLConnection.ExecuteAsync("DELETE FROM TableItems WHERE item_id IN (" + DeleteString + ")");
                //    Debug.WriteLine("Removed " + DeletedItems + " older items...");
                //}

                if (EnableUI) { await EventProgressEnableUI(); }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to delete older items: " + ex.Message);
                await EventProgressEnableUI();
                return false;
            }
        }
    }
}