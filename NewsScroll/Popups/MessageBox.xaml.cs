using ArnoldVinkCode;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArnoldVinkMessageBox
{
    public partial class AVMessageBox : UserControl
    {
        //Popup Variables
        public static AVMessageBox vAVMessageBox = null;
        private static bool vMessageBoxPopupCancelled = false;
        private static Int32 vMessageBoxPopupResult = 0;

        //Initialize popup
        public AVMessageBox() { this.InitializeComponent(); }

        //Open and close the popup
        public static async Task<Int32> Popup(string Question, string Description, string Answer1, string Answer2, string Answer3, string Answer4, bool ShowCancel)
        {
            TaskCompletionSource<Int32> TaskResult = new TaskCompletionSource<Int32>();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    if (vAVMessageBox == null)
                    {
                        vAVMessageBox = new AVMessageBox();

                        //Set messagebox question content
                        vAVMessageBox.grid_MessageBox_Text.Text = Question;
                        if (!String.IsNullOrWhiteSpace(Description))
                        {
                            vAVMessageBox.grid_MessageBox_Description.Text = Description;
                            vAVMessageBox.grid_MessageBox_Description.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vAVMessageBox.grid_MessageBox_Description.Text = "";
                            vAVMessageBox.grid_MessageBox_Description.Visibility = Visibility.Collapsed;
                        }
                        if (!String.IsNullOrWhiteSpace(Answer1))
                        {
                            vAVMessageBox.grid_MessageBox_Btn1.Content = Answer1;
                            vAVMessageBox.grid_MessageBox_Btn1.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vAVMessageBox.grid_MessageBox_Btn1.Content = "";
                            vAVMessageBox.grid_MessageBox_Btn1.Visibility = Visibility.Collapsed;
                        }
                        if (!String.IsNullOrWhiteSpace(Answer2))
                        {
                            vAVMessageBox.grid_MessageBox_Btn2.Content = Answer2;
                            vAVMessageBox.grid_MessageBox_Btn2.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vAVMessageBox.grid_MessageBox_Btn2.Content = "";
                            vAVMessageBox.grid_MessageBox_Btn2.Visibility = Visibility.Collapsed;
                        }
                        if (!String.IsNullOrWhiteSpace(Answer3))
                        {
                            vAVMessageBox.grid_MessageBox_Btn3.Content = Answer3;
                            vAVMessageBox.grid_MessageBox_Btn3.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vAVMessageBox.grid_MessageBox_Btn3.Content = "";
                            vAVMessageBox.grid_MessageBox_Btn3.Visibility = Visibility.Collapsed;
                        }
                        if (!String.IsNullOrWhiteSpace(Answer4))
                        {
                            vAVMessageBox.grid_MessageBox_Btn4.Content = Answer4;
                            vAVMessageBox.grid_MessageBox_Btn4.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vAVMessageBox.grid_MessageBox_Btn4.Content = "";
                            vAVMessageBox.grid_MessageBox_Btn4.Visibility = Visibility.Collapsed;
                        }
                        if (ShowCancel)
                        {
                            vAVMessageBox.grid_MessageBox_BtnCancel.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            vAVMessageBox.grid_MessageBox_BtnCancel.Visibility = Visibility.Collapsed;
                        }

                        //Reset messagebox variables
                        vMessageBoxPopupResult = 0;
                        vMessageBoxPopupCancelled = false;

                        //Open the popup
                        vAVMessageBox.popup_Main.IsOpen = true;

                        //Focus on the popup
                        vAVMessageBox.grid_MessageBox_Btn1.Focus(FocusState.Programmatic);

                        //Wait for user messagebox input
                        while (vMessageBoxPopupResult == 0 && !vMessageBoxPopupCancelled) { await Task.Delay(10); }

                        //Close the popup
                        vAVMessageBox.popup_Main.IsOpen = false;
                        vAVMessageBox = null;

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
        void grid_MessageBox_BtnCancel_Click(object sender, RoutedEventArgs e) { vMessageBoxPopupCancelled = true; }

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