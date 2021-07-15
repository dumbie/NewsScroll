using ArnoldVinkCode;
using System;
using System.Diagnostics;
using static ArnoldVinkCode.ArnoldVinkSettings;

namespace NewsScroll
{
    public class Cleanup
    {
        //Cleanup image download cache
        public static void CleanImageDownloadCache()
        {
            try
            {
                Debug.WriteLine("Cleaning image download cache...");
                string[] fileNames = AVFiles.Directory_ListFiles("Cache", true);
                foreach (string fileName in fileNames)
                {
                    DateTime fileDate = AVFiles.File_CreationTime(fileName, false);
                    int removeDays = Convert.ToInt32(AppSettingLoad("RemoveItemsRange"));
                    if (DateTime.Now.Subtract(fileDate).TotalDays > removeDays)
                    {
                        AVFiles.File_Delete(fileName, false);
                        Debug.WriteLine("Removing image cache: " + fileName + fileDate);
                    }
                }
            }
            catch { }
        }
    }
}