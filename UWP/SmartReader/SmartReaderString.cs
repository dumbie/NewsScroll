using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartReader
{
    public partial class Reader
    {
        //Arnold Encoding Fix
        private async Task<string> GetStringAsync(Uri resource)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("News Scroll (Robot; Bot)");
                HttpResponseMessage response = await httpClient.GetAsync(resource).ConfigureAwait(false);
                string dati = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    var headLan = response.Content.Headers.FirstOrDefault(x => x.Key.ToLower() == "content-language");
                    if (headLan.Value != null && headLan.Value.Any())
                    {
                        language = headLan.Value.ElementAt(0);
                    }

                    var headCont = response.Content.Headers.FirstOrDefault(x => x.Key.ToLower() == "content-type");
                    if (headCont.Value != null && headCont.Value.Any())
                    {
                        int index = headCont.Value.ElementAt(0).IndexOf("charset=");
                        if (index != -1)
                        {
                            charset = headCont.Value.ElementAt(0).Substring(index + 8);
                        }
                    }

                    //Check the site used encoding
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    if (charset != null && charset.ToLower().StartsWith("iso-"))
                    {
                        Encoding siteEncoding = null;
                        try
                        {
                            siteEncoding = Encoding.GetEncoding(charset);
                        }
                        catch
                        {
                            siteEncoding = Encoding.UTF8;
                        }

                        byte[] StringBytesOriginal = await response.Content.ReadAsByteArrayAsync();
                        dati = siteEncoding.GetString(StringBytesOriginal);

                        if (Debug) { LoggerDelegate("Used charset: " + siteEncoding.EncodingName); }
                        System.Diagnostics.Debug.WriteLine("Used charset: " + siteEncoding.EncodingName);
                    }
                    else
                    {
                        Encoding siteEncoding = Encoding.UTF8;
                        byte[] StringBytesOriginal = await response.Content.ReadAsByteArrayAsync();
                        dati = siteEncoding.GetString(StringBytesOriginal);

                        if (Debug) { LoggerDelegate("Used charset: " + siteEncoding.EncodingName); }
                        System.Diagnostics.Debug.WriteLine("Used charset: " + siteEncoding.EncodingName);
                    }
                }

                return dati;
            }
        }
    }
}