using ArnoldVinkCode;
using HtmlAgilityPack;
using NewsScroll.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace NewsScroll
{
    public partial class ItemPopup
    {
        private async Task GenerateImage(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                //Decode the image source link
                string sourceUri = string.Empty;
                if (htmlNode.Attributes["src"] != null)
                {
                    sourceUri = WebUtility.HtmlDecode(htmlNode.Attributes["src"].Value);
                    sourceUri = WebUtility.UrlDecode(sourceUri);
                }
                else if (htmlNode.Attributes["srcset"] != null)
                {
                    sourceUri = WebUtility.HtmlDecode(htmlNode.Attributes["srcset"].Value);
                    sourceUri = WebUtility.UrlDecode(sourceUri);
                }

                //Split an image srcset link
                Regex RegexSourceset = new Regex(@"(?:\s+\d+[wx])(?:,\s+)?");
                IEnumerable<string> ImageSources = RegexSourceset.Split(sourceUri).Where(x => x != string.Empty);
                if (ImageSources.Any()) { sourceUri = ImageSources.LastOrDefault(); }

                //Split http(s):// tags from uri
                if (sourceUri.Contains("https://") && sourceUri.LastIndexOf("https://") <= 20) { sourceUri = sourceUri.Substring(sourceUri.LastIndexOf("https://")); }
                if (sourceUri.Contains("http://") && sourceUri.LastIndexOf("http://") <= 20) { sourceUri = sourceUri.Substring(sourceUri.LastIndexOf("http://")); }

                //Check if image needs to be blocked
                if (string.IsNullOrWhiteSpace(sourceUri) || AppVariables.BlockedListUrl.Any(sourceUri.ToLower().Contains))
                {
                    System.Diagnostics.Debug.WriteLine("Blocked image: " + sourceUri);
                    return;
                }

                //Check if device is low on memory
                if (AVFunctions.DevMemoryAvailableMB() < 100)
                {
                    ImageContainer imgLowMemory = new ImageContainer();
                    imgLowMemory.item_status.Text = "Image not loaded,\ndevice is low on memory.";
                    imgLowMemory.IsHitTestVisible = false;

                    addElement.Children.Add(imgLowMemory);
                    //GenerateBreak(addElement);
                    return;
                }

                //Create item image
                System.Diagnostics.Debug.WriteLine("Adding image: " + sourceUri);
                ImageContainer imgContainer = new ImageContainer();
                imgContainer.MaxHeight = AppVariables.MaximumItemImageHeight;
                imgContainer.item_image_Value = sourceUri;

                //Get and set alt from the image
                if (vImageShowAlt && htmlNode.Attributes["alt"] != null)
                {
                    string AltText = Process.ProcessItemTextSummary(htmlNode.Attributes["alt"].Value, false, false);
                    if (!string.IsNullOrWhiteSpace(AltText))
                    {
                        imgContainer.item_description.Text = AltText;
                        imgContainer.item_description.Visibility = Visibility.Visible;
                    }
                }

                addElement.Children.Add(imgContainer);
                //GenerateBreak(addElement);
            }
            catch { }
        }

        private void GenerateVideo(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                //Check if media loading is allowed
                if (!AppVariables.LoadMedia)
                {
                    VideoContainer video = new VideoContainer();
                    video.item_status.Text = "Video not loaded,\nnetwork is not available.";

                    addElement.Children.Add(video);
                    //GenerateBreak(addElement);
                    return;
                }

                //Check if device is low on memory
                if (AVFunctions.DevMemoryAvailableMB() < 200)
                {
                    VideoContainer video = new VideoContainer();
                    video.item_status.Text = "Video not loaded,\ndevice is low on memory.";

                    addElement.Children.Add(video);
                    //GenerateBreak(addElement);
                    return;
                }

                //Check if low bandwidth mode is enabled
                if ((bool)AppVariables.ApplicationSettings["LowBandwidthMode"])
                {
                    VideoContainer video = new VideoContainer();
                    video.item_status.Text = "Video not loaded,\nlow bandwidth mode.";

                    addElement.Children.Add(video);
                    //GenerateBreak(addElement);
                    return;
                }

                //Create item video
                string VideoString = htmlNode.Attributes["src"].Value;
                if (!string.IsNullOrWhiteSpace(VideoString))
                {
                    System.Diagnostics.Debug.WriteLine("Opening video: " + VideoString);
                    VideoContainer video = new VideoContainer();

                    //Disable auto play
                    video.item_source.AutoPlay = false;

                    //Set video source
                    Uri videoUri = new Uri(VideoString, UriKind.RelativeOrAbsolute);
                    video.item_source.Source = MediaSource.CreateFromUri(videoUri);

                    addElement.Children.Add(video);
                    //GenerateBreak(addElement);
                }
            }
            catch { }
        }

        private void GenerateWebview(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                //Check if webview limit reached
                if (vWebViewAdded == vWebViewLimit)
                {
                    WebContainer webView = new WebContainer();
                    webView.item_status.Text = "Webview not loaded,\nlimit has been reached.";

                    addElement.Children.Add(webView);
                    //GenerateBreak(addElement);
                    return;
                }

                //Check if media loading is allowed
                if (!AppVariables.LoadMedia)
                {
                    WebContainer webView = new WebContainer();
                    webView.item_status.Text = "Webview not loaded,\nnetwork is not available.";

                    addElement.Children.Add(webView);
                    //GenerateBreak(addElement);
                    return;
                }

                //Check if device is low on memory
                if (AVFunctions.DevMemoryAvailableMB() < 200)
                {
                    WebContainer webView = new WebContainer();
                    webView.item_status.Text = "Webview not loaded,\ndevice is low on memory.";

                    addElement.Children.Add(webView);
                    //GenerateBreak(addElement);
                    return;
                }

                //Check if low bandwidth mode is enabled
                if ((bool)AppVariables.ApplicationSettings["LowBandwidthMode"])
                {
                    WebContainer webView = new WebContainer();
                    webView.item_status.Text = "Webview not loaded,\nlow bandwidth mode.";

                    addElement.Children.Add(webView);
                    //GenerateBreak(addElement);
                    return;
                }

                //Create item webview
                string WebLink = htmlNode.Attributes["src"].Value;
                if (!string.IsNullOrWhiteSpace(WebLink))
                {
                    System.Diagnostics.Debug.WriteLine("Opening webview: " + WebLink);

                    WebContainer webView = new WebContainer();
                    webView.item_source.Source = new Uri(WebLink);
                    webView.item_source.NewWindowRequested += webview_Full_NewWindowRequested;

                    addElement.Children.Add(webView);
                    //GenerateBreak(addElement);
                    System.Diagnostics.Debug.WriteLine("Added webview: " + WebLink);

                    //Update the webview count
                    vWebViewAdded++;
                }
            }
            catch { }
        }

        private async Task GenerateHyperLink(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                //Add other child node elements
                vIgnoreText = true;
                await AddNodes(addElement, htmlNode, false);
                vIgnoreText = false;

                //Get the text
                string StringText = Process.ProcessItemTextFull(htmlNode.InnerText, false, false, true);

                //Get the link
                string LinkUrl = string.Empty;
                if (htmlNode.Attributes["href"] != null)
                {
                    LinkUrl = htmlNode.Attributes["href"].Value;
                    LinkUrl = WebUtility.HtmlDecode(LinkUrl);
                    LinkUrl = WebUtility.UrlDecode(LinkUrl);
                }
                if (string.IsNullOrWhiteSpace(LinkUrl))
                {
                    LinkUrl = "#";
                }

                //Generate hyperlink
                if (!string.IsNullOrWhiteSpace(StringText) && !LinkUrl.StartsWith("javascript:"))
                {
                    TextBlock hyperLink = new TextBlock();
                    hyperLink.Text = StringText;
                    hyperLink.TextWrapping = TextWrapping.Wrap;
                    ToolTipService.SetToolTip(hyperLink, LinkUrl);

                    if (vTextDecorations != null) { hyperLink.TextDecorations = (TextDecorations)vTextDecorations; vTextDecorations = null; } else { hyperLink.TextDecorations = TextDecorations.Underline; }
                    if (vFontStyles != null) { hyperLink.FontStyle = (FontStyle)vFontStyles; vFontStyles = null; }
                    if (vFontWeight != null) { hyperLink.FontWeight = (FontWeight)vFontWeight; vFontWeight = null; }

                    Binding FontSizeBinding = new Binding();
                    FontSizeBinding.Source = (DynamicStyle)Application.Current.Resources["DynamicStyle"];
                    FontSizeBinding.Path = new PropertyPath("TextSizeMedium");
                    hyperLink.SetBinding(TextBlock.FontSizeProperty, FontSizeBinding);

                    Binding StyleBinding = new Binding();
                    StyleBinding.Source = Application.Current.Resources["TextBlockAccent"];
                    hyperLink.SetBinding(TextBlock.StyleProperty, StyleBinding);

                    hyperLink.Tapped += async delegate
                    {
                        Uri targetUri = new Uri(LinkUrl, UriKind.RelativeOrAbsolute);
                        await OpenBrowser(targetUri, false);
                    };

                    addElement.Children.Add(hyperLink);
                    System.Diagnostics.Debug.WriteLine("Added hyperLink: " + StringText);
                }
            }
            catch { }
        }

        private async Task GenerateBold(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                vFontWeight = FontWeights.Bold;
                await AddNodes(addElement, htmlNode, false);
            }
            catch { }
        }

        private async Task GenerateUnderline(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                if (vTextDecorations != null)
                {
                    vTextDecorations |= TextDecorations.Underline;
                }
                else
                {
                    vTextDecorations = TextDecorations.Underline;
                }
                await AddNodes(addElement, htmlNode, false);
            }
            catch { }
        }

        private async Task GenerateStrikethrough(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                if (vTextDecorations != null)
                {
                    vTextDecorations |= TextDecorations.Strikethrough;
                }
                else
                {
                    vTextDecorations = TextDecorations.Strikethrough;
                }
                await AddNodes(addElement, htmlNode, false);
            }
            catch { }
        }

        private async Task GenerateItalic(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                if (vFontStyles != null)
                {
                    vFontStyles |= FontStyle.Italic;
                }
                else
                {
                    vFontStyles = FontStyle.Italic;
                }
                await AddNodes(addElement, htmlNode, false);
            }
            catch { }
        }

        private void GenerateHeader(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                string stringText = Process.ProcessItemTextFull(htmlNode.InnerText, false, false, true);
                if (!string.IsNullOrWhiteSpace(stringText))
                {
                    TextBlock headerText = new TextBlock();
                    headerText.Text = stringText;
                    headerText.TextWrapping = TextWrapping.Wrap;

                    Binding StyleBinding = new Binding();
                    StyleBinding.Source = Application.Current.Resources["TextBlockAccent"];
                    headerText.SetBinding(TextBlock.StyleProperty, StyleBinding);

                    Binding FontSizeBinding = new Binding();
                    FontSizeBinding.Source = (DynamicStyle)Application.Current.Resources["DynamicStyle"];
                    FontSizeBinding.Path = new PropertyPath("TextSizeLarge");
                    headerText.SetBinding(TextBlock.FontSizeProperty, FontSizeBinding);

                    addElement.Children.Add(headerText);
                }
            }
            catch { }
        }

        private void GenerateGridText(StackPanel addElement, HtmlNode htmlNode, string textHeader, bool textRaw, bool textHtml, TextAlignment textAlignment, HorizontalAlignment horizontalAlignment)
        {
            try
            {
                //Grid Stackpanel
                StackPanel stackPanelGrid = new StackPanel();

                //Grid Header
                if (!string.IsNullOrWhiteSpace(textHeader))
                {
                    TextBlock TextBlockHeader = new TextBlock();
                    TextBlockHeader.Text = textHeader + ":";
                    TextBlockHeader.TextWrapping = TextWrapping.Wrap;
                    TextBlockHeader.HorizontalAlignment = HorizontalAlignment.Left;
                    TextBlockHeader.Foreground = new SolidColorBrush((Color)Application.Current.Resources["ApplicationAccentLightColor"]);

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(TextBlockHeader);
                }

                //Grid Text
                string StringText = string.Empty;
                if (textHtml)
                {
                    StringText = Process.ProcessItemTextFull(htmlNode.InnerHtml, false, false, true);
                }
                else
                {
                    StringText = Process.ProcessItemTextFull(htmlNode.InnerText, false, false, true);
                }
                if (!string.IsNullOrWhiteSpace(StringText))
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = StringText;
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.TextAlignment = textAlignment;
                    textBlock.HorizontalAlignment = horizontalAlignment;

                    //StackPanel Content
                    StackPanel stackPanelContent = new StackPanel();
                    stackPanelContent.Background = new SolidColorBrush((Color)Application.Current.Resources["ApplicationDarkGrayTransparentColor"]);
                    stackPanelContent.Children.Add(textBlock);

                    //Add to stackpanel grid
                    stackPanelGrid.Children.Add(stackPanelContent);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No text found to add to grid text.");
                    return;
                }

                addElement.Children.Add(stackPanelGrid);
            }
            catch { }
        }

        private async Task GenerateGridContent(StackPanel addElement, HtmlNode htmlNode, string textHeader)
        {
            try
            {
                //Check if node contains childs
                if (!htmlNode.ChildNodes.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No child nodes found to add to grid content.");
                    return;
                }

                //Grid Stackpanel
                StackPanel stackPanelGrid = new StackPanel();

                //Grid Header
                if (!string.IsNullOrWhiteSpace(textHeader))
                {
                    TextBlock TextBlockHeader = new TextBlock();
                    TextBlockHeader.Text = textHeader + ":";
                    TextBlockHeader.TextWrapping = TextWrapping.Wrap;
                    TextBlockHeader.HorizontalAlignment = HorizontalAlignment.Left;
                    TextBlockHeader.Foreground = new SolidColorBrush((Color)Application.Current.Resources["ApplicationAccentLightColor"]);

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(TextBlockHeader);
                }

                //StackPanel Content
                StackPanel stackPanelContent = new StackPanel();
                stackPanelContent.Background = new SolidColorBrush((Color)Application.Current.Resources["ApplicationDarkGrayTransparentColor"]);
                await AddNodes(stackPanelContent, htmlNode, false);

                //Add to stackpanel grid
                stackPanelGrid.Children.Add(stackPanelContent);

                addElement.Children.Add(stackPanelGrid);
            }
            catch { }
        }

        private async Task GenerateTable(StackPanel addElement, HtmlNode htmlNode, string textHeader)
        {
            try
            {
                //Check if node contains childs
                if (!htmlNode.ChildNodes.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No child nodes found to add to table.");
                    return;
                }

                //Set image settings
                vImageShowAlt = false;

                //Grid Stackpanel
                StackPanel stackPanelGrid = new StackPanel();

                //Grid Header
                if (!string.IsNullOrWhiteSpace(textHeader))
                {
                    TextBlock TextBlockHeader = new TextBlock();
                    TextBlockHeader.Text = textHeader + ":";
                    TextBlockHeader.TextWrapping = TextWrapping.Wrap;
                    TextBlockHeader.HorizontalAlignment = HorizontalAlignment.Left;
                    TextBlockHeader.Foreground = new SolidColorBrush((Color)Application.Current.Resources["ApplicationAccentLightColor"]);

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(TextBlockHeader);
                }

                int RowCurrentCount = 0;
                int RowTotalCount = htmlNode.Descendants("tr").Count();
                if (RowTotalCount > 0)
                {
                    //Add table child node elements
                    Grid gridContent = new Grid();
                    gridContent.Background = new SolidColorBrush((Color)Application.Current.Resources["ApplicationDarkGrayTransparentColor"]);

                    foreach (HtmlNode TableRow in htmlNode.Descendants("tr"))
                    {
                        int ColumnCurrentCount = 0;

                        //Create and add row
                        RowDefinition gridRowDefinition = new RowDefinition();
                        gridContent.RowDefinitions.Add(gridRowDefinition);

                        Grid gridRow = new Grid();
                        gridContent.Children.Add(gridRow);
                        Grid.SetRow(gridRow, RowCurrentCount);
                        RowCurrentCount++;

                        //Set grid row style
                        if (RowCurrentCount != RowTotalCount)
                        {
                            gridRow.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["ApplicationAccentLightColor"]) { Opacity = 0.60 };
                            gridRow.BorderThickness = new Thickness(0, 0, 0, 2);
                        }

                        //Table Header
                        foreach (HtmlNode TableHeader in TableRow.Descendants("th"))
                        {
                            ColumnDefinition gridColDefinition = new ColumnDefinition();
                            gridRow.ColumnDefinitions.Add(gridColDefinition);

                            TextBlock textBlock = new TextBlock();
                            textBlock.Foreground = new SolidColorBrush((Color)Application.Current.Resources["ApplicationAccentLightColor"]);
                            textBlock.Text = TableHeader.InnerText;
                            textBlock.TextWrapping = TextWrapping.Wrap;

                            gridRow.Children.Add(textBlock);
                            Grid.SetColumn(textBlock, ColumnCurrentCount);
                            ColumnCurrentCount++;
                        }

                        //Table Column
                        foreach (HtmlNode TableColumn in TableRow.Descendants("td"))
                        {
                            ColumnDefinition gridColDefinition = new ColumnDefinition();
                            gridRow.ColumnDefinitions.Add(gridColDefinition);

                            //Add other child node elements
                            StackPanel spanContent = new StackPanel();
                            await AddNodes(spanContent, TableColumn, false);

                            gridRow.Children.Add(spanContent);
                            Grid.SetColumn(spanContent, ColumnCurrentCount);
                            ColumnCurrentCount++;
                        }
                    }

                    //Add to stackpanel grid
                    stackPanelGrid.Children.Add(gridContent);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No row found in the table.");

                    //Add other child node elements
                    StackPanel spanContent = new StackPanel();
                    spanContent.Background = new SolidColorBrush((Color)Application.Current.Resources["ApplicationDarkGrayTransparentColor"]);
                    await AddNodes(spanContent, htmlNode, false);

                    //Add to stackpanel grid
                    stackPanelGrid.Children.Add(spanContent);
                }

                addElement.Children.Add(stackPanelGrid);

                //Set image settings
                vImageShowAlt = true;
            }
            catch { }
        }

        private async Task GenerateUl(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                await AddNodes(addElement, htmlNode, false);
                //GenerateBreak(addElement);
            }
            catch { }
        }

        private void GenerateLi(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                string liText = htmlNode.InnerText;
                if (!string.IsNullOrWhiteSpace(liText))
                {
                    TextBlock textLabel = new TextBlock();
                    textLabel.TextWrapping = TextWrapping.Wrap;
                    textLabel.Inlines.Add(new Run() { Text = "* ", Foreground = new SolidColorBrush((Color)Application.Current.Resources["ApplicationAccentLightColor"]) });
                    textLabel.Inlines.Add(new Run() { Text = liText });

                    addElement.Children.Add(textLabel);
                    System.Diagnostics.Debug.WriteLine("Added li: " + liText);
                }
            }
            catch { }
        }

        private void GenerateBreak(StackPanel addElement)
        {
            try
            {
                Grid addGrid = new Grid();
                addGrid.Height = 15;
                addGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                addGrid.Background = new SolidColorBrush(Colors.Transparent);
                addElement.Children.Add(addGrid);
            }
            catch { }
        }

        private async Task GenerateParagraph(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                await AddNodes(addElement, htmlNode, false);
                //GenerateBreak(addElement);
                //GenerateBreak(addElement);
            }
            catch { }
        }

        private void GenerateText(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                string stringText = Process.ProcessItemTextFull(htmlNode.InnerText, true, false, true);
                if (!string.IsNullOrWhiteSpace(stringText))
                {
                    TextBlock textLabel = new TextBlock();
                    textLabel.Text = stringText;
                    textLabel.TextWrapping = TextWrapping.Wrap;

                    if (vTextDecorations != null) { textLabel.TextDecorations = (TextDecorations)vTextDecorations; vTextDecorations = null; }
                    if (vFontStyles != null) { textLabel.FontStyle = (FontStyle)vFontStyles; vFontStyles = null; }
                    if (vFontWeight != null) { textLabel.FontWeight = (FontWeight)vFontWeight; vFontWeight = null; }

                    Binding FontSizeBinding = new Binding();
                    FontSizeBinding.Source = (DynamicStyle)Application.Current.Resources["DynamicStyle"];
                    FontSizeBinding.Path = new PropertyPath("TextSizeMedium");
                    textLabel.SetBinding(TextBlock.FontSizeProperty, FontSizeBinding);

                    Binding StyleBinding = new Binding();
                    StyleBinding.Source = Application.Current.Resources["TextBlockBlack"];
                    textLabel.SetBinding(TextBlock.StyleProperty, StyleBinding);

                    addElement.Children.Add(textLabel);
                    System.Diagnostics.Debug.WriteLine("Added text: " + stringText);
                }
            }
            catch { }
        }

        private async Task GenerateSpan(StackPanel addElement, HtmlNode htmlNode)
        {
            try
            {
                //Check if span contains iframe
                HtmlAttribute iframeNode = htmlNode.Attributes.Where(x => x.Name.Contains("iframe")).FirstOrDefault();
                if (iframeNode != null)
                {
                    System.Diagnostics.Debug.WriteLine("Adding span node with iframe.");
                    WebContainer webView = new WebContainer();
                    webView.item_status.Text = "Webview not loaded,\nopen in browser.";

                    addElement.Children.Add(webView);
                    //GenerateBreak(addElement);
                }
                else
                {
                    await AddNodes(addElement, htmlNode, false);
                }
            }
            catch { }
        }
    }
}