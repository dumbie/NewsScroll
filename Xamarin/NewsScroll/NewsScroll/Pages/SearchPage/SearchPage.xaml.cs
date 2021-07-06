using ArnoldVinkCode;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;
using static NewsScroll.Api.Api;
using static NewsScroll.AppEvents.AppEvents;
using static NewsScroll.AppVariables;
using static NewsScroll.Lists.Lists;

namespace NewsScroll
{
    public partial class SearchPage : ContentPage
    {
        //Page Variables
        private int vPreviousScrollItem = 0;

        public SearchPage()
        {
            InitializeComponent();
            Page_Loaded();
        }

        //Application Navigation
        private async void Page_Loaded()
        {
            try
            {
                //Check the set account
                if (!CheckAccount())
                {
                    await CleanupPageResources();
                    App.NavigateToPage(new SettingsPage(), true, false);
                    return;
                }

                //Register page events
                RegisterPageEvents();

                //Adjust the scrolling direction
                //await AdjustItemsScrollingDirection(Convert.ToInt32(AppVariables.ApplicationSettings["ItemScrollDirection"]));

                //Adjust the list view style
                //ChangeListViewStyle(Convert.ToInt32(AppVariables.ApplicationSettings["ListViewStyle"]));

                //Adjust the swiping direction
                SwipeBarAdjust();

                //Bind list to ListView
                listview_Items.ItemsSource = List_SearchItems;
                combobox_FeedSelection.ItemsSource = List_FeedSelect;

                //Load the search history
                await LoadSearchHistory();

                //Load feeds into selector
                await LoadSelectionFeeds(null, null, false, true);
                combobox_FeedSelection.SelectedIndex = 0;

                //Focus on the text box to open keyboard
                txtbox_Search.IsEnabled = false;
                txtbox_Search.IsEnabled = true;
                txtbox_Search.Focus();
            }
            catch { }
        }

        //Register page events
        private void RegisterPageEvents()
        {
            try
            {
                //Register popup and load events
                EventProgressDisableUI += new DelegateProgressDisableUI(ProgressDisableUI);
                EventProgressEnableUI += new DelegateProgressEnableUI(ProgressEnableUI);
                EventHideShowHeader += new DelegateHideShowHeader(HideShowHeader);
                EventHideProgressionStatus += new DelegateHideProgressionStatus(HideProgressionStatus);
                //EventAdjustItemsScrollingDirection += new DelegateAdjustItemsScrollingDirection(AdjustItemsScrollingDirection);
                //EventChangeListViewStyle += new DelegateChangeListViewStyle(ChangeListViewStyle);

                //Register ListView events
                listview_Items.ItemTapped += EventsListView.listview_Items_Tapped;

                //Register ListView scroll viewer
                listview_Items.ItemAppearing += ScrollViewer_ItemAppearing;
                listview_Items.ItemDisappearing += ScrollViewer_ItemDisappearing;

                //Register item status count events
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += button_StatusCurrentItem_Tapped;
                button_StatusCurrentItem.GestureRecognizers.Add(tapGestureRecognizer);

                //Monitor user touch swipe
                if (!(bool)AppSettingLoad("DisableSwipeActions"))
                {
                    SwipeBarAdjust();
                }
            }
            catch { }
        }

        //Disable page events
        private void DisablePageEvents()
        {
            try
            {
                //Register popup and load events
                EventProgressDisableUI -= new DelegateProgressDisableUI(ProgressDisableUI);
                EventProgressEnableUI -= new DelegateProgressEnableUI(ProgressEnableUI);
                EventHideShowHeader -= new DelegateHideShowHeader(HideShowHeader);
                EventHideProgressionStatus -= new DelegateHideProgressionStatus(HideProgressionStatus);
                //EventAdjustItemsScrollingDirection -= new DelegateAdjustItemsScrollingDirection(AdjustItemsScrollingDirection);
                //EventChangeListViewStyle -= new DelegateChangeListViewStyle(ChangeListViewStyle);

                //Register ListView events
                listview_Items.ItemTapped -= EventsListView.listview_Items_Tapped;

                //Register ListView scroll viewer
                listview_Items.ItemAppearing -= ScrollViewer_ItemAppearing;
                listview_Items.ItemDisappearing -= ScrollViewer_ItemDisappearing;

                //Monitor user touch swipe
                grid_SwipeBar.GestureRecognizers.Clear();
            }
            catch { }
        }

        //Cleanup page resources
        async Task CleanupPageResources()
        {
            try
            {
                Debug.WriteLine("Clearing page resources...");

                DisablePageEvents();
                vSearchFeed = null;

                listview_Items.ItemsSource = null;
                combobox_FeedSelection.ItemsSource = null;

                await ClearObservableCollection(List_Feeds);
                await ClearObservableCollection(List_FeedSelect);
                await ClearObservableCollection(List_NewsItems);
                await ClearObservableCollection(List_SearchItems);
                await ClearObservableCollection(List_StarredItems);
            }
            catch { }
        }

        //Hide the progression status
        private void HideProgressionStatus()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    txt_NewsScrollInfo.IsVisible = false;
                }
                catch { }
            });
        }

        //Item status scroll events
        private async void button_StatusCurrentItem_Tapped(object sender, EventArgs e)
        {
            try
            {
                bool Scrolled = await EventsItemStatus.ListViewScroller(listview_Items, AppVariables.CurrentViewItemsCount, vPreviousScrollItem);
                if (Scrolled)
                {
                    vPreviousScrollItem = AppVariables.CurrentViewItemsCount;
                }
            }
            catch { }
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
                        //Disable Search
                        txtbox_Search.IsEnabled = false;
                        txtbox_Search.Opacity = 0.30;

                        //Disable content
                        button_StatusCurrentItem.IsEnabled = false;
                        button_StatusCurrentItem.Opacity = 0.30;

                        //Disable buttons
                        iconApi.IsEnabled = false;
                        iconApi.Opacity = 0.30;
                        iconNews.IsEnabled = false;
                        iconNews.Opacity = 0.30;
                        iconStar.IsEnabled = false;
                        iconStar.Opacity = 0.30;
                        iconSettings.IsEnabled = false;
                        iconSettings.Opacity = 0.30;
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

                    //Enable Search
                    txtbox_Search.IsEnabled = true;
                    txtbox_Search.Opacity = 1;

                    //Enable content
                    button_StatusCurrentItem.IsEnabled = true;
                    button_StatusCurrentItem.Opacity = 1;

                    //Enable buttons
                    iconApi.IsEnabled = true;
                    iconApi.Opacity = 1;
                    iconNews.IsEnabled = true;
                    iconNews.Opacity = 1;
                    iconStar.IsEnabled = true;
                    iconStar.Opacity = 1;
                    iconSettings.IsEnabled = true;
                    iconSettings.Opacity = 1;
                }
                catch { }
                AppVariables.BusyApplication = false;
            });
        }

        //User Interface - Buttons
        async void iconStar_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.NavigateToPage(new StarredPage(), true, false);
            }
            catch { }
        }

        async void iconNews_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.NavigateToPage(new NewsPage(), true, false);
            }
            catch { }
        }

        async void iconApi_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.NavigateToPage(new ApiPage(), true, false);
            }
            catch { }
        }

        private async void iconPersonalize_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);
                //fix
                //PersonalizePopup personalizePopup = new PersonalizePopup();
                //await personalizePopup.OpenPopup();
            }
            catch { }
        }

        async void iconSettings_Tap(object sender, EventArgs e)
        {
            try
            {
                HideShowMenu(true);

                //Wait for busy application
                await ApiUpdate.WaitForBusyApplication();

                await CleanupPageResources();
                App.NavigateToPage(new SettingsPage(), true, false);
            }
            catch { }
        }

        private async void txtbox_Search_SearchButtonPressed(object sender, EventArgs e)
        {
            try
            {
                //Set search or selection string
                vSearchTerm = txtbox_Search.Text;
                vSearchTerm = AVFunctions.StringRemoveEnd(vSearchTerm, " ");
                txtbox_Search.Text = vSearchTerm;

                //Check searchbox input
                if (string.IsNullOrWhiteSpace(vSearchTerm))
                {
                    if (!List_SearchItems.Any())
                    {
                        txt_NewsScrollInfo.Text = "Please select a feed to search in and enter a search term to look for...";
                        txt_NewsScrollInfo.IsVisible = true;
                    }

                    //Focus on the text box to open keyboard
                    txtbox_Search.IsEnabled = false;
                    txtbox_Search.IsEnabled = true;
                    txtbox_Search.Focus();
                    return;
                }

                //Search for the items
                await LoadItems();
            }
            catch { }
        }
    }
}