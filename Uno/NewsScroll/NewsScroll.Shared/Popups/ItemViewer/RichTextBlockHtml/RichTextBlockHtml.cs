using ArnoldVinkCode;
using HtmlAgilityPack;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace NewsScroll
{
    public partial class ItemViewer
    {
        //Rich text block html variables
        private bool vImageShowAlt = true;
        private bool vIgnoreText = false;
        private WebView vWindowWebview = null;
        private int vWebViewLimit = 3;
        private int vWebViewAdded = 0;

        private async Task<bool> HtmlToRichTextBlock(RichTextBlock TargetRichTextBlock, string FullHtml, string BaseLink)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Converting: " + FullHtml);

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

                //Set the generated blocks to the rich text block
                Span addSpan = new Span();
                await AddNodes(addSpan, htmlDocument.DocumentNode, false);
                if (addSpan.Inlines.Any())
                {
                    Paragraph paragraph = new Paragraph();
                    paragraph.Inlines.Add(addSpan);

                    TargetRichTextBlock.Blocks.Clear();
                    TargetRichTextBlock.Blocks.Add(paragraph);
                }

                System.Diagnostics.Debug.WriteLine("Converted html to rich text block.");
                return true;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to convert html to rich text block.");
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
                    if (Image.Attributes["src"] != null)
                    {
                        string CurrentLink = Image.Attributes["src"].Value;
                        if (!AppVariables.BlockedListUrl.Any(CurrentLink.Contains))
                        {
                            if (CurrentLink.StartsWith("//")) { Image.Attributes["src"].Value = "http:" + CurrentLink; }
                            else if (CurrentLink.StartsWith("/")) { Image.Attributes["src"].Value = BaseLink + CurrentLink; }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Blocked image: " + CurrentLink);
                            Image.RemoveAll();
                        }
                    }
                }

                //Check for invalid video links
                System.Diagnostics.Debug.WriteLine("Checking video links...");
                foreach (HtmlNode Video in htmlDocument.DocumentNode.Descendants("video"))
                {
                    if (Video.Attributes["src"] != null)
                    {
                        string CurrentLink = Video.Attributes["src"].Value;
                        if (!CurrentLink.StartsWith("http"))
                        {
                            if (CurrentLink.StartsWith("//")) { Video.Attributes["src"].Value = "http:" + CurrentLink; }
                            else { Video.Attributes["src"].Value = BaseLink + CurrentLink; }
                        }
                    }
                }

                //Check for invalid iframe links
                System.Diagnostics.Debug.WriteLine("Checking iframe links...");
                foreach (HtmlNode Iframe in htmlDocument.DocumentNode.Descendants("iframe"))
                {
                    if (Iframe.Attributes["src"] != null)
                    {
                        string CurrentLink = Iframe.Attributes["src"].Value;
                        if (!CurrentLink.StartsWith("http"))
                        {
                            if (CurrentLink.StartsWith("//")) { Iframe.Attributes["src"].Value = "http:" + CurrentLink; }
                            else { Iframe.Attributes["src"].Value = BaseLink + CurrentLink; }
                        }
                    }
                }

                //Check for invalid hyper links
                System.Diagnostics.Debug.WriteLine("Checking hyper links...");
                foreach (HtmlNode Link in htmlDocument.DocumentNode.Descendants("a"))
                {
                    if (Link.Attributes["href"] != null)
                    {
                        string CurrentLink = Link.Attributes["href"].Value;
                        if (!CurrentLink.StartsWith("http"))
                        {
                            if (CurrentLink.StartsWith("//")) { Link.Attributes["href"].Value = "http:" + CurrentLink; }
                            else { Link.Attributes["src"].Value = BaseLink + CurrentLink; }
                        }
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to validate html link: " + BaseLink);
            }
        }
    }
}