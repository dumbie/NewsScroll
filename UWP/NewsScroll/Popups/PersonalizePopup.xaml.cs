using ArnoldVinkCode;
using NewsScroll.Classes;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static NewsScroll.Events.Events;
using static NewsScroll.Startup.Startup;

namespace NewsScroll
{
    public partial class PersonalizePopup : UserControl
    {
        //Popup Variables
        public static bool PopupIsOpen = false;

        //Initialize popup
        public PersonalizePopup() { this.InitializeComponent(); }

        //Open the popup
        public async Task OpenPopup()
        {
            try
            {
                if (PopupIsOpen)
                {
                    Debug.WriteLine("The popup is already open...");
                    return;
                }

                //Open the popup
                popup_Main.IsOpen = true;
                PopupIsOpen = true;

                //Focus on the popup
                iconClose.Focus(FocusState.Programmatic);

                //Load and set the settings
                await SettingsLoad();
                SettingsSave();

                //Show the switch fullscreen mode
                if (!AVFunctions.DevMobile()) { button_SwitchScreenMode.Visibility = Visibility.Visible; }
            }
            catch { }
        }

        //Close the popup
        private void iconClose_Tap(object sender, RoutedEventArgs e) { try { Close(); } catch { } }
        public void Close()
        {
            try
            {
                //Close the popup
                popup_Main.IsOpen = false;
                PopupIsOpen = false;
            }
            catch { }
        }

        private async Task SettingsLoad()
        {
            try
            {
                Debug.WriteLine("Loading application settings...");

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
                    if ((bool)CheckBox.IsChecked)
                    {
                        AppVariables.ApplicationSettings["DisableLandscapeDisplay"] = true;
                        DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                    }
                    else
                    {
                        AppVariables.ApplicationSettings["DisableLandscapeDisplay"] = false;
                        DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.Landscape;
                    }
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

        private void button_SwitchScreenMode_Tap(object sender, RoutedEventArgs e)
        {
            try { SwitchScreenMode(); } catch { }
        }

        //Monitor the application size
        private Double PreviousLayoutWidth = 0;
        private Double PreviousLayoutHeight = 0;
        private void OnLayoutUpdated(object sender, object e)
        {
            try
            {
                Rect ScreenResolution = AVFunctions.AppWindowResolution();
                Double NewLayoutWidth = ScreenResolution.Width;
                Double NewLayoutHeight = ScreenResolution.Height;
                if (NewLayoutWidth != PreviousLayoutWidth || NewLayoutHeight != PreviousLayoutHeight)
                {
                    PreviousLayoutWidth = NewLayoutWidth;
                    PreviousLayoutHeight = NewLayoutHeight;

                    grid_Main.Width = NewLayoutWidth;
                    grid_Main.Height = NewLayoutHeight;
                }
            }
            catch { }
        }
    }
}