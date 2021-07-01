using ArnoldVinkCode;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
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

                if (string.IsNullOrWhiteSpace(AppSettingLoad("ApiAccount").ToString()) || string.IsNullOrWhiteSpace(AppSettingLoad("ApiPassword").ToString()))
                {
                    Debug.WriteLine("Empty account or password is set.");
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
                if (string.IsNullOrWhiteSpace(AppSettingLoad("ConnectApiAuth").ToString()))
                {
                    Debug.WriteLine("Client is currently not logged in.");
                    return false;
                }
            }
            catch { }
            return true;
        }

        //Login to the api
        static public async Task<bool> Login(bool Silent, bool EnableUI)
        {
            try
            {
                if (!Silent) { EventProgressDisableUI("Logging into The Old Reader...", true); }
                Debug.WriteLine("Logging into The Old Reader.");

                string PostString = "client=NewsScroll&accountType=HOSTED_OR_GOOGLE&service=reader&output=json&Email=" + WebUtility.HtmlEncode(AppSettingLoad("ApiAccount").ToString()) + "&Passwd=" + WebUtility.HtmlEncode(AppSettingLoad("ApiPassword").ToString());
                StringContent PostContent = new StringContent(PostString, Encoding.UTF8, "application/x-www-form-urlencoded");
                Uri PostUri = new Uri(ApiConnectionUrl + "accounts/ClientLogin");

                string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", null, PostUri, PostContent);

                JObject WebJObject = JObject.Parse(PostHttp);
                if (WebJObject["Auth"] != null)
                {
                    ApiMessageError = string.Empty;
                    await AppSettingSave("ConnectApiAuth", WebJObject["Auth"].ToString());
                    if (EnableUI) { EventProgressEnableUI(); }
                    return true;
                }
                else
                {
                    EventProgressEnableUI();
                    return false;
                }
            }
            catch
            {
                EventProgressEnableUI();
                return false;
            }
        }
    }
}