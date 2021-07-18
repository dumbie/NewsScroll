using NewsScroll.Classes;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace NewsScroll.Api
{
    partial class Api
    {
        //Share item
        public static void ShareItem(Items ListFeedItem)
        {
            try
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += (args, xout) =>
                {
                    string Description = "I just found out an interesting item to read, click on the link to open it.";

                    xout.Request.Data.SetWebLink(new Uri(ListFeedItem.item_link));
                    xout.Request.Data.Properties.Title = ListFeedItem.item_title;
                    xout.Request.Data.Properties.Description = Description;
                    xout.Request.Data.SetText(Description);
                };

                DataTransferManager.ShowShareUI();
            }
            catch { }
        }
    }
}