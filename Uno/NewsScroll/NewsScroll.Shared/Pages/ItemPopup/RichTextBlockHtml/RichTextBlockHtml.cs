using ArnoldVinkCode;
using HtmlAgilityPack;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;

namespace NewsScroll
{
    public partial class ItemPopup
    {
        //HTML XAML variables
        private FontStyle? vFontStyles = null;
        private FontWeight? vFontWeight = null;
        private TextDecorations? vTextDecorations = null;
        private bool vImageShowAlt = true;
        private bool vIgnoreText = false;
        private WebView vWindowWebview = null;
        private int vWebViewLimit = 3;
        private int vWebViewAdded = 0;

        private async Task<bool> HtmlToXaml(StackPanel targetElement, string FullHtml, string BaseLink)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Converting HTML to XAML: " + FullHtml);

                //Set the webview limit based on device
                if (AVFunctions.DevMobile())
                {
                    vWebViewLimit = 3;
                }
                else
                {
                    vWebViewLimit = 6;
                }

                //Load and parse the html document
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(FullHtml);

                //Check for invalid links
                CheckInvalidLinks(htmlDocument, BaseLink);

                //Add the generated blocks to the stackpanel
                targetElement.Children.Clear();
                await AddNodes(targetElement, htmlDocument.DocumentNode, false);

                System.Diagnostics.Debug.WriteLine("Converted html to xaml.");
                return true;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to convert html to xaml.");
                return false;
            }
        }

        //Check for invalid links
        private static void CheckInvalidLinks(HtmlDocument htmlDocument, string BaseLink)
        {
            try
            {
                //Check for invalid image links
                System.Diagnostics.Debug.WriteLine("Checking image links...");
                foreach (HtmlNode Image in htmlDocument.DocumentNode.Descendants("img"))
                {
                    try
                    {
                        if (Image.Attributes["src"] != null)
                        {
                            string CurrentLink = Image.Attributes["src"].Value;
                            if (!AppVariables.BlockedListUrl.Any(CurrentLink.Contains))
                            {
                                if (CurrentLink.StartsWith("//"))
                                {
                                    Image.Attributes["src"].Value = "http:" + CurrentLink;
                                }
                                else if (CurrentLink.StartsWith("/"))
                                {
                                    Image.Attributes["src"].Value = BaseLink + CurrentLink;
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Blocked image: " + CurrentLink);
                                Image.RemoveAll();
                            }
                        }
                    }
                    catch { }
                }

                //Check for invalid video links
                System.Diagnostics.Debug.WriteLine("Checking video links...");
                foreach (HtmlNode Video in htmlDocument.DocumentNode.Descendants("video"))
                {
                    try
                    {
                        if (Video.Attributes["src"] != null)
                        {
                            string CurrentLink = Video.Attributes["src"].Value;
                            if (!CurrentLink.StartsWith("http"))
                            {
                                if (CurrentLink.StartsWith("//"))
                                {
                                    Video.Attributes["src"].Value = "http:" + CurrentLink;
                                }
                                else
                                {
                                    Video.Attributes["src"].Value = BaseLink + CurrentLink;
                                }
                            }
                        }
                    }
                    catch { }
                }

                //Check for invalid iframe links
                System.Diagnostics.Debug.WriteLine("Checking iframe links...");
                foreach (HtmlNode Iframe in htmlDocument.DocumentNode.Descendants("iframe"))
                {
                    try
                    {
                        if (Iframe.Attributes["src"] != null)
                        {
                            string CurrentLink = Iframe.Attributes["src"].Value;
                            if (!CurrentLink.StartsWith("http"))
                            {
                                if (CurrentLink.StartsWith("//"))
                                {
                                    Iframe.Attributes["src"].Value = "http:" + CurrentLink;
                                }
                                else
                                {
                                    Iframe.Attributes["src"].Value = BaseLink + CurrentLink;
                                }
                            }
                        }
                    }
                    catch { }
                }

                //Check for invalid hyper links
                System.Diagnostics.Debug.WriteLine("Checking hyper links...");
                foreach (HtmlNode Link in htmlDocument.DocumentNode.Descendants("a"))
                {
                    try
                    {
                        if (Link.Attributes["href"] != null)
                        {
                            string CurrentLink = Link.Attributes["href"].Value;
                            if (!CurrentLink.StartsWith("http"))
                            {
                                if (CurrentLink.StartsWith("//"))
                                {
                                    Link.Attributes["href"].Value = "http:" + CurrentLink;
                                }
                                else
                                {
                                    Link.Attributes["src"].Value = BaseLink + CurrentLink;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to validate html link: " + BaseLink);
            }
        }
    }
}