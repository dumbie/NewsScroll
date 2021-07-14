using HtmlAgilityPack;
using Octane.Xamarin.Forms.VideoPlayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppVariables;

namespace NewsScroll
{
    public partial class ItemViewer
    {
        private async void GenerateImage(StackLayout addElement, HtmlNode htmlNode)
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
                    Debug.WriteLine("Blocked image: " + sourceUri);
                    return;
                }

                //fix
                ////Check if device is low on memory
                //if (AVFunctions.DevMemoryAvailableMB() < 100)
                //{
                //    grid_item_image img = new grid_item_image();
                //    img.item_status.Text = "Image not loaded,\ndevice is low on memory.";
                //    img.IsEnabled = false;

                //    InlineUIContainer iui = new InlineUIContainer();
                //    iui.Child = img;

                //    addElement.Children.Add(iui);
                //    //GenerateBreak(addElement);
                //    return;
                //}

                //Check if media is a gif(v) file
                bool ImageIsGif = sourceUri.ToLower().Contains(".gif");
                //bool ImageIsSvg = sourceUri.ToLower().Contains(".svg");

                //Check if low bandwidth mode is enabled
                if (ImageIsGif && (bool)AppSettingLoad("LowBandwidthMode"))
                {
                    Label img_lowbandwidth = new Label();
                    img_lowbandwidth.Text = "Gif not loaded,\nlow bandwidth mode.";

                    addElement.Children.Add(img_lowbandwidth);
                    return;
                }

                //Create item image
                Image img_source = new Image();
                Uri imageUri = new Uri(sourceUri);
                Stream imageStream = await dependencyAVImages.DownloadResizeImage(imageUri, 1024, 1024);
                img_source.Source = ImageSource.FromStream(() => imageStream);
                img_source.MinimumHeightRequest = 300;

                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                img_source.GestureRecognizers.Add(tapGestureRecognizer);
                tapGestureRecognizer.Tapped += delegate
                {
                    new ImagePopup().Popup(sourceUri);
                };

                addElement.Children.Add(img_source);

                //Get and set alt from the image
                if (vImageShowAlt && htmlNode.Attributes["alt"] != null)
                {
                    string imageAltText = Process.ProcessItemTextSummary(htmlNode.Attributes["alt"].Value, false, false);
                    if (!string.IsNullOrWhiteSpace(imageAltText))
                    {
                        Label imageAltLabel = new Label();
                        imageAltLabel.Text = "* " + imageAltText;
                        imageAltLabel.SetDynamicResource(Label.TextColorProperty, "ApplicationAccentLightColor");
                        imageAltLabel.SetDynamicResource(Label.BackgroundColorProperty, "ApplicationLightGrayColor");

                        //Fix
                        ////Enable or disable text selection
                        //if ((bool)AppSettingLoad("ItemTextSelection"])
                        //{
                        //    img.item_description.IsTextSelectionEnabled = true;
                        //}
                        //else
                        //{
                        //    img.item_description.IsTextSelectionEnabled = false;
                        //}

                        addElement.Children.Add(imageAltLabel);
                    }
                }

                Debug.WriteLine("Added image: " + sourceUri);
                //GenerateBreak(addElement);
            }
            catch
            {
                Label img_failed = new Label();
                img_failed.Text = "Image is not available,\nopen item in browser to view it.";

                addElement.Children.Add(img_failed);
                //GenerateBreak(addElement);
                return;
            }
        }

        private void GenerateVideo(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                //Check if media loading is allowed
                if (!AppVariables.LoadMedia)
                {
                    Label video_network = new Label();
                    video_network.Text = "Video not loaded,\nnetwork is not available.";

                    addElement.Children.Add(video_network);
                    //GenerateBreak(addElement);
                    return;
                }

                //fix
                ////Check if device is low on memory
                //if (AVFunctions.DevMemoryAvailableMB() < 200)
                //{
                //    grid_item_video video = new grid_item_video();
                //    video.item_status.Text = "Video not loaded,\ndevice is low on memory.";

                //    InlineUIContainer iui = new InlineUIContainer();
                //    iui.Child = video;

                //    addElement.Children.Add(iui);
                //    //GenerateBreak(addElement);
                //    return;
                //}

                //Check if low bandwidth mode is enabled
                if ((bool)AppSettingLoad("LowBandwidthMode"))
                {
                    Label img_lowbandwidth = new Label();
                    img_lowbandwidth.Text = "Video not loaded,\nlow bandwidth mode.";

                    addElement.Children.Add(img_lowbandwidth);
                    //GenerateBreak(addElement);
                    return;
                }

                //Create item video
                string VideoString = htmlNode.Attributes["src"].Value;
                if (!string.IsNullOrWhiteSpace(VideoString))
                {
                    VideoPlayer video = new VideoPlayer();
                    video.MinimumHeightRequest = 300;
                    video.HorizontalOptions = LayoutOptions.Fill;
                    video.Source = new Uri(VideoString);
                    video.DisplayControls = true;

                    //Check if media is a gif(v) file
                    if (VideoString.ToLower().Contains(".gif"))
                    {
                        video.Repeat = true;
                        video.AutoPlay = true;
                    }
                    else
                    {
                        video.AutoPlay = false;
                    }

                    addElement.Children.Add(video);
                    //GenerateBreak(addElement);
                    Debug.WriteLine("Added video: " + VideoString);
                }
            }
            catch { }
        }

        private void GenerateWebview(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                //Check if webview limit reached
                if (vWebViewAdded == vWebViewLimit)
                {
                    Label webView_limit = new Label();
                    webView_limit.Text = "Webview not loaded,\nlimit has been reached.";

                    addElement.Children.Add(webView_limit);
                    //GenerateBreak(addElement);
                    return;
                }

                //Check if media loading is allowed
                if (!AppVariables.LoadMedia)
                {
                    Label webView_notavailable = new Label();
                    webView_notavailable.Text = "Webview not loaded,\nnetwork is not available.";

                    addElement.Children.Add(webView_notavailable);
                    //GenerateBreak(addElement);
                    return;
                }

                //fix
                ////Check if device is low on memory
                //if (AVFunctions.DevMemoryAvailableMB() < 200)
                //{
                //    Label webView_lowmemory = new Label();
                //    webView_lowmemory.Text = "Webview not loaded,\ndevice is low on memory.";

                //    addElement.Children.Add(webView_lowmemory);
                //    //GenerateBreak(addElement);
                //    return;
                //}

                //Check if low bandwidth mode is enabled
                if ((bool)AppSettingLoad("LowBandwidthMode"))
                {
                    Label img_lowbandwidth = new Label();
                    img_lowbandwidth.Text = "Webview not loaded,\nlow bandwidth mode.";

                    addElement.Children.Add(img_lowbandwidth);
                    //GenerateBreak(addElement);
                    return;
                }

                //Create item webview
                string WebLink = htmlNode.Attributes["src"].Value;
                if (!string.IsNullOrWhiteSpace(WebLink))
                {
                    WebView webView = new WebView();
                    webView.Source = new Uri(WebLink);
                    webView.MinimumHeightRequest = 300;

                    addElement.Children.Add(webView);
                    //GenerateBreak(addElement);
                    Debug.WriteLine("Added webview: " + WebLink);

                    //Update the webview count
                    vWebViewAdded++;
                }
            }
            catch { }
        }

        private void GenerateHyperLink(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                //Add other child node elements
                vIgnoreText = true;
                AddNodes(addElement, htmlNode, false);
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
                    Label hyperLink = new Label();
                    hyperLink.Text = StringText;
                    hyperLink.SetDynamicResource(Label.TextColorProperty, "ApplicationAccentLightColor");
                    hyperLink.TextDecorations = TextDecorations.Underline;

                    TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                    hyperLink.GestureRecognizers.Add(tapGestureRecognizer);
                    tapGestureRecognizer.Tapped += async delegate
                    {
                        Uri targetUri = new Uri(LinkUrl, UriKind.RelativeOrAbsolute);
                        await OpenBrowser(targetUri, false);
                    };

                    addElement.Children.Add(hyperLink);
                    Debug.WriteLine("Added hyperLink: " + StringText);
                }
            }
            catch { }
        }

        private void GenerateBold(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                Label bold = new Label();
                bold.Text = htmlNode.InnerText;
                bold.FontAttributes = FontAttributes.Bold;

                addElement.Children.Add(bold);
            }
            catch { }
        }

        private void GenerateUnderline(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                Label underline = new Label();
                underline.Text = htmlNode.InnerText;
                underline.TextDecorations = TextDecorations.Underline;

                addElement.Children.Add(underline);
            }
            catch { }
        }

        private void GenerateStrikethrough(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                Label underline = new Label();
                underline.Text = htmlNode.InnerText;
                underline.TextDecorations = TextDecorations.Strikethrough;

                addElement.Children.Add(underline);
            }
            catch { }
        }

        private void GenerateItalic(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                Label italic = new Label();
                italic.Text = htmlNode.InnerText;
                italic.FontAttributes = FontAttributes.Italic;

                addElement.Children.Add(italic);
            }
            catch { }
        }

        private void GenerateHeader(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                string StringText = Process.ProcessItemTextFull(htmlNode.InnerText, false, false, true);
                if (!string.IsNullOrWhiteSpace(StringText))
                {
                    Label headerText = new Label();
                    headerText.Text = StringText;
                    headerText.SetDynamicResource(Label.TextColorProperty, "ApplicationAccentLightColor");
                    headerText.SetDynamicResource(Label.FontSizeProperty, "TextSizeLarge");

                    addElement.Children.Add(headerText);
                }
            }
            catch { }
        }

        private void GenerateGridText(StackLayout addElement, HtmlNode htmlNode, string textHeader, bool textRaw, TextAlignment textAlignment, LayoutOptions horizontalOptions)
        {
            try
            {
                //Grid stacklayout
                StackLayout stackPanelGrid = new StackLayout();

                //Grid Header
                if (!string.IsNullOrWhiteSpace(textHeader))
                {
                    Label LabelHeader = new Label();
                    LabelHeader.Text = textHeader + ":";
                    LabelHeader.HorizontalOptions = LayoutOptions.Start;
                    LabelHeader.SetDynamicResource(Label.TextColorProperty, "ApplicationAccentLightColor");

                    //Fix
                    ////Enable or disable text selection
                    //if ((bool)AppSettingLoad("ItemTextSelection"])
                    //{
                    //    LabelHeader.IsTextSelectionEnabled = true;
                    //}
                    //else
                    //{
                    //    LabelHeader.IsTextSelectionEnabled = false;
                    //}

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(LabelHeader);
                }

                //Grid Text
                string StringText = string.Empty;
                if (textRaw)
                {
                    StringText = Process.ProcessItemTextFull(htmlNode.InnerHtml, false, false, true);
                }
                else
                {
                    StringText = Process.ProcessItemTextFull(htmlNode.InnerText, false, false, true);
                }

                if (!string.IsNullOrWhiteSpace(StringText))
                {
                    Label Label = new Label();
                    Label.Text = StringText;
                    Label.HorizontalTextAlignment = textAlignment;
                    Label.HorizontalOptions = horizontalOptions;
                    Label.SetDynamicResource(Label.BackgroundColorProperty, "ApplicationLightGrayColor");

                    //fix
                    ////Enable or disable text selection
                    //if ((bool)AppSettingLoad("ItemTextSelection"])
                    //{
                    //    Label.IsTextSelectionEnabled = true;
                    //}
                    //else
                    //{
                    //    Label.IsTextSelectionEnabled = false;
                    //}

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(Label);
                    addElement.Children.Add(stackPanelGrid);
                }
                else
                {
                    Debug.WriteLine("No text found to add to grid text.");
                    return;
                }
            }
            catch { }
        }

        private void GenerateGridContent(StackLayout addElement, HtmlNode htmlNode, string textHeader)
        {
            try
            {
                //Check if node contains childs
                if (!htmlNode.ChildNodes.Any())
                {
                    Debug.WriteLine("No child nodes found to add to grid content.");
                    return;
                }

                //Grid stacklayout
                StackLayout stackPanelGrid = new StackLayout();

                //Grid Header
                if (!string.IsNullOrWhiteSpace(textHeader))
                {
                    Label LabelHeader = new Label();
                    LabelHeader.Text = textHeader + ":";
                    LabelHeader.HorizontalOptions = LayoutOptions.Start;
                    LabelHeader.SetDynamicResource(Label.TextColorProperty, "ApplicationAccentLightColor");

                    //Fix
                    ////Enable or disable text selection
                    //if ((bool)AppSettingLoad("ItemTextSelection"])
                    //{
                    //    LabelHeader.IsTextSelectionEnabled = true;
                    //}
                    //else
                    //{
                    //    LabelHeader.IsTextSelectionEnabled = false;
                    //}

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(LabelHeader);
                }

                //Add other child node elements
                StackLayout spanContent = new StackLayout();
                spanContent.SetDynamicResource(StackLayout.BackgroundColorProperty, "ApplicationLightGrayColor");
                AddNodes(spanContent, htmlNode, false);
                stackPanelGrid.Children.Add(spanContent);

                //Fix
                ////Enable or disable text selection
                //if ((bool)AppSettingLoad("ItemTextSelection"])
                //{
                //    richLabel.IsTextSelectionEnabled = true;
                //}
                //else
                //{
                //    richLabel.IsTextSelectionEnabled = false;
                //}

                //Add to stackpanel grid
                addElement.Children.Add(stackPanelGrid);
            }
            catch { }
        }

        private void GenerateTable(StackLayout addElement, HtmlNode htmlNode, string textHeader)
        {
            try
            {
                //Check if node contains childs
                if (!htmlNode.ChildNodes.Any()) { Debug.WriteLine("No child nodes found to add to grid content."); return; }

                //Set image settings
                vImageShowAlt = false;

                //Grid Stackpanel
                StackLayout stackPanelGrid = new StackLayout();

                //Grid Header
                if (!string.IsNullOrWhiteSpace(textHeader))
                {
                    Label LabelHeader = new Label();
                    LabelHeader.Text = textHeader + ":";
                    LabelHeader.HorizontalOptions = LayoutOptions.Start;
                    LabelHeader.SetDynamicResource(Label.TextColorProperty, "ApplicationAccentLightColor");

                    //Fix
                    ////Enable or disable text selection
                    //if ((bool)AppSettingLoad("ItemTextSelection"])
                    //{
                    //    LabelHeader.IsTextSelectionEnabled = true;
                    //}
                    //else
                    //{
                    //    LabelHeader.IsTextSelectionEnabled = false;
                    //}

                    //Add to stackpanel
                    stackPanelGrid.Children.Add(LabelHeader);
                }

                int RowCurrentCount = 0;
                int RowTotalCount = htmlNode.Descendants("tr").Count();
                if (RowTotalCount > 0)
                {
                    //Add table child node elements
                    Grid gridContent = new Grid();
                    gridContent.SetDynamicResource(Grid.BackgroundColorProperty, "ApplicationLightGrayColor");

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

                        //Fix
                        ////Set grid row style
                        //if (RowCurrentCount != RowTotalCount)
                        //{
                        //    gridRow.BorderBrush = new SolidColorBrush((Color)ApplicationLightGrayColor);
                        //    gridRow.BorderThickness = new Thickness(0, 0, 0, 2);
                        //}

                        //Table Header
                        foreach (HtmlNode TableHeader in TableRow.Descendants("th"))
                        {
                            ColumnDefinition gridColDefinition = new ColumnDefinition();
                            gridRow.ColumnDefinitions.Add(gridColDefinition);

                            Label Label = new Label();
                            Label.Text = TableHeader.InnerText;
                            Label.SetDynamicResource(Label.TextColorProperty, "ApplicationAccentLightColor");

                            //Fix
                            ////Enable or disable text selection
                            //if ((bool)AppSettingLoad("ItemTextSelection"])
                            //{
                            //    Label.IsTextSelectionEnabled = true;
                            //}
                            //else
                            //{
                            //    Label.IsTextSelectionEnabled = false;
                            //}

                            gridRow.Children.Add(Label);
                            Grid.SetColumn(Label, ColumnCurrentCount);
                            ColumnCurrentCount++;
                        }

                        //Table Column
                        foreach (HtmlNode TableColumn in TableRow.Descendants("td"))
                        {
                            ColumnDefinition gridColDefinition = new ColumnDefinition();
                            gridRow.ColumnDefinitions.Add(gridColDefinition);

                            //Add other child node elements
                            StackLayout spanContent = new StackLayout();
                            AddNodes(spanContent, TableColumn, false);

                            ////Enable or disable text selection
                            //if ((bool)AppSettingLoad("ItemTextSelection"])
                            //{
                            //    richLabel.IsTextSelectionEnabled = true;
                            //}
                            //else
                            //{
                            //    richLabel.IsTextSelectionEnabled = false;
                            //}

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
                    Debug.WriteLine("No row found in the table.");

                    //Add other child node elements
                    StackLayout spanContent = new StackLayout();
                    spanContent.SetDynamicResource(StackLayout.BackgroundColorProperty, "ApplicationLightGrayColor");
                    AddNodes(spanContent, htmlNode, false);

                    ////Enable or disable text selection
                    //if ((bool)AppSettingLoad("ItemTextSelection"])
                    //{
                    //    richLabel.IsTextSelectionEnabled = true;
                    //}
                    //else
                    //{
                    //    richLabel.IsTextSelectionEnabled = false;
                    //}

                    //Add to stackpanel grid
                    stackPanelGrid.Children.Add(spanContent);
                }

                addElement.Children.Add(stackPanelGrid);

                //Set image settings
                vImageShowAlt = true;
            }
            catch { }
        }

        private void GenerateUl(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                AddNodes(addElement, htmlNode, false);
                //GenerateBreak(addElement);
            }
            catch { }
        }

        private void GenerateLi(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                string liText = htmlNode.InnerText;
                if (!string.IsNullOrWhiteSpace(liText))
                {
                    Span liHeader = new Span { Text = "* " };
                    liHeader.SetDynamicResource(Span.TextColorProperty, "ApplicationAccentLightColor");
                    Span liContent = new Span { Text = liText };

                    FormattedString formattedString = new FormattedString();
                    formattedString.Spans.Add(liHeader);
                    formattedString.Spans.Add(liContent);

                    Label textLabel = new Label();
                    textLabel.FormattedText = formattedString;

                    addElement.Children.Add(textLabel);
                    Debug.WriteLine("Added li node: " + liText);
                }
            }
            catch { }
        }

        private void GenerateBreak(StackLayout addElement)
        {
            try
            {
                Grid addGrid = new Grid();
                addGrid.HeightRequest = 10;
                addGrid.HorizontalOptions = LayoutOptions.Fill;
                addGrid.BackgroundColor = Color.Transparent;

                addElement.Children.Add(addGrid);
            }
            catch { }
        }

        private void GenerateParagraph(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                AddNodes(addElement, htmlNode, false);
                //GenerateBreak(addElement);
                //GenerateBreak(addElement);
            }
            catch { }
        }

        private void GenerateText(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                string stringText = Process.ProcessItemTextFull(htmlNode.InnerText, true, false, true);
                if (!string.IsNullOrWhiteSpace(stringText))
                {
                    Label textLabel = new Label();
                    textLabel.Text = stringText;
                    textLabel.SetDynamicResource(Label.StyleProperty, "LabelDark");
                    textLabel.SetDynamicResource(Label.FontSizeProperty, "TextSizeMedium");

                    addElement.Children.Add(textLabel);
                    Debug.WriteLine("Added text node: " + stringText);
                }
            }
            catch { }
        }

        private void GenerateSpan(StackLayout addElement, HtmlNode htmlNode)
        {
            try
            {
                AddNodes(addElement, htmlNode, false);
                //GenerateBreak(addElement);
            }
            catch { }
        }
    }
}