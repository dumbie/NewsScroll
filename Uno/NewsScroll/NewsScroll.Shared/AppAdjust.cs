using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NewsScroll
{
    class AppAdjust
    {
        //Adjust the Color Theme
        public static void AdjustColorTheme()
        {
            try
            {
                int SelectedTheme = Convert.ToInt32(AppVariables.ApplicationSettings["ColorTheme"]);
                System.Diagnostics.Debug.WriteLine("Adjusting the application theme to: " + SelectedTheme);

                //Fix
                //if (SelectedTheme == 0)
                //{
                //    SolidColorBrush ColorEnabled = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                //    SolidColorBrush ColorDisabled = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                //    ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ApplicationLightGrayBrush = ColorEnabled;
                //    ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ApplicationDarkGrayBrush = ColorDisabled;
                //    ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ElementRequestedTheme = ElementTheme.Light;
                //}
                //else if (SelectedTheme == 1)
                //{
                //    SolidColorBrush ColorEnabled = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                //    SolidColorBrush ColorDisabled = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                //    ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ApplicationLightGrayBrush = ColorEnabled;
                //    ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ApplicationDarkGrayBrush = ColorDisabled;
                //    ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ElementRequestedTheme = ElementTheme.Dark;
                //}
                //else
                //{
                //    if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                //    {
                //        SolidColorBrush ColorEnabled = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                //        SolidColorBrush ColorDisabled = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                //        ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ApplicationLightGrayBrush = ColorEnabled;
                //        ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ApplicationDarkGrayBrush = ColorDisabled;
                //        ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ElementRequestedTheme = ElementTheme.Dark;
                //    }
                //    else
                //    {
                //        SolidColorBrush ColorEnabled = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                //        SolidColorBrush ColorDisabled = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                //        ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ApplicationLightGrayBrush = ColorEnabled;
                //        ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ApplicationDarkGrayBrush = ColorDisabled;
                //        ((StyleUpdater)Application.Current.Resources["StyleUpdater"]).ElementRequestedTheme = ElementTheme.Light;
                //    }
                //}
            }
            catch { }
        }

        //Adjust the Font Sizes
        public static void AdjustFontSizes()
        {
            try
            {
                int FontSize = Convert.ToInt32(AppVariables.ApplicationSettings["AdjustFontSize"]);
                //System.Diagnostics.Debug.WriteLine("Adjusting the font size to: " + FontSize);

                //Fix
                //double SmallSize = (double)Application.Current.Resources["TextSizeSmall"] + FontSize;
                //((StyleUpdater)Application.Current.Resources["StyleUpdater"]).TextSizeSmall = SmallSize;

                //double MediumSize = (double)Application.Current.Resources["TextSizeMedium"] + FontSize;
                //((StyleUpdater)Application.Current.Resources["StyleUpdater"]).TextSizeMedium = MediumSize;

                //double LargeSize = (double)Application.Current.Resources["TextSizeLarge"] + FontSize;
                //((StyleUpdater)Application.Current.Resources["StyleUpdater"]).TextSizeLarge = LargeSize;

                //double HugeSize = (double)Application.Current.Resources["TextSizeHuge"] + FontSize;
                //((StyleUpdater)Application.Current.Resources["StyleUpdater"]).TextSizeHuge = HugeSize;
            }
            catch { }
        }

        //Wait for element to finish the layout update
        public static Task FinishLayoutUpdateAsync(FrameworkElement frameworkElement)
        {
            try
            {
                TaskCompletionSource<bool> TaskResult = new TaskCompletionSource<bool>();
                void eventHandler(object sender, object args)
                {
                    frameworkElement.LayoutUpdated -= eventHandler;
                    TaskResult.SetResult(true);
                }

                frameworkElement.LayoutUpdated += eventHandler;
                return Task.WhenAll(new[] { Task.Delay(10), TaskResult.Task });
            }
            catch { return null; }
        }
    }
}