﻿using ArnoldVinkCode;
using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace NewsScroll
{
    partial class Process
    {
        //Get first item image
        public static string GetItemHtmlFirstImage(String FullHtml, String BaseLink)
        {
            string ReturnImageSource = String.Empty;
            try
            {
                //Load and parse the html document
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(FullHtml);

                //Check for invalid image links
                foreach (HtmlNode Image in htmlDocument.DocumentNode.Descendants("img"))
                {
                    string CurrentImageSource = Image.Attributes["src"].Value;
                    if (!AppVariables.BlockedListUrl.Any(CurrentImageSource.Contains))
                    {
                        if (!CurrentImageSource.StartsWith("http"))
                        {
                            if (CurrentImageSource.StartsWith("//")) { ReturnImageSource = "http:" + CurrentImageSource; }
                            else { ReturnImageSource = BaseLink + "/" + CurrentImageSource; }
                            break;
                        }
                        else
                        {
                            ReturnImageSource = CurrentImageSource;
                            break;
                        }
                    }
                    else { Debug.WriteLine("Blocked image: " + CurrentImageSource); }
                }
            }
            catch { }
            return ReturnImageSource;
        }

        public static string ProcessItemTextSummary(String SummaryContent, bool CutToMaximumLength, bool CheckEmpty)
        {
            try
            {
                //Set and decode the html
                SummaryContent = WebUtility.HtmlDecode(SummaryContent);
                //SummaryContent = WebUtility.UrlDecode(SummaryContent);

                //Remove tabs from the content
                SummaryContent = SummaryContent.Replace("\t", string.Empty);

                //Add new line breaks to </p>
                SummaryContent = SummaryContent.Replace("</p>", "\n\n</p>");

                //Replace <li> to a character
                SummaryContent = SummaryContent.Replace("<li>", "* <li>");

                //Replace <h> to a character
                SummaryContent = Regex.Replace(SummaryContent, @"(<h[0-9]>)", "- $1");

                //Remove html tags from the content
                SummaryContent = Regex.Replace(SummaryContent, @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", String.Empty, RegexOptions.Singleline);

                //Replace multiple spaces
                SummaryContent = Regex.Replace(SummaryContent, @"\s*( ){2,}\s*", " ");

                //Replace multiple line breaks
                SummaryContent = Regex.Replace(SummaryContent, @"\s*(\n){3,}\s*", "\n\n");

                //Remove starting and ending line breaks
                SummaryContent = AVFunctions.StringRemoveMultiStart(SummaryContent, new string[] { "\n", " " });
                SummaryContent = AVFunctions.StringRemoveMultiEnd(SummaryContent, new string[] { "\n", " " });

                //Check for empty text content
                if (CheckEmpty && String.IsNullOrWhiteSpace(SummaryContent)) { SummaryContent = "This item does not contain any text, please open this item to read it."; }

                //Cut text to maximum length
                if (CutToMaximumLength) { SummaryContent = AVFunctions.StringCut(SummaryContent, AppVariables.MaximumItemTextLength, "..."); }

                //Return the item content
                return SummaryContent;
            }
            catch
            {
                Debug.WriteLine("Failed processing item summary content.");
                return SummaryContent;
            }
        }

        public static string ProcessItemTextFull(String SummaryContent, bool CutToMaximumLength, bool RemoveSpacing, bool RemoveLineBreaks)
        {
            try
            {
                //Set and decode the html
                SummaryContent = WebUtility.HtmlDecode(SummaryContent);
                //SummaryContent = WebUtility.UrlDecode(SummaryContent);

                //Remove tabs from the content
                SummaryContent = SummaryContent.Replace("\t", string.Empty);

                //Replace multiple spaces
                SummaryContent = Regex.Replace(SummaryContent, @"\s*( ){2,}\s*", " ");

                //Replace multiple line breaks
                SummaryContent = Regex.Replace(SummaryContent, @"\s*(\n){3,}\s*", "\n\n");

                //Remove starting and ending line breaks
                if (RemoveLineBreaks)
                {
                    SummaryContent = AVFunctions.StringRemoveMultiStart(SummaryContent, new string[] { "\n" });
                    SummaryContent = AVFunctions.StringRemoveMultiEnd(SummaryContent, new string[] { "\n" });
                }

                //Remove starting and ending spacing
                if (RemoveSpacing)
                {
                    SummaryContent = AVFunctions.StringRemoveMultiStart(SummaryContent, new string[] { " " });
                    SummaryContent = AVFunctions.StringRemoveMultiEnd(SummaryContent, new string[] { " " });
                }

                //Cut text to maximum length
                if (CutToMaximumLength) { SummaryContent = AVFunctions.StringCut(SummaryContent, AppVariables.MaximumItemTextLength, "..."); }

                //Return the item content
                return SummaryContent;
            }
            catch
            {
                Debug.WriteLine("Failed processing item full content.");
                return SummaryContent;
            }
        }
    }
}