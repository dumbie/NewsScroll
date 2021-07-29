using ArnoldVinkCode;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static NewsScroll.Database.Database;

namespace NewsScroll
{
    public partial class ItemPopup
    {
        //Load item into the viewer
        private async Task LoadItem(string CustomItemContent)
        {
            try
            {
                //Set item bindings
                Binding binding_item_title = new Binding();
                binding_item_title.Source = vCurrentItem;
                binding_item_title.Path = new PropertyPath("item_title");
                tb_ItemTitle.SetBinding(TextBlock.TextProperty, binding_item_title);

                Binding binding_item_datestring = new Binding();
                binding_item_datestring.Source = vCurrentItem;
                binding_item_datestring.Path = new PropertyPath("item_datestring");
                tb_ItemDateString.SetBinding(TextBlock.TextProperty, binding_item_datestring);

                Binding binding_item_read_status = new Binding();
                binding_item_read_status.Source = vCurrentItem;
                binding_item_read_status.Path = new PropertyPath("item_read_status");
                image_item_read_status.SetBinding(Image.VisibilityProperty, binding_item_read_status);

                Binding binding_item_star_status = new Binding();
                binding_item_star_status.Source = vCurrentItem;
                binding_item_star_status.Path = new PropertyPath("item_star_status");
                image_item_star_status.SetBinding(Image.VisibilityProperty, binding_item_star_status);

                Binding binding_feed_icon = new Binding();
                binding_feed_icon.Source = vCurrentItem;
                binding_feed_icon.Path = new PropertyPath("feed_icon");
                image_feed_icon.SetBinding(Image.SourceProperty, binding_feed_icon);

                Binding binding_feed_title = new Binding();
                binding_feed_title.Source = vCurrentItem;
                binding_feed_title.Path = new PropertyPath("feed_title");

                ToolTip header_tooltip = new ToolTip();
                header_tooltip.SetBinding(ToolTip.ContentProperty, binding_feed_title);
                ToolTipService.SetToolTip(stackpanel_HeaderItem, header_tooltip);

                //Load the full item
                TableItems LoadTable = await SQLConnection.Table<TableItems>().Where(x => x.item_id == vCurrentItem.item_id).FirstOrDefaultAsync();
                if (LoadTable != null)
                {
                    //Load the item content
                    bool SetHtmlToRichTextBlock = false;
                    if (!string.IsNullOrWhiteSpace(CustomItemContent))
                    {
                        await HtmlToXaml(rtb_ItemContent, CustomItemContent, string.Empty);
                        SetHtmlToRichTextBlock = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(LoadTable.item_content_full))
                    {
                        SetHtmlToRichTextBlock = await HtmlToXaml(rtb_ItemContent, LoadTable.item_content_full, string.Empty);
                    }

                    //Check if html to xaml has failed
                    if (!SetHtmlToRichTextBlock || !rtb_ItemContent.Children.Any())
                    {
                        //Load summary text
                        TextBlock textLabel = new TextBlock();
                        textLabel.Text = AVFunctions.StringCut(LoadTable.item_content, AppVariables.MaximumItemTextLength, "...");

                        //Add paragraph to rich text block
                        rtb_ItemContent.Children.Clear();
                        rtb_ItemContent.Children.Add(textLabel);
                    }

                    //Check if item content contains preview image
                    await CheckItemContentContainsPreviewImage(LoadTable);

                    //Adjust the itemviewer size
                    await AdjustItemViewerSize();
                }
            }
            catch { }
        }

        //Check if item content contains preview image
        private async Task CheckItemContentContainsPreviewImage(TableItems LoadTable)
        {
            try
            {
                int ItemImagecount = 0;
                bool FoundPreviewImage = false;

                //Check the preview image
                string ItemImageLink = LoadTable.item_image;
                if (string.IsNullOrWhiteSpace(ItemImageLink))
                {
                    item_image.item_image_Value = null;
                    item_image.Visibility = Visibility.Collapsed;
                    return;
                }

                //Check if there are images and the preview image is included
                CheckTextBlockForPreviewImage(rtb_ItemContent, ItemImageLink, ref ItemImagecount, ref FoundPreviewImage);

                //Update the preview image based on result
                if (ItemImagecount == 0 || !FoundPreviewImage)
                {
                    System.Diagnostics.Debug.WriteLine("No media found in rich text block, adding item image.");
                    item_image.MaxHeight = AppVariables.MaximumItemImageHeight;
                    item_image.item_image_Value = ItemImageLink;
                    item_image.Visibility = Visibility.Visible;
                }
                else
                {
                    item_image.item_image_Value = null;
                    item_image.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                item_image.item_image_Value = null;
                item_image.Visibility = Visibility.Collapsed;
            }
        }

        //Check if there are images and the preview image is included
        private void CheckTextBlockForPreviewImage(DependencyObject SearchElement, string ItemImageLink, ref int ItemImagecount, ref bool FoundPreviewImage)
        {
            try
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(SearchElement); i++)
                {
                    try
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(SearchElement, i);
                        if (child.GetType() == typeof(ImageContainer))
                        {
                            ItemImagecount++;
                            ImageContainer frameworkElement = child as ImageContainer;
                            BitmapImage bitmapSource = frameworkElement.item_source.Source as BitmapImage;

                            string CompareBitmapLink = Regex.Replace(bitmapSource.UriSource.ToString(), @"^(?:http(?:s)?://)?(?:www(?:[0-9]+)?\.)?", string.Empty, RegexOptions.IgnoreCase).ToLower();
                            string CompareItemImageLink = Regex.Replace(ItemImageLink, @"^(?:http(?:s)?://)?(?:www(?:[0-9]+)?\.)?", string.Empty, RegexOptions.IgnoreCase).ToLower();
                            //System.Diagnostics.Debug.WriteLine("Comparing image: " + CompareBitmapLink + " vs " + CompareItemImageLink);

                            if (CompareBitmapLink == CompareItemImageLink)
                            {
                                FoundPreviewImage = true;
                                break;
                            }
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("No image, checking if there is a sub image.");
                            CheckTextBlockForPreviewImage(child, ItemImageLink, ref ItemImagecount, ref FoundPreviewImage);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}