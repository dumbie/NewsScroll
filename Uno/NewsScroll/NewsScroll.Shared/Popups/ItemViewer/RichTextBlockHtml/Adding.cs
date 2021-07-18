using HtmlAgilityPack;
using System.Threading.Tasks;
using Windows.UI.Xaml.Documents;

namespace NewsScroll
{
    public partial class ItemViewer
    {
        private async Task AddNodes(Span addSpan, HtmlNode htmlNode, bool FirstOnly)
        {
            try
            {
                foreach (HtmlNode childNode in htmlNode.ChildNodes)
                {
                    System.Diagnostics.Debug.WriteLine("Adding node: " + childNode.Name);
                    await AddBlockFromNode(addSpan, childNode);
                    if (FirstOnly) { break; }
                }
            }
            catch { }
        }

        private Span AddSpacing(Span addSpan)
        {
            try
            {
                Span spacingSpan = new Span();
                spacingSpan.Inlines.Add(new Run { Text = " " });
                spacingSpan.Inlines.Add(addSpan);
                spacingSpan.Inlines.Add(new Run { Text = " " });
                return spacingSpan;
            }
            catch { }
            return addSpan;
        }

        private async Task AddBlockFromNode(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                switch (htmlNode.Name.ToLower())
                {
                    //Text
                    case "#text": { if (!vIgnoreText) { GenerateText(addSpan, htmlNode); } return; }

                    //Text Grids
                    //fix
                    //case "code": { GenerateGridText(addSpan, htmlNode, "Code", false, true, TextAlignment.Left, HorizontalAlignment.Stretch); return; }
                    //case "cite": { GenerateGridText(addSpan, htmlNode, String.Empty, true, false, TextAlignment.Center, HorizontalAlignment.Stretch); return; }
                    //case "figcaption": { GenerateGridText(addSpan, htmlNode, String.Empty, true, false, TextAlignment.Center, HorizontalAlignment.Stretch); return; }

                    //Text Styles
                    case "strong": { await GenerateBold(addSpan, htmlNode); return; }
                    case "b": { await GenerateBold(addSpan, htmlNode); return; }
                    case "em": { await GenerateItalic(addSpan, htmlNode); return; }
                    case "i": { await GenerateItalic(addSpan, htmlNode); return; }
                    case "u": { await GenerateUnderline(addSpan, htmlNode); return; }
                    case "h1": { GenerateHeader(addSpan, htmlNode); return; }
                    case "h2": { GenerateHeader(addSpan, htmlNode); return; }
                    case "h3": { GenerateHeader(addSpan, htmlNode); return; }
                    case "h4": { GenerateHeader(addSpan, htmlNode); return; }
                    case "h5": { GenerateHeader(addSpan, htmlNode); return; }
                    case "br": { GenerateBreak(addSpan); return; }

                    //Elements
                    case "img": { await GenerateImage(addSpan, htmlNode); return; }
                    case "picture": { await GenerateImage(addSpan, htmlNode); return; }
                    case "figure": { await GenerateImage(addSpan, htmlNode); return; }
                    case "video": { GenerateVideo(addSpan, htmlNode); return; }
                    case "iframe": { GenerateWebview(addSpan, htmlNode); return; }

                    //Table
                    case "table": { await GenerateTable(addSpan, htmlNode, "Table"); return; }

                    //Menu
                    case "ul": { await GenerateUl(addSpan, htmlNode); return; }
                    case "li": { await GenerateLi(addSpan, htmlNode); return; }

                    //Containers
                    case "p": { await GenerateParagraph(addSpan, htmlNode); return; }
                    case "a": { await GenerateHyperLink(addSpan, htmlNode); return; }
                    case "span": { await GenerateSpan(addSpan, htmlNode); return; }

                    //Containers Grid
                    case "blockquote": { await GenerateGridContent(addSpan, htmlNode, "Quote"); return; }

                    //Default
                    default: { await AddNodes(addSpan, htmlNode, false); return; }
                }
            }
            catch { }
        }
    }
}