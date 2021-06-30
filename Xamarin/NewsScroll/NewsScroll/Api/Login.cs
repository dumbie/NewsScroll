using System.Diagnostics;
using System.Threading.Tasks;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Events.Events;

namespace NewsScroll.Api
{
    partial class Api
    {
        //Check the username and password
        static public bool CheckAccount()
        {
            try
            {
                if (!AppSettingCheck("ApiAccount") || !AppSettingCheck("ApiPassword"))
                {
                    Debug.WriteLine("No account or password is set.");
                    return false;
                }
            }
            catch { }
            return true;
        }

        //Check if the client is logged in
        static public bool CheckLogin()
        {
            try
            {
                //if (String.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ConnectApiAuth"].ToString()))
                //{
                //    Debug.WriteLine("Client is currently not logged in.");
                //    return false;
                //}
            }
            catch { }
            return true;
        }

        //Login to the api
        static public async Task<bool> Login(bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { await EventProgressDisableUI("Logging into The Old Reader...", true); }
                Debug.WriteLine("Logging into The Old Reader.");

                //HttpStringContent PostContent = new HttpStringContent("client=NewsScroll&accountType=HOSTED_OR_GOOGLE&service=reader&output=json&Email=" + WebUtility.HtmlEncode(AppVariables.ApplicationSettings["ApiAccount"].ToString()) + "&Passwd=" + WebUtility.HtmlEncode(AppVariables.ApplicationSettings["ApiPassword"].ToString()), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");
                //HttpResponseMessage PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", null, new Uri(ApiConnectionUrl + "accounts/ClientLogin"), PostContent);
                //JObject WebJObject = JObject.Parse(PostHttp.Content.ToString());
                //if (WebJObject["Auth"] != null)
                //{
                //    ApiMessageError = String.Empty;
                //    AppVariables.ApplicationSettings["ConnectApiAuth"] = WebJObject["Auth"].ToString();

                //    if (EnableUI) { await EventProgressEnableUI(); }
                //    return true;
                //}
                //else
                //{
                //    await EventProgressEnableUI();
                //    return false;
                //}
                return false;
            }
            catch
            {
                await EventProgressEnableUI();
                return false;
            }
        }
    }
}