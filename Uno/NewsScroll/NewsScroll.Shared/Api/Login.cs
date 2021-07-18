using ArnoldVinkCode;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
                System.Diagnostics.Debug.WriteLine("Checking account and password.");
                if (string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiAccount"].ToString()) || string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ApiPassword"].ToString()))
                {
                    System.Diagnostics.Debug.WriteLine("No account or password is set, moving to settings page.");
                    App.vApplicationFrame.Navigate(typeof(SettingsPage));
                    //FixApp.vApplicationFrame.BackStack.Clear();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to check account: " + ex.Message);
                return false;
            }
        }

        //Check if the client is logged in
        static public bool CheckLogin()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AppVariables.ApplicationSettings["ConnectApiAuth"].ToString()))
                {
                    System.Diagnostics.Debug.WriteLine("Client is currently not logged in.");
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
                if (!Silent) { await EventProgressDisableUI("Logging into The Old Reader...", true); }
                System.Diagnostics.Debug.WriteLine("Logging into The Old Reader.");

                StringContent PostContent = new StringContent("client=NewsScroll&accountType=HOSTED_OR_GOOGLE&service=reader&output=json&Email=" + WebUtility.HtmlEncode(AppVariables.ApplicationSettings["ApiAccount"].ToString()) + "&Passwd=" + WebUtility.HtmlEncode(AppVariables.ApplicationSettings["ApiPassword"].ToString()), Encoding.UTF8, "application/x-www-form-urlencoded");
                string PostHttp = await AVDownloader.SendPostRequestAsync(7500, "News Scroll", null, new Uri(ApiConnectionUrl + "accounts/ClientLogin"), PostContent);
                JObject WebJObject = JObject.Parse(PostHttp);
                if (WebJObject["Auth"] != null)
                {
                    ApiMessageError = string.Empty;
                    AppVariables.ApplicationSettings["ConnectApiAuth"] = WebJObject["Auth"].ToString();

                    if (EnableUI) { await EventProgressEnableUI(); }
                    return true;
                }
                else
                {
                    await EventProgressEnableUI();
                    return false;
                }
            }
            catch
            {
                await EventProgressEnableUI();
                return false;
            }
        }
    }
}