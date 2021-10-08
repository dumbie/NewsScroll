using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static NewsScroll.Events.Events;

namespace NewsScroll
{
    public partial class PersonalizePopup : UserControl
    {
        //Initialize popup
        public PersonalizePopup() { this.InitializeComponent(); }

        //Open the popup
        public async Task Popup()
        {
            try
            {
                //Open the popup
                popup_Main.IsOpen = true;

                //Focus on the popup
                iconClose.Focus(FocusState.Programmatic);

                //Load and set the settings
                await SettingsLoad();
                SettingsSave();
            }
            catch { }
        }

        //Close the popup
        private void iconClose_Tap(object sender, RoutedEventArgs e) { try { Close(); } catch { } }
        public void Close()
        {
            try
            {
                popup_Main.IsOpen = false;
            }
            catch { }
        }

        private async Task SettingsLoad()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading application settings...");

                //Disable Landscape Display
                setting_DisableLandscapeDisplay.IsChecked = (bool)AppVariables.ApplicationSettings["DisableLandscapeDisplay"];

                //Color Theme
                setting_ColorTheme.SelectedIndex = Convert.ToInt32(AppVariables.ApplicationSettings["ColorTheme"]);

                //Item Scroll Direction
                setting_ItemScrollDirection.SelectedIndex = Convert.ToInt32(AppVariables.ApplicationSettings["ItemScrollDirection"]);

                //Adjust Font Size
                setting_AdjustFontSize.Value = Convert.ToInt32(AppVariables.ApplicationSettings["AdjustFontSize"]);

                //Item list view styles
                ComboBoxImageList ComboTitleImageText = new ComboBoxImageList();
                ComboTitleImageText.Image = await AVImage.LoadBitmapImage("ms-appx:///Assets/ListStyles/0.png", false);
                ComboTitleImageText.Title = "Title, image and text";
                setting_ListViewStyle.Items.Add(ComboTitleImageText);

                ComboBoxImageList ComboTitleImage = new ComboBoxImageList();
                ComboTitleImage.Image = await AVImage.LoadBitmapImage("ms-appx:///Assets/ListStyles/1.png", false);
                ComboTitleImage.Title = "Title and image";
                setting_ListViewStyle.Items.Add(ComboTitleImage);

                ComboBoxImageList ComboTitleText = new ComboBoxImageList();
                ComboTitleText.Image = await AVImage.LoadBitmapImage("ms-appx:///Assets/ListStyles/2.png", false);
                ComboTitleText.Title = "Title and text";
                setting_ListViewStyle.Items.Add(ComboTitleText);

                ComboBoxImageList ComboTitle = new ComboBoxImageList();
                ComboTitle.Image = await AVImage.LoadBitmapImage("ms-appx:///Assets/ListStyles/3.png", false);
                ComboTitle.Title = "Title only";
                setting_ListViewStyle.Items.Add(ComboTitle);

                //Item list view style
                setting_ListViewStyle.SelectedIndex = Convert.ToInt32(AppVariables.ApplicationSettings["ListViewStyle"]);
            }
            catch { }
        }

        private void SettingsSave()
        {
            try
            {
                //Disable Landscape Display
                setting_DisableLandscapeDisplay.Click += (sender, e) =>
                {
                    CheckBox CheckBox = (CheckBox)sender;
                    AppVariables.ApplicationSettings["DisableLandscapeDisplay"] = (bool)CheckBox.IsChecked;

                    //Adjust screen rotation
                    AppAdjust.AdjustScreenRotation();
                };

                //Color Theme
                setting_ColorTheme.SelectionChanged += (sender, e) =>
                {
                    ComboBox ComboBox = (ComboBox)sender;
                    AppVariables.ApplicationSettings["ColorTheme"] = ComboBox.SelectedIndex;

                    //Adjust the color theme
                    AppAdjust.AdjustColorTheme();
                };

                //Item Scroll Direction
                setting_ItemScrollDirection.SelectionChanged += (sender, e) =>
                {
                    ComboBox ComboBox = (ComboBox)sender;
                    AppVariables.ApplicationSettings["ItemScrollDirection"] = ComboBox.SelectedIndex;

                    //Adjust the scrolling direction
                    EventAdjustItemsScrollingDirection(ComboBox.SelectedIndex);
                };

                //Adjust Font Size
                setting_AdjustFontSize.ValueChanged += (sender, e) =>
                {
                    Slider Slider = (Slider)sender;
                    AppVariables.ApplicationSettings["AdjustFontSize"] = Slider.Value.ToString();

                    //Adjust the font sizes
                    AppAdjust.AdjustFontSizes();
                };

                //Item list view style
                setting_ListViewStyle.SelectionChanged += (sender, e) =>
                {
                    ComboBox ComboBox = (ComboBox)sender;
                    AppVariables.ApplicationSettings["ListViewStyle"] = ComboBox.SelectedIndex;

                    //Adjust the list style
                    EventChangeListViewStyle(ComboBox.SelectedIndex);
                };
            }
            catch { }
        }
    }
}