﻿using ArnoldVinkCode;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NewsScroll
{
    public partial class MessagePopup : UserControl
    {
        //Popup Variables
        private static bool vPopupDone = false;
        private static int vPopupResult = 0;

        //Initialize popup
        public MessagePopup() { this.InitializeComponent(); }

        //Open the popup
        public async Task<int> Popup(string Question, string Description, string Answer1, string Answer2, string Answer3, string Answer4, string Answer5, bool ShowCancel)
        {
            TaskCompletionSource<int> TaskResult = new TaskCompletionSource<int>();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    //Set messagebox question content
                    grid_MessageBox_Text.Text = Question;
                    if (!string.IsNullOrWhiteSpace(Description))
                    {
                        grid_MessageBox_Description.Text = Description;
                        grid_MessageBox_Description.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid_MessageBox_Description.Text = "";
                        grid_MessageBox_Description.Visibility = Visibility.Collapsed;
                    }
                    if (!string.IsNullOrWhiteSpace(Answer1))
                    {
                        grid_MessageBox_Btn1.Content = Answer1;
                        grid_MessageBox_Btn1.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid_MessageBox_Btn1.Content = "";
                        grid_MessageBox_Btn1.Visibility = Visibility.Collapsed;
                    }
                    if (!string.IsNullOrWhiteSpace(Answer2))
                    {
                        grid_MessageBox_Btn2.Content = Answer2;
                        grid_MessageBox_Btn2.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid_MessageBox_Btn2.Content = "";
                        grid_MessageBox_Btn2.Visibility = Visibility.Collapsed;
                    }
                    if (!string.IsNullOrWhiteSpace(Answer3))
                    {
                        grid_MessageBox_Btn3.Content = Answer3;
                        grid_MessageBox_Btn3.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid_MessageBox_Btn3.Content = "";
                        grid_MessageBox_Btn3.Visibility = Visibility.Collapsed;
                    }
                    if (!string.IsNullOrWhiteSpace(Answer4))
                    {
                        grid_MessageBox_Btn4.Content = Answer4;
                        grid_MessageBox_Btn4.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid_MessageBox_Btn4.Content = "";
                        grid_MessageBox_Btn4.Visibility = Visibility.Collapsed;
                    }
                    if (!string.IsNullOrWhiteSpace(Answer5))
                    {
                        grid_MessageBox_Btn5.Content = Answer5;
                        grid_MessageBox_Btn5.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid_MessageBox_Btn5.Content = "";
                        grid_MessageBox_Btn5.Visibility = Visibility.Collapsed;
                    }
                    if (ShowCancel)
                    {
                        grid_MessageBox_BtnCancel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grid_MessageBox_BtnCancel.Visibility = Visibility.Collapsed;
                    }

                    //Reset messagebox variables
                    vPopupResult = 0;
                    vPopupDone = false;

                    //Open the popup
                    Grid gridPopup = AppVariables.FindPageGridPopup();
                    if (gridPopup == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Popup cannot be opened, no grid found.");
                        return;
                    }
                    gridPopup.Children.Add(this);

                    //Focus on the popup
                    grid_MessageBox_Btn1.Focus(FocusState.Programmatic);

                    //Wait for user messagebox input
                    while (vPopupResult == 0 && !vPopupDone) { await Task.Delay(10); }

                    //Close the popup
                    gridPopup.Children.Remove(this);
                }
                catch { }
                TaskResult.SetResult(vPopupResult);
            });
            return await TaskResult.Task;
        }

        //Set MessageBox Popup Result
        void grid_MessageBox_Btn1_Click(object sender, RoutedEventArgs e) { vPopupResult = 1; }
        void grid_MessageBox_Btn2_Click(object sender, RoutedEventArgs e) { vPopupResult = 2; }
        void grid_MessageBox_Btn3_Click(object sender, RoutedEventArgs e) { vPopupResult = 3; }
        void grid_MessageBox_Btn4_Click(object sender, RoutedEventArgs e) { vPopupResult = 4; }
        void grid_MessageBox_Btn5_Click(object sender, RoutedEventArgs e) { vPopupResult = 5; }
        void grid_MessageBox_BtnCancel_Click(object sender, RoutedEventArgs e) { vPopupDone = true; }

        //Monitor the application size
        private double PreviousLayoutWidth = 0;
        private double PreviousLayoutHeight = 0;
        private void OnLayoutUpdated(object sender, object e)
        {
            try
            {
                Rect ScreenResolution = AVFunctions.AppWindowResolution();
                double NewLayoutWidth = ScreenResolution.Width;
                double NewLayoutHeight = ScreenResolution.Height;
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