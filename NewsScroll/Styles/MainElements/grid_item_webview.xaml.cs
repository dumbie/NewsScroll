﻿using ArnoldVinkCode;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace NewsScroll.MainElements
{
    public partial class grid_item_webview
    {
        //Webview variables
        private DispatcherTimer vDispatcherTimer_MemoryCheck = new DispatcherTimer();

        //Initialize window
        public grid_item_webview()
        {
            this.InitializeComponent();
            this.Loaded += delegate
            {
                //Show the scroll swipe overlay
                if (AVFunctions.DevMobile())
                {
                    grid_MobileScrollLeft.Visibility = Visibility.Visible;
                    grid_MobileScrollRight.Visibility = Visibility.Visible;
                }

                //Create timer that monitors the available memory
                vDispatcherTimer_MemoryCheck.Interval = TimeSpan.FromSeconds(1);
                vDispatcherTimer_MemoryCheck.Tick += DispatcherTimer_MemoryCheck_Tick;
                vDispatcherTimer_MemoryCheck.Start();
            };
        }

        //Monitor memory usage
        void DispatcherTimer_MemoryCheck_Tick(object sender, object e)
        {
            try
            {
                //Get the apps current memory usage
                ulong memoryPercentage = AVFunctions.DevMemoryUsePercentage();

                //Check if the memory is almost full
                if (memoryPercentage >= 80)
                {
                    ShowMemoryWarning();
                }
            }
            catch { }
        }

        //Show memory warning
        void ShowMemoryWarning()
        {
            try
            {
                Debug.WriteLine("Memory limit almost reached, unloading website to prevent crash.");

                //Set memory text
                item_status.Text = "Webview unloaded,\ndevice is low on memory.";

                //Unload the webviewer
                item_source.Stop();
                item_source.NavigateToString(string.Empty);
            }
            catch { }
        }
    }
}