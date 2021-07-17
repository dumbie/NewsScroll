using HtmlAgilityPack;
using System.Diagnostics;
using Xamarin.Forms;

namespace NewsScroll
{
    public partial class ItemPopup
    {
        private void AddNodes(StackLayout addElement, HtmlNode htmlNode, bool firstOnly)
        {
            try
            {
                foreach (HtmlNode childNode in htmlNode.ChildNodes)
                {
                    Debug.WriteLine("Adding node: " + childNode.Name);
                    AddBlockFromNode(addElement, childNode);
                    if (firstOnly) { break; }
                }
            }
            catch { }
        }

        private void AddBlockFromNode(StackLayout addElement, HtmlNode htmlNode)
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
                    case "img": { GenerateImage(addElement, htmlNode); return; }
                    case "picture": { GenerateImage(addElement, htmlNode); return; }
                    case "figure": { GenerateImage(addElement, htmlNode); return; }
                    case "video": { GenerateVideo(addElement, htmlNode); return; }
                    case "iframe": { GenerateWebview(addElement, htmlNode); return; }

                    //Table
                    case "table": { GenerateTable(addElement, htmlNode, "Table"); return; }

                    //Menu
                    case "ul": { GenerateUl(addElement, htmlNode); return; }
                    case "li": { GenerateLi(addElement, htmlNode); return; }

                    //Containers
                    case "p": { GenerateParagraph(addElement, htmlNode); return; }
                    case "a": { GenerateHyperLink(addElement, htmlNode); return; }
                    case "span": { GenerateSpan(addElement, htmlNode); return; }

                    //Containers Grid
                    case "q": { GenerateGridContent(addElement, htmlNode, "Quote"); return; }
                    case "blockquote": { GenerateGridContent(addElement, htmlNode, "Quote"); return; }
                    case "section": { GenerateGridContent(addElement, htmlNode, string.Empty); return; }

                    //Default
                    default: { AddNodes(addElement, htmlNode, false); return; }
                }
            }
            catch { }
        }
    }
}