using HtmlAgilityPack;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NewsScroll
{
    public partial class ItemPopup
    {
        private async Task AddNodes(StackPanel addElement, HtmlNode htmlNode, bool FirstOnly)
        {
            try
            {
                foreach (HtmlNode childNode in htmlNode.ChildNodes)
                {
                    System.Diagnostics.Debug.WriteLine("Adding node: " + childNode.Name);
                    await AddBlockFromNode(addElement, childNode);
                    if (FirstOnly) { break; }
                }
            }
            catch { }
        }

        private async Task AddBlockFromNode(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                switch (htmlNode.Name.ToLower())
                {
                    //Text
                    case "#text": { if (!vIgnoreText) { GenerateText(addElement, htmlNode); } return; }

                    //Text Grids
                    case "code": { GenerateGridText(addElement, htmlNode, "Code", false, true, Windows.UI.Xaml.TextAlignment.Left, HorizontalAlignment.Stretch); return; }
                    case "cite": { GenerateGridText(addElement, htmlNode, string.Empty, true, false, Windows.UI.Xaml.TextAlignment.Center, HorizontalAlignment.Stretch); return; }
                    case "figcaption": { GenerateGridText(addElement, htmlNode, string.Empty, true, false, Windows.UI.Xaml.TextAlignment.Center, HorizontalAlignment.Stretch); return; }

                    //Text Styles
                    case "strong": { await GenerateBold(addElement, htmlNode); return; }
                    case "bold": { await GenerateBold(addElement, htmlNode); return; }
                    case "b": { await GenerateBold(addElement, htmlNode); return; }
                    case "em": { await GenerateItalic(addElement, htmlNode); return; }
                    case "i": { await GenerateItalic(addElement, htmlNode); return; }
                    case "ins": { await GenerateUnderline(addElement, htmlNode); return; }
                    case "u": { await GenerateUnderline(addElement, htmlNode); return; }
                    case "strike": { await GenerateStrikethrough(addElement, htmlNode); return; }
                    case "del": { await GenerateStrikethrough(addElement, htmlNode); return; }
                    case "h1": { GenerateHeader(addElement, htmlNode); return; }
                    case "h2": { GenerateHeader(addElement, htmlNode); return; }
                    case "h3": { GenerateHeader(addElement, htmlNode); return; }
                    case "h4": { GenerateHeader(addElement, htmlNode); return; }
                    case "h5": { GenerateHeader(addElement, htmlNode); return; }

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
                    case "section": { await GenerateGridContent(addElement, htmlNode, string.Empty); return; }

                    //Break
                    case "br": { GenerateBreak(addElement); return; }

                    //Default
                    default: { await AddNodes(addElement, htmlNode, false); return; }
                }
            }
            catch { }
        }
    }
}