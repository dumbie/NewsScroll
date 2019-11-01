using ArnoldVinkCode;
using HtmlAgilityPack;
using NewsScroll.MainElements;
using NewsScroll.Styles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NewsScroll
{
    public partial class ItemViewer
    {
        private async Task GenerateImage(Span addSpan, HtmlNode htmlNode)
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
                IEnumerable<String> ImageSources = RegexSourceset.Split(sourceUri).Where(x => x != string.Empty);
                if (ImageSources.Any()) { sourceUri = ImageSources.LastOrDefault(); }

                //Split http(s):// tags from uri
                if (sourceUri.Contains("https://") && sourceUri.LastIndexOf("https://") <= 20) { sourceUri = sourceUri.Substring(sourceUri.LastIndexOf("https://")); }
                if (sourceUri.Contains("http://") && sourceUri.LastIndexOf("http://") <= 20) { sourceUri = sourceUri.Substring(sourceUri.LastIndexOf("http://")); }

                //Check if image needs to be blocked
                if (String.IsNullOrWhiteSpace(sourceUri) || AppVariables.BlockedListUrl.Any(sourceUri.ToLower().Contains))
                {
                    Debug.WriteLine("Blocked image: " + sourceUri);
                    return;
                }

                //Check if device is low on memory
                if (AVFunctions.DevAvailableMemory() < 50)
                {
                    grid_item_image img = new grid_item_image();
                    img.item_status.Text = "Image not loaded,\ndevice is low on memory.";
                    img.IsHitTestVisible = false;

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = img;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                    return;
                }

                //Check if media is a gif(v) file
                bool ImageIsGif = sourceUri.ToLower().Contains(".gif");
                bool ImageIsSvg = sourceUri.ToLower().Contains(".svg");

                //Check if low bandwidth mode is enabled
                if (ImageIsGif && (bool)AppVariables.ApplicationSettings["LowBandwidthMode"])
                {
                    grid_item_image img = new grid_item_image();
                    img.item_status.Text = "Gif not loaded,\nlow bandwidth mode.";
                    img.IsHitTestVisible = false;

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = img;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                    return;
                }

                //Create item image
                Debug.WriteLine("Adding image: " + sourceUri);
                SvgImageSource SvgImage = null;
                BitmapImage BitmapImage = null;
                if (ImageIsSvg)
                {
                    SvgImage = await AVImage.LoadSvgImage(sourceUri);
                }
                else
                {
                    BitmapImage = await AVImage.LoadBitmapImage(sourceUri, true);
                }

                if (SvgImage != null || BitmapImage != null)
                {
                    grid_item_image img = new grid_item_image();
                    img.MaxHeight = AppVariables.MaximumItemImageHeight;

                    if (SvgImage != null)
                    {
                        img.item_source.Source = SvgImage;
                    }

                    if (BitmapImage != null)
                    {
                        img.value_item_image = BitmapImage;
                    }

                    //Get and set alt from the image
                    if (vImageShowAlt && htmlNode.Attributes["alt"] != null)
                    {
                        string AltText = Process.ProcessItemTextSummary(htmlNode.Attributes["alt"].Value, false, false);
                        if (!String.IsNullOrWhiteSpace(AltText))
                        {
                            img.item_description.Text = AltText;
                            img.item_description.Visibility = Visibility.Visible;

                            //Enable or disable text selection
                            if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                            {
                                img.item_description.IsTextSelectionEnabled = true;
                            }
                            else
                            {
                                img.item_description.IsTextSelectionEnabled = false;
                            }
                        }
                    }

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = img;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                }
                else
                {
                    grid_item_image img = new grid_item_image();
                    img.item_status.Text = "Image is not available,\nopen item in browser to view it.";
                    img.IsHitTestVisible = false;

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = img;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                    return;
                }
            }
            catch { }
        }

        private void GenerateVideo(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                //Check if media loading is allowed
                if (!AppVariables.LoadMedia)
                {
                    grid_item_video video = new grid_item_video();
                    video.item_status.Text = "Video not loaded,\nnetwork is not available.";

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = video;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                    return;
                }

                //Check if device is low on memory
                if (AVFunctions.DevAvailableMemory() < 200)
                {
                    grid_item_video video = new grid_item_video();
                    video.item_status.Text = "Video not loaded,\ndevice is low on memory.";

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = video;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                    return;
                }

                //Check if low bandwidth mode is enabled
                if ((bool)AppVariables.ApplicationSettings["LowBandwidthMode"])
                {
                    grid_item_image img = new grid_item_image();
                    img.item_status.Text = "Video not loaded,\nlow bandwidth mode.";
                    img.IsHitTestVisible = false;

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = img;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                    return;
                }

                //Create item video
                string VideoString = htmlNode.Attributes["src"].Value;
                if (!String.IsNullOrWhiteSpace(VideoString))
                {
                    Debug.WriteLine("Opening video: " + VideoString);

                    grid_item_video video = new grid_item_video();
                    video.item_source.Source = new Uri(VideoString);

                    //Check if media is a gif(v) file
                    if (VideoString.ToLower().Contains(".gif"))
                    {
                        video.item_source.AutoPlay = true;
                        video.item_source.MediaEnded += delegate
                        {
                            video.item_source.Position = new TimeSpan();
                            video.item_source.Play();
                        };
                    }
                    else
                    {
                        video.item_source.AutoPlay = false;
                    }

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = video;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                }
            }
            catch { }
        }

        private void GenerateWebview(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                //Check if media loading is allowed
                if (!AppVariables.LoadMedia)
                {
                    grid_item_webview webView = new grid_item_webview();
                    webView.item_status.Text = "Webview not loaded,\nnetwork is not available.";

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = webView;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                    return;
                }

                //Check if device is low on memory
                if (AVFunctions.DevAvailableMemory() < 200)
                {
                    grid_item_webview webView = new grid_item_webview();
                    webView.item_status.Text = "Webview not loaded,\ndevice is low on memory.";

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = webView;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                    return;
                }

                //Check if low bandwidth mode is enabled
                if ((bool)AppVariables.ApplicationSettings["LowBandwidthMode"])
                {
                    grid_item_image img = new grid_item_image();
                    img.item_status.Text = "Webview not loaded,\nlow bandwidth mode.";
                    img.IsHitTestVisible = false;

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = img;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                    return;
                }

                //Create item webview
                string WebLink = htmlNode.Attributes["src"].Value;
                if (!String.IsNullOrWhiteSpace(WebLink))
                {
                    Debug.WriteLine("Opening webview: " + WebLink);

                    grid_item_webview webView = new grid_item_webview();
                    webView.item_source.Source = new Uri(WebLink);
                    webView.item_source.ContainsFullScreenElementChanged += webview_Full_ContainsFullScreenElementChanged;
                    webView.item_source.NewWindowRequested += webview_Full_NewWindowRequested;

                    InlineUIContainer iui = new InlineUIContainer();
                    iui.Child = webView;

                    addSpan.Inlines.Add(iui);
                    //addSpan.Inlines.Add(new LineBreak());
                }
            }
            catch { }
        }

        private async Task GenerateHyperLink(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                //Add other child node elements
                vIgnoreText = true;
                await AddNodes(addSpan, htmlNode, false);
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
                    Hyperlink hyperLink = new Hyperlink();
                    hyperLink.Inlines.Add(new Run() { Text = StringText });
                    ToolTipService.SetToolTip(hyperLink, LinkUrl);
                    hyperLink.Click += async delegate
                    {
                        Uri targetUri = new Uri(ToolTipService.GetToolTip(hyperLink).ToString(), UriKind.RelativeOrAbsolute);
                        await OpenBrowser(targetUri, false);
                    };

                    Debug.WriteLine("Adding text link: " + StringText);
                    addSpan.Inlines.Add(hyperLink);
                }
            }
            catch { }
        }

        private async Task GenerateBold(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                Bold bold = new Bold();
                await AddNodes(bold, htmlNode, false);
                addSpan.Inlines.Add(bold);
            }
            catch { }
        }

        private async Task GenerateUnderline(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                Underline underline = new Underline();
                await AddNodes(underline, htmlNode, false);
                addSpan.Inlines.Add(underline);
            }
            catch { }
        }

        private async Task GenerateItalic(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                Italic italic = new Italic();
                await AddNodes(italic, htmlNode, false);
                addSpan.Inlines.Add(italic);
            }
            catch { }
        }

        private void GenerateHeader(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                string StringText = Process.ProcessItemTextFull(htmlNode.InnerText, false, false, true);
                if (!String.IsNullOrWhiteSpace(StringText))
                {
                    Binding FontSizeBinding = new Binding();
                    FontSizeBinding.Source = (StyleUpdater)Application.Current.Resources["StyleUpdater"];
                    FontSizeBinding.Path = new PropertyPath("TextSizeLarge");

                    Span spanHeader = new Span();
                    BindingOperations.SetBinding(spanHeader, Span.FontSizeProperty, FontSizeBinding);
                    spanHeader.Inlines.Add(new Run() { Text = StringText, Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]) });
                    spanHeader.Inlines.Add(new LineBreak());

                    addSpan.Inlines.Add(spanHeader);
                }
            }
            catch { }
        }

        private void GenerateGridText(Span addSpan, HtmlNode htmlNode, String textHeader, bool textRaw, bool textHtml, TextAlignment textAlignment, HorizontalAlignment horizontalAlignment)
        {
            try
            {
                //Grid Stackpanel
                StackPanel stackPanelGrid = new StackPanel();

                //Grid Header
                if (!String.IsNullOrWhiteSpace(textHeader))
                {
                    TextBlock TextBlockHeader = new TextBlock();
                    TextBlockHeader.Text = textHeader + ":";
                    TextBlockHeader.TextWrapping = TextWrapping.Wrap;
                    TextBlockHeader.HorizontalAlignment = HorizontalAlignment.Left;
                    TextBlockHeader.Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]);

                    //Enable or disable text selection
                    if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                    {
                        TextBlockHeader.IsTextSelectionEnabled = true;
                    }
                    else
                    {
                        TextBlockHeader.IsTextSelectionEnabled = false;
                    }

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(TextBlockHeader);
                }

                //Grid Text
                string StringText = string.Empty;
                if (textHtml) { StringText = Process.ProcessItemTextFull(htmlNode.InnerHtml, false, false, true); } else { StringText = Process.ProcessItemTextFull(htmlNode.InnerText, false, false, true); }
                if (!String.IsNullOrWhiteSpace(StringText))
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = StringText;
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.TextAlignment = textAlignment;
                    textBlock.HorizontalAlignment = horizontalAlignment;

                    //Enable or disable text selection
                    if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                    {
                        textBlock.IsTextSelectionEnabled = true;
                    }
                    else
                    {
                        textBlock.IsTextSelectionEnabled = false;
                    }

                    //StackPanel Content
                    StackPanel stackPanelContent = new StackPanel();
                    stackPanelContent.Background = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136)) { Opacity = 0.20 };
                    stackPanelContent.Children.Add(textBlock);

                    //Add to stackpanel grid
                    stackPanelGrid.Children.Add(stackPanelContent);
                }
                else
                {
                    Debug.WriteLine("No text found to add to grid text.");
                    return;
                }

                InlineUIContainer iui = new InlineUIContainer();
                iui.Child = stackPanelGrid;
                addSpan.Inlines.Add(iui);
            }
            catch { }
        }

        private async Task GenerateGridContent(Span addSpan, HtmlNode htmlNode, String textHeader)
        {
            try
            {
                //Check if node contains childs
                if (!htmlNode.ChildNodes.Any()) { Debug.WriteLine("No child nodes found to add to grid content."); return; }

                //Grid Stackpanel
                StackPanel stackPanelGrid = new StackPanel();

                //Grid Header
                if (!String.IsNullOrWhiteSpace(textHeader))
                {
                    TextBlock TextBlockHeader = new TextBlock();
                    TextBlockHeader.Text = textHeader + ":";
                    TextBlockHeader.TextWrapping = TextWrapping.Wrap;
                    TextBlockHeader.HorizontalAlignment = HorizontalAlignment.Left;
                    TextBlockHeader.Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]);

                    //Enable or disable text selection
                    if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                    {
                        TextBlockHeader.IsTextSelectionEnabled = true;
                    }
                    else
                    {
                        TextBlockHeader.IsTextSelectionEnabled = false;
                    }

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(TextBlockHeader);
                }

                //Add other child node elements
                Span spanContent = new Span();
                await AddNodes(spanContent, htmlNode, false);
                Paragraph paraContent = new Paragraph();
                paraContent.Inlines.Add(spanContent);

                //Convert span to textblock
                RichTextBlock richTextBlock = new RichTextBlock();
                richTextBlock.TextWrapping = TextWrapping.Wrap;

                //Enable or disable text selection
                if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                {
                    richTextBlock.IsTextSelectionEnabled = true;
                }
                else
                {
                    richTextBlock.IsTextSelectionEnabled = false;
                }

                richTextBlock.Blocks.Add(paraContent);

                //StackPanel Content
                StackPanel stackPanelContent = new StackPanel();
                stackPanelContent.Background = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136)) { Opacity = 0.20 };
                stackPanelContent.Children.Add(richTextBlock);

                //Add to stackpanel grid
                stackPanelGrid.Children.Add(stackPanelContent);

                InlineUIContainer iui = new InlineUIContainer();
                iui.Child = stackPanelGrid;
                addSpan.Inlines.Add(iui);
            }
            catch { }
        }

        private async Task GenerateTable(Span addSpan, HtmlNode htmlNode, String textHeader)
        {
            try
            {
                //Check if node contains childs
                if (!htmlNode.ChildNodes.Any()) { Debug.WriteLine("No child nodes found to add to grid content."); return; }

                //Set image settings
                vImageShowAlt = false;

                //Grid Stackpanel
                StackPanel stackPanelGrid = new StackPanel();

                //Grid Header
                if (!String.IsNullOrWhiteSpace(textHeader))
                {
                    TextBlock TextBlockHeader = new TextBlock();
                    TextBlockHeader.Text = textHeader + ":";
                    TextBlockHeader.TextWrapping = TextWrapping.Wrap;
                    TextBlockHeader.HorizontalAlignment = HorizontalAlignment.Left;
                    TextBlockHeader.Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]);

                    //Enable or disable text selection
                    if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                    {
                        TextBlockHeader.IsTextSelectionEnabled = true;
                    }
                    else
                    {
                        TextBlockHeader.IsTextSelectionEnabled = false;
                    }

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(TextBlockHeader);
                }

                Int32 RowCurrentCount = 0;
                Int32 RowTotalCount = htmlNode.Descendants("tr").Count();
                if (RowTotalCount > 0)
                {
                    //Add table child node elements
                    Grid gridContent = new Grid();
                    gridContent.Background = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136)) { Opacity = 0.20 };

                    foreach (HtmlNode TableRow in htmlNode.Descendants("tr"))
                    {
                        Int32 ColumnCurrentCount = 0;

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
                            gridRow.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]) { Opacity = 0.60 };
                            gridRow.BorderThickness = new Thickness(0, 0, 0, 2);
                        }

                        //Table Header
                        foreach (HtmlNode TableHeader in TableRow.Descendants("th"))
                        {
                            ColumnDefinition gridColDefinition = new ColumnDefinition();
                            gridRow.ColumnDefinitions.Add(gridColDefinition);

                            TextBlock textBlock = new TextBlock();
                            textBlock.Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]);
                            textBlock.Text = TableHeader.InnerText;

                            //Enable or disable text selection
                            if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                            {
                                textBlock.IsTextSelectionEnabled = true;
                            }
                            else
                            {
                                textBlock.IsTextSelectionEnabled = false;
                            }

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
                            Span spanContent = new Span();
                            await AddNodes(spanContent, TableColumn, false);
                            Paragraph paraContent = new Paragraph();
                            paraContent.Inlines.Add(spanContent);

                            //Convert span to textblock
                            RichTextBlock richTextBlock = new RichTextBlock();
                            richTextBlock.TextWrapping = TextWrapping.Wrap;

                            //Enable or disable text selection
                            if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                            {
                                richTextBlock.IsTextSelectionEnabled = true;
                            }
                            else
                            {
                                richTextBlock.IsTextSelectionEnabled = false;
                            }

                            richTextBlock.Blocks.Add(paraContent);

                            gridRow.Children.Add(richTextBlock);
                            Grid.SetColumn(richTextBlock, ColumnCurrentCount);
                            ColumnCurrentCount++;
                        }
                    }

                    //Add to stackpanel grid
                    stackPanelGrid.Children.Add(gridContent);
                }
                else
                {
                    Debug.WriteLine("No row found in the table.");

                    //Add other child node elements
                    Span spanContent = new Span();
                    await AddNodes(spanContent, htmlNode, false);
                    Paragraph paraContent = new Paragraph();
                    paraContent.Inlines.Add(spanContent);

                    //Convert span to textblock
                    RichTextBlock richTextBlock = new RichTextBlock();
                    richTextBlock.TextWrapping = TextWrapping.Wrap;

                    //Enable or disable text selection
                    if ((bool)AppVariables.ApplicationSettings["ItemTextSelection"])
                    {
                        richTextBlock.IsTextSelectionEnabled = true;
                    }
                    else
                    {
                        richTextBlock.IsTextSelectionEnabled = false;
                    }

                    richTextBlock.Blocks.Add(paraContent);

                    //StackPanel Content
                    StackPanel stackPanelContent = new StackPanel();
                    stackPanelContent.Background = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136)) { Opacity = 0.20 };
                    stackPanelContent.Children.Add(richTextBlock);

                    //Add to stackpanel grid
                    stackPanelGrid.Children.Add(stackPanelContent);
                }

                InlineUIContainer iui = new InlineUIContainer();
                iui.Child = stackPanelGrid;
                addSpan.Inlines.Add(iui);

                //Set image settings
                vImageShowAlt = true;
            }
            catch { }
        }

        private async Task GenerateUl(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                await AddNodes(addSpan, htmlNode, false);
                addSpan.Inlines.Add(new LineBreak());
            }
            catch { }
        }

        private async Task GenerateLi(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                addSpan.Inlines.Add(new Run() { Text = "* ", Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]) });
                await AddNodes(addSpan, htmlNode, false);
                addSpan.Inlines.Add(new LineBreak());
            }
            catch { }
        }

        private void GenerateBreak(Span addSpan)
        {
            try
            {
                addSpan.Inlines.Add(new LineBreak());
            }
            catch { }
        }

        private async Task GenerateParagraph(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                await AddNodes(addSpan, htmlNode, false);
                addSpan.Inlines.Add(new LineBreak());
                addSpan.Inlines.Add(new LineBreak());
            }
            catch { }
        }

        private void GenerateText(Span addSpan, HtmlNode htmlNode)
        {
            try
            {
                string StringText = Process.ProcessItemTextFull(htmlNode.InnerText, true, false, true);
                if (!String.IsNullOrWhiteSpace(StringText))
                {
                    Debug.WriteLine("Adding text node: " + StringText);
                    addSpan.Inlines.Add(new Run() { Text = StringText });
                }
            }
            catch { }
        }
    }
}