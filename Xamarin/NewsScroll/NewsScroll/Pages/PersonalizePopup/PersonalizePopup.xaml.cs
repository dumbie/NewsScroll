﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.AppEvents.AppEvents;

namespace NewsScroll
{
    public partial class PersonalizePopup : ContentPage
    {
        //Initialize popup
        public PersonalizePopup()
        {
            this.InitializeComponent();
            Page_Loaded();
        }

        private void Page_Loaded()
        {
            try
            {
                //Focus on the popup
                iconClose.Focus();

                //Load and set the settings
                SettingsLoad();
                SettingsSave();
            }
            catch { }
        }

        //Show the popup
        public static void Popup()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    PersonalizePopup newPopup = new PersonalizePopup();
                    App.NavigateToPage(newPopup, true, true);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create personalize popup: " + ex.Message);
            }
        }

        //Close the popup
        private async void iconClose_Tap(object sender, EventArgs e) { try { await Close(); } catch { } }
        public async Task Close()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PopModalAsync(false);
            }
            catch { }
        }

        private void SettingsLoad()
        {
            try
            {
                Debug.WriteLine("Loading application settings...");

                //Color Theme
                setting_ColorTheme.SelectedIndex = Convert.ToInt32(AppSettingLoad("ColorTheme"));

                //Item Scroll Direction
                setting_ListViewDirection.SelectedIndex = Convert.ToInt32(AppSettingLoad("ListViewDirection"));

                //Adjust Font Size
                int fontSizeValue = Convert.ToInt32(AppSettingLoad("AdjustFontSize"));
                setting_AdjustFontSize.Value = fontSizeValue;
                setting_AdjustFontSizeText.Text = "Adjust the displayed font size: " + fontSizeValue;

                ////Item list view styles
                //ComboBoxImageList ComboTitleImageText = new ComboBoxImageList();
                //ComboTitleImageText.Image = await AVImage.LoadBitmapImage("ms-appx:///Assets/ListStyles/0.png", false);
                //ComboTitleImageText.Title = "Title, image and text";
                //setting_ListViewStyle.Items.Add(ComboTitleImageText);

                //ComboBoxImageList ComboTitleImage = new ComboBoxImageList();
                //ComboTitleImage.Image = await AVImage.LoadBitmapImage("ms-appx:///Assets/ListStyles/1.png", false);
                //ComboTitleImage.Title = "Title and image";
                //setting_ListViewStyle.Items.Add(ComboTitleImage);

                //ComboBoxImageList ComboTitleText = new ComboBoxImageList();
                //ComboTitleText.Image = await AVImage.LoadBitmapImage("ms-appx:///Assets/ListStyles/2.png", false);
                //ComboTitleText.Title = "Title and text";
                //setting_ListViewStyle.Items.Add(ComboTitleText);

                //ComboBoxImageList ComboTitle = new ComboBoxImageList();
                //ComboTitle.Image = await AVImage.LoadBitmapImage("ms-appx:///Assets/ListStyles/3.png", false);
                //ComboTitle.Title = "Title only";
                //setting_ListViewStyle.Items.Add(ComboTitle);

                //Item list view style
                setting_ListViewStyle.SelectedIndex = Convert.ToInt32(AppSettingLoad("ListViewStyle"));
            }
            catch { }
        }

        private void SettingsSave()
        {
            try
            {
                //Color Theme
                setting_ColorTheme.SelectedIndexChanged += async (sender, e) =>
                {
                    Picker ComboBox = (Picker)sender;
                    await AppSettingSave("ColorTheme", ComboBox.SelectedIndex);

                    //Adjust the color theme
                    AppAdjust.AdjustColorTheme();
                };

                //Item Scroll Direction
                setting_ListViewDirection.SelectedIndexChanged += async (sender, e) =>
                {
                    Picker ComboBox = (Picker)sender;
                    await AppSettingSave("ListViewDirection", ComboBox.SelectedIndex);

                    //Adjust the scrolling direction
                    if (EventChangeListViewDirection != null)
                    {
                        EventChangeListViewDirection(ComboBox.SelectedIndex);
                    }
                };

                //Adjust Font Size
                setting_AdjustFontSize.ValueChanged += async (sender, e) =>
                {
                    Slider Slider = (Slider)sender;
                    await AppSettingSave("AdjustFontSize", Convert.ToInt32(Slider.Value).ToString());
                    setting_AdjustFontSizeText.Text = "Adjust the displayed font size: " + Convert.ToInt32(Slider.Value);

                    //Adjust the font sizes
                    AppAdjust.AdjustFontSizes();
                };

                //Item list view style
                setting_ListViewStyle.SelectedIndexChanged += async (sender, e) =>
                {
                    Picker ComboBox = (Picker)sender;
                    await AppSettingSave("ListViewStyle", ComboBox.SelectedIndex);

                    //Adjust the list style
                    if (EventChangeListViewStyle != null)
                    {
                        EventChangeListViewStyle(ComboBox.SelectedIndex);
                    }
                };
            }
            catch { }
        }
    }
}