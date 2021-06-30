using NewsScroll.Classes;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using static NewsScroll.Api.Api;
using static NewsScroll.AppVariables;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class NewsPage : ContentPage
    {
        //Page Variables
        //0 = All items
        //1 = All read items
        //2 = All unread items
        public static Feeds vCurrentLoadingFeedFolder = null;
        private static Int32 vPreviousScrollItem = 0;

        public NewsPage()
        {
            InitializeComponent();
            Page_Loaded();
        }

        private async void Page_Loaded()
        {
            try
            {
                //Check settings
                await SettingsPage.SettingsCheck();

                //Check the set account
                if (!CheckAccount())
                {
                    await Navigation.PushModalAsync(new SettingsPage());
                    return;
                }

                //Show page status
                ProgressDisableUI("Preparing news page...", true);

                //Bind lists
                ListView_Items.ItemsSource = List_NewsItems;
                combobox_FeedSelection.ItemsSource = List_FeedSelect;

                //Load all the feeds
                await LoadFeeds();

                //Load all the items
                await LoadItems();
            }
            catch { }
        }

        //Load all the feeds
        private async Task LoadFeeds()
        {
            //Clear all select feeds
            await ClearObservableCollection(List_FeedSelect);

            List_FeedSelect.Add("Feed title");
            List_FeedSelect.Add("Feed title");
            List_FeedSelect.Add("Feed title");
            List_FeedSelect.Add("Feed title");
        }

        //Load all the items
        private async Task LoadItems()
        {
            //Clear all items and reset load count
            await ClearObservableCollection(List_NewsItems);

            Image image = new Image { Source = "https://www.google.com/images/branding/googlelogo/2x/googlelogo_color_92x30dp.png" };
            Items testitem = new Items();
            testitem.item_title = "News title";
            testitem.item_image = image.Source;
            testitem.item_image_visibility = true;
            List_NewsItems.Add(testitem);

            ProgressEnableUI();
            txt_NewsScrollInfo.IsVisible = false;
        }

        //Progressbar/UI Status
        void ProgressDisableUI(string ProgressMsg, bool DisableInterface)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    AppVariables.BusyApplication = true;

                    //Enable progressbar
                    label_StatusApplication.Text = ProgressMsg;
                    grid_StatusApplication.IsVisible = true;

                    if (DisableInterface)
                    {
                        //Disable feed selection
                        combobox_FeedSelection.IsEnabled = false;
                        combobox_FeedSelection.Opacity = 0.30;

                        //Disable content
                        button_StatusCurrentItem.IsEnabled = false;
                        button_StatusCurrentItem.Opacity = 0.30;

                        //Disable buttons
                        iconApi.IsEnabled = false;
                        iconApi.Opacity = 0.30;
                        iconStar.IsEnabled = false;
                        iconStar.Opacity = 0.30;
                        iconSearch.IsEnabled = false;
                        iconSearch.Opacity = 0.30;
                        iconSettings.IsEnabled = false;
                        iconSettings.Opacity = 0.30;
                        iconRefresh.IsEnabled = false;
                        iconRefresh.Opacity = 0.30;
                        iconReadAll.IsEnabled = false;
                        iconReadAll.Opacity = 0.30;
                    }
                }
                catch { }
                AppVariables.BusyApplication = true;
            });
        }

        void ProgressEnableUI()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    //Disable progressbar
                    grid_StatusApplication.IsVisible = false;
                    label_StatusApplication.Text = string.Empty;

                    //Enable feed selection
                    combobox_FeedSelection.IsEnabled = true;
                    combobox_FeedSelection.Opacity = 1;

                    //Enable content
                    button_StatusCurrentItem.IsEnabled = true;
                    button_StatusCurrentItem.Opacity = 1;

                    //Enable buttons
                    iconApi.IsEnabled = true;
                    iconApi.Opacity = 1;
                    iconStar.IsEnabled = true;
                    iconStar.Opacity = 1;
                    iconSearch.IsEnabled = true;
                    iconSearch.Opacity = 1;
                    iconSettings.IsEnabled = true;
                    iconSettings.Opacity = 1;
                    iconRefresh.IsEnabled = true;
                    iconRefresh.Opacity = 1;
                    iconReadAll.IsEnabled = true;
                    iconReadAll.Opacity = 1;
                }
                catch { }
                AppVariables.BusyApplication = false;
            });
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


        private async Task<int> DoubleClickCheck()
        {
            try
            {
                vSingleTappedClickCount++;
                if (vSingleTappedClickCount == 1)
                {
                    await Task.Delay(300);
                    if (vSingleTappedClickCount == 1)
                    {
                        vSingleTappedClickCount = 0;
                        return 1;
                    }
                    else
                    {
                        vSingleTappedClickCount = 0;
                        return 2;
                    }
                }
            }
            catch { }
            return 0;
        }

        private async void iconMenu_Clicked(object sender, EventArgs e)
        {
            try
            {
                int doubleClick = await DoubleClickCheck();
                if (doubleClick == 1)
                {
                    Debug.WriteLine("iconMenu_Clicked Single");
                    StackLayout_Header.IsVisible = true;
                    if (grid_PopupMenu.IsVisible)
                    {
                        grid_PopupMenu.IsVisible = false;
                    }
                    else
                    {
                        grid_PopupMenu.IsVisible = true;
                    }
                }
                else if (doubleClick == 2)
                {
                    Debug.WriteLine("iconMenu_Clicked Double");
                    if (StackLayout_Header.IsVisible)
                    {
                        grid_PopupMenu.IsVisible = false;
                        StackLayout_Header.IsVisible = false;
                    }
                    else
                    {
                        StackLayout_Header.IsVisible = true;
                    }
                }
            }
            catch { }
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

        private async void iconSettings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new SettingsPage());
        }
    }
}