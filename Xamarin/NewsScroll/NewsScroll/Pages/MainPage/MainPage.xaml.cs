using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace NewsScroll
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Page_Loaded();
        }

        private void Page_Loaded()
        {
            try
            {
                List<string> test = new List<string>();
                test.Add("News1");
                test.Add("News2");
                test.Add("News3");
                test.Add("News4");
                ListView_Items.ItemsSource = test;
            }
            catch { }
        }

        private void iconReadAll_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("iconReadAll_Clicked");
        }

        private void iconRefresh_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("iconRefresh_Clicked");
        }

        private void button_StatusCurrentItem_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("button_StatusCurrentItem_Clicked");
        }

        private void iconMenu_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("iconMenu_Clicked");
            if (grid_PopupMenu.IsVisible)
            {
                grid_PopupMenu.IsVisible = false;
            }
            else
            {
                grid_PopupMenu.IsVisible = true;
            }
        }

        private void iconStar_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("iconStar_Clicked");
        }

        private void iconSearch_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("iconSearch_Clicked");
        }

        private void iconApi_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("iconApi_Clicked");
        }

        private void iconPersonalize_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("iconPersonalize_Clicked");
        }

        private void iconSettings_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("iconSettings_Clicked");
        }
    }
}