﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NewsScroll
{
    public partial class MessagePopup : ContentPage
    {
        //Window Initialize
        public MessagePopup()
        {
            try
            {
                InitializeComponent();
            }
            catch
            {
                Debug.WriteLine("Failed to initialize messagebox, check app.xaml styles.");
            }
        }

        //Popup Variables
        private bool vPopupDone = false;
        private string vPopupResult = string.Empty;

        //Wait for result
        private async Task<string> WaitResult(string Question, string Description, List<string> Answers)
        {
            try
            {
                //Set messagebox question
                if (!string.IsNullOrWhiteSpace(Question))
                {
                    grid_MessageBox_Question.Text = Question;
                    grid_MessageBox_Question.IsVisible = true;
                    grid_MessageBox_Border.IsVisible = true;
                }
                else
                {
                    grid_MessageBox_Question.Text = string.Empty;
                    grid_MessageBox_Question.IsVisible = false;
                    grid_MessageBox_Border.IsVisible = false;
                }

                //Set messagebox description
                if (!string.IsNullOrWhiteSpace(Description))
                {
                    grid_MessageBox_Description.Text = Description;
                    grid_MessageBox_Description.IsVisible = true;
                }
                else
                {
                    grid_MessageBox_Description.Text = string.Empty;
                    grid_MessageBox_Description.IsVisible = false;
                }

                //Set the messagebox answers
                listbox_MessageBox.ItemsSource = Answers;
                listbox_MessageBox.SelectedItem = 0;

                //Reset popup variables
                vPopupResult = string.Empty;
                vPopupDone = false;

                //Wait for user messagebox input
                while (vPopupResult == string.Empty && !vPopupDone && this.IsVisible) { await Task.Delay(500); }

                //Close the messagebox popup
               await Close();
            }
            catch { }
            return vPopupResult;
        }

        //Show the popup
        public static async Task<string> Popup(string Question, string Description, List<string> Answers)
        {
            try
            {
                string messageResult = null;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    MessagePopup newPopup = new MessagePopup();
                    App.NavigateToPage(newPopup, true, true);
                    messageResult = await newPopup.WaitResult(Question, Description, Answers);
                });
                while (messageResult == null) { await Task.Delay(100); }
                return messageResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create message popup: " + ex.Message);
                return string.Empty;
            }
        }

        //Close the popup
        public async Task Close()
        {
            try
            {
                vPopupDone = true;
                await Application.Current.MainPage.Navigation.PopModalAsync(false);
            }
            catch { }
        }

        //Set the popup result
        private void listbox_MessageBox_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                string tappedString = e.Item.ToString();
                vPopupResult = tappedString;
                vPopupDone = true;
                Debug.WriteLine("Selected messagebox answer: " + vPopupResult);
            }
            catch { }
        }
    }
}