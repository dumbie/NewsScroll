using ArnoldVinkCode;
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
        public static MessagePopup vMessagePopup = null;
        private static bool vMessageBoxPopupCancelled = false;
        private static int vMessageBoxPopupResult = 0;

        //Initialize popup
        public MessagePopup() { this.InitializeComponent(); }

        //Open and close the popup
        public static async Task<int> Popup(string Question, string Description, string Answer1, string Answer2, string Answer3, string Answer4, string Answer5, bool ShowCancel)
        {
            TaskCompletionSource<int> TaskResult = new TaskCompletionSource<int>();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    if (vMessagePopup == null)
                    {
                        vMessagePopup = new MessagePopup();

                        //Set messagebox question content
                        vMessagePopup.grid_MessageBox_Text.Text = Question;
                        if (!string.IsNullOrWhiteSpace(Description))
                        {
                            vMessagePopup.grid_MessageBox_Description.Text = Description;
                            vMessagePopup.grid_MessageBox_Description.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vMessagePopup.grid_MessageBox_Description.Text = "";
                            vMessagePopup.grid_MessageBox_Description.Visibility = Visibility.Collapsed;
                        }
                        if (!string.IsNullOrWhiteSpace(Answer1))
                        {
                            vMessagePopup.grid_MessageBox_Btn1.Content = Answer1;
                            vMessagePopup.grid_MessageBox_Btn1.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vMessagePopup.grid_MessageBox_Btn1.Content = "";
                            vMessagePopup.grid_MessageBox_Btn1.Visibility = Visibility.Collapsed;
                        }
                        if (!string.IsNullOrWhiteSpace(Answer2))
                        {
                            vMessagePopup.grid_MessageBox_Btn2.Content = Answer2;
                            vMessagePopup.grid_MessageBox_Btn2.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vMessagePopup.grid_MessageBox_Btn2.Content = "";
                            vMessagePopup.grid_MessageBox_Btn2.Visibility = Visibility.Collapsed;
                        }
                        if (!string.IsNullOrWhiteSpace(Answer3))
                        {
                            vMessagePopup.grid_MessageBox_Btn3.Content = Answer3;
                            vMessagePopup.grid_MessageBox_Btn3.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vMessagePopup.grid_MessageBox_Btn3.Content = "";
                            vMessagePopup.grid_MessageBox_Btn3.Visibility = Visibility.Collapsed;
                        }
                        if (!string.IsNullOrWhiteSpace(Answer4))
                        {
                            vMessagePopup.grid_MessageBox_Btn4.Content = Answer4;
                            vMessagePopup.grid_MessageBox_Btn4.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vMessagePopup.grid_MessageBox_Btn4.Content = "";
                            vMessagePopup.grid_MessageBox_Btn4.Visibility = Visibility.Collapsed;
                        }
                        if (!string.IsNullOrWhiteSpace(Answer5))
                        {
                            vMessagePopup.grid_MessageBox_Btn5.Content = Answer5;
                            vMessagePopup.grid_MessageBox_Btn5.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vMessagePopup.grid_MessageBox_Btn5.Content = "";
                            vMessagePopup.grid_MessageBox_Btn5.Visibility = Visibility.Collapsed;
                        }
                        if (ShowCancel)
                        {
                            vMessagePopup.grid_MessageBox_BtnCancel.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vMessagePopup.grid_MessageBox_BtnCancel.Visibility = Visibility.Collapsed;
                        }

                        //Reset messagebox variables
                        vMessageBoxPopupResult = 0;
                        vMessageBoxPopupCancelled = false;

                        //Open the popup
                        vMessagePopup.popup_Main.IsOpen = true;

                        //Focus on the popup
                        vMessagePopup.grid_MessageBox_Btn1.Focus(FocusState.Programmatic);

                        //Wait for user messagebox input
                        while (vMessageBoxPopupResult == 0 && !vMessageBoxPopupCancelled) { await Task.Delay(10); }

                        //Close the popup
                        vMessagePopup.popup_Main.IsOpen = false;
                        vMessagePopup = null;

                        TaskResult.SetResult(vMessageBoxPopupResult);
                    }
                }
                catch { TaskResult.SetResult(0); }
            });
            return await TaskResult.Task;
        }

        //Set MessageBox Popup Result
        void grid_MessageBox_Btn1_Click(object sender, RoutedEventArgs e) { vMessageBoxPopupResult = 1; }
        void grid_MessageBox_Btn2_Click(object sender, RoutedEventArgs e) { vMessageBoxPopupResult = 2; }
        void grid_MessageBox_Btn3_Click(object sender, RoutedEventArgs e) { vMessageBoxPopupResult = 3; }
        void grid_MessageBox_Btn4_Click(object sender, RoutedEventArgs e) { vMessageBoxPopupResult = 4; }
        void grid_MessageBox_Btn5_Click(object sender, RoutedEventArgs e) { vMessageBoxPopupResult = 5; }
        void grid_MessageBox_BtnCancel_Click(object sender, RoutedEventArgs e) { vMessageBoxPopupCancelled = true; }

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