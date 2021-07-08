using HtmlAgilityPack;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NewsScroll
{
    public partial class ItemViewer
    {
        private async Task AddNodes(StackLayout addElement, HtmlNode htmlNode, bool firstOnly)
        {
            try
            {
                foreach (HtmlNode childNode in htmlNode.ChildNodes)
                {
                    Debug.WriteLine("Adding node: " + childNode.Name);
                    await AddBlockFromNode(addElement, childNode);
                    if (firstOnly) { break; }
                }
            }
            catch { }
        }

        private async Task AddBlockFromNode(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                switch (htmlNode.Name.ToLower())
                {
                    //Text
                    case "#text": { if (!vIgnoreText) { GenerateText(addElement, htmlNode); } return; }

                    //Text Grids
                    case "code": { GenerateGridText(addElement, htmlNode, "Code", true, TextAlignment.Start, LayoutOptions.Fill); return; }
                    case "cite": { GenerateGridText(addElement, htmlNode, string.Empty, false, TextAlignment.Center, LayoutOptions.Fill); return; }
                    case "figcaption": { GenerateGridText(addElement, htmlNode, string.Empty, false, TextAlignment.Center, LayoutOptions.Fill); return; }

                    //Text Styles
                    case "strong": { GenerateBold(addElement, htmlNode); return; }
                    case "bold": { GenerateBold(addElement, htmlNode); return; }
                    case "b": { GenerateBold(addElement, htmlNode); return; }
                    case "em": { GenerateItalic(addElement, htmlNode); return; }
                    case "i": { GenerateItalic(addElement, htmlNode); return; }
                    case "ins": { GenerateUnderline(addElement, htmlNode); return; }
                    case "u": { GenerateUnderline(addElement, htmlNode); return; }
                    case "strike": { GenerateStrikethrough(addElement, htmlNode); return; }
                    case "del": { GenerateStrikethrough(addElement, htmlNode); return; }
                    case "h1": { GenerateHeader(addElement, htmlNode); return; }
                    case "h2": { GenerateHeader(addElement, htmlNode); return; }
                    case "h3": { GenerateHeader(addElement, htmlNode); return; }
                    case "h4": { GenerateHeader(addElement, htmlNode); return; }
                    case "h5": { GenerateHeader(addElement, htmlNode); return; }
                    case "br": { GenerateBreak(addElement); return; }

                    //Elements
                    case "img": { await GenerateImage(addElement, htmlNode); return; }
                    case "picture": { await GenerateImage(addElement, htmlNode); return; }
                    case "figure": { await GenerateImage(addElement, htmlNode); return; }
                    case "video": { GenerateVideo(addElement, htmlNode); return; }
                    case "iframe": { GenerateWebview(addElement, htmlNode); return; }

                    //Table
                    case "table": { await GenerateTable(addElement, htmlNode, "Table"); return; }

                    //Menu
                    case "ul": { await GenerateUl(addElement, htmlNode); return; }
                    case "li": { GenerateLi(addElement, htmlNode); return; }

                    //Containers
                    case "p": { await GenerateParagraph(addElement, htmlNode); return; }
                    case "a": { await GenerateHyperLink(addElement, htmlNode); return; }
                    case "span": { await GenerateSpan(addElement, htmlNode); return; }

                    //Containers Grid
                    case "q": { await GenerateGridContent(addElement, htmlNode, "Quote"); return; }
                    case "blockquote": { await GenerateGridContent(addElement, htmlNode, "Quote"); return; }

                    //Default
                    default: { await AddNodes(addElement, htmlNode, false); return; }
                }
            }
            catch { }
        }
    }
}