﻿using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NewsScroll
{
    public partial class StarredPage
    {
        private Point TouchStartingPoint;
        private double TouchSwipingLimit = 70;
        private double TouchSwipingTarget = 80;
        private bool TouchSwipingCompleted = false;

        //Adjust the swipe bar
        void SwipeBarAdjust()
        {
            try
            {
                int SwipeBarDirection = Convert.ToInt32(AppVariables.ApplicationSettings["SwipeDirection"]);
                if (SwipeBarDirection == 0)
                {
                    //Left to right
                    grid_SwipeBar.Height = double.NaN;
                    grid_SwipeBar.Width = 15;
                    grid_SwipeBar.Margin = new Thickness(0, 100, 0, 80);
                    grid_SwipeBar.HorizontalAlignment = HorizontalAlignment.Left;
                    grid_SwipeBar.VerticalAlignment = VerticalAlignment.Stretch;
                }
                else if (SwipeBarDirection == 1)
                {
                    //Top to bottom
                    grid_SwipeBar.Height = 15;
                    grid_SwipeBar.Width = double.NaN;
                    grid_SwipeBar.Margin = new Thickness(40, 0, 40, 0);
                    grid_SwipeBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                    grid_SwipeBar.VerticalAlignment = VerticalAlignment.Top;
                }
                else if (SwipeBarDirection == 2)
                {
                    //Right to left
                    grid_SwipeBar.Height = double.NaN;
                    grid_SwipeBar.Width = 15;
                    grid_SwipeBar.Margin = new Thickness(0, 100, 0, 80);
                    grid_SwipeBar.HorizontalAlignment = HorizontalAlignment.Right;
                    grid_SwipeBar.VerticalAlignment = VerticalAlignment.Stretch;
                }
                else if (SwipeBarDirection == 3)
                {
                    //Bottom to top
                    grid_SwipeBar.Height = 15;
                    grid_SwipeBar.Width = double.NaN;
                    grid_SwipeBar.Margin = new Thickness(40, 0, 40, 0);
                    grid_SwipeBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                    grid_SwipeBar.VerticalAlignment = VerticalAlignment.Bottom;
                }
            }
            catch { }
        }

        //Validate the swipe range
        private bool SwipeValidate(double PositionY, double PositionX)
        {
            try
            {
                double CurrentVerticalDifference = Math.Abs(PositionY - TouchStartingPoint.Y);
                double CurrentHorizontalDifference = Math.Abs(PositionX - TouchStartingPoint.X);
                int SwipeBarDirection = Convert.ToInt32(AppVariables.ApplicationSettings["SwipeDirection"]);

                //Check horizontal swipe
                if (SwipeBarDirection == 0 || SwipeBarDirection == 2)
                {
                    if (CurrentVerticalDifference > TouchSwipingLimit) { return false; }
                    if (CurrentHorizontalDifference < TouchSwipingTarget) { return false; }
                }

                //Check vertical swipe
                if (SwipeBarDirection == 1 || SwipeBarDirection == 3)
                {
                    if (CurrentVerticalDifference < TouchSwipingTarget) { return false; }
                    if (CurrentHorizontalDifference > TouchSwipingLimit) { return false; }
                }

                return true;
            }
            catch { return false; }
        }

        //Handle user touch swipe
        void Page_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            try
            {
                TouchStartingPoint = e.Position;
            }
            catch { }
        }

        void Page_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                if (SwipeValidate(e.Position.Y, e.Position.X))
                {
                    if (!TouchSwipingCompleted)
                    {
                        TouchSwipingCompleted = true;

                        string SwipeAction = "hide";
                        if (stackpanel_Header.Visibility != Visibility.Visible) { SwipeAction = "open"; }

                        textblock_SwipeAction.Text = "Release to " + SwipeAction + " menu";
                        grid_SwipeAction.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    TouchSwipingCompleted = false;
                    grid_SwipeAction.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        async void Page_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            try
            {
                grid_SwipeAction.Visibility = Visibility.Collapsed;

                if (TouchSwipingCompleted)
                {
                    await HideShowHeader(false);
                    TouchSwipingCompleted = false;
                }

                //Reset the item selection
                ListView_Items.SelectedIndex = -1;
            }
            catch { }
        }
    }
}