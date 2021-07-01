using NewsScroll.Classes;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace NewsScroll.Api
{
    partial class Api
    {
        //Share item
        public static async Task ShareItem(Items ListFeedItem)
        {
            try
            {
                string Description = "I just found out an interesting item to read, click on the link to open it.";
                await Share.RequestAsync(new ShareTextRequest
                {
                    Uri = ListFeedItem.item_link,
                    Title = ListFeedItem.item_title,
                    Text = Description,
                });
            }
            catch { }
        }
    }
}