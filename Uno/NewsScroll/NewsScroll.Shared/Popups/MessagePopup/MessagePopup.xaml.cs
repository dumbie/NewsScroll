using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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
        public async Task<int> OpenPopup(string Question, string Description, string Answer1, string Answer2, string Answer3, string Answer4, string Answer5, bool ShowCancel)
        {
            TaskCompletionSource<int> TaskResult = new TaskCompletionSource<int>();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await OpenPopupRaw(Question, Description, Answer1, Answer2, Answer3, Answer4, Answer5, ShowCancel);
                TaskResult.SetResult(vPopupResult);
            });
            return await TaskResult.Task;
        }

        //Open the popup
        private async Task<int> OpenPopupRaw(string Question, string Description, string Answer1, string Answer2, string Answer3, string Answer4, string Answer5, bool ShowCancel)
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
                popup_Main.IsOpen = true;

                //Focus on the popup
                grid_MessageBox_Btn1.Focus(FocusState.Programmatic);

                //Wait for user messagebox input
                while (vPopupResult == 0 && !vPopupDone) { await Task.Delay(10); }

                //Close the popup
                popup_Main.IsOpen = false;
            }
            catch { }
            return vPopupResult;
        }

        //Set MessageBox Popup Result
        void grid_MessageBox_Btn1_Click(object sender, RoutedEventArgs e) { vPopupResult = 1; }
        void grid_MessageBox_Btn2_Click(object sender, RoutedEventArgs e) { vPopupResult = 2; }
        void grid_MessageBox_Btn3_Click(object sender, RoutedEventArgs e) { vPopupResult = 3; }
        void grid_MessageBox_Btn4_Click(object sender, RoutedEventArgs e) { vPopupResult = 4; }
        void grid_MessageBox_Btn5_Click(object sender, RoutedEventArgs e) { vPopupResult = 5; }
        void grid_MessageBox_BtnCancel_Click(object sender, RoutedEventArgs e) { vPopupDone = true; }
    }
}