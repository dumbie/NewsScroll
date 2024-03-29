﻿using System;
using Xamarin.Forms;
using static ArnoldVinkCode.ArnoldVinkSettings;

namespace NewsScroll
{
    public partial class ItemPopup
    {
        //Adjust the swipe bar
        void SwipeBarAdjust()
        {
            try
            {
                int SwipeBarDirection = Convert.ToInt32(AppSettingLoad("SwipeDirection"));
                if (SwipeBarDirection == 0)
                {
                    //Left to right
                    grid_SwipeBar.HeightRequest = -1;
                    grid_SwipeBar.WidthRequest = 15;
                    grid_SwipeBar.Margin = new Thickness(0, 100, 0, 80);
                    grid_SwipeBar.HorizontalOptions = LayoutOptions.Start;
                    grid_SwipeBar.VerticalOptions = LayoutOptions.Fill;
                    grid_SwipeBar.GestureRecognizers.Clear();
                    SwipeGestureRecognizer swipeGestureRecognizer = new SwipeGestureRecognizer();
                    swipeGestureRecognizer.Direction = SwipeDirection.Right;
                    swipeGestureRecognizer.Swiped += Page_ManipulationCompleted;
                    grid_SwipeBar.GestureRecognizers.Add(swipeGestureRecognizer);
                }
                else if (SwipeBarDirection == 1)
                {
                    //Top to bottom
                    grid_SwipeBar.HeightRequest = 15;
                    grid_SwipeBar.WidthRequest = -1;
                    grid_SwipeBar.Margin = new Thickness(40, 0, 40, 0);
                    grid_SwipeBar.HorizontalOptions = LayoutOptions.Fill;
                    grid_SwipeBar.VerticalOptions = LayoutOptions.Start;
                    grid_SwipeBar.GestureRecognizers.Clear();
                    SwipeGestureRecognizer swipeGestureRecognizer = new SwipeGestureRecognizer();
                    swipeGestureRecognizer.Direction = SwipeDirection.Down;
                    swipeGestureRecognizer.Swiped += Page_ManipulationCompleted;
                    grid_SwipeBar.GestureRecognizers.Add(swipeGestureRecognizer);
                }
                else if (SwipeBarDirection == 2)
                {
                    //Right to left
                    grid_SwipeBar.HeightRequest = -1;
                    grid_SwipeBar.WidthRequest = 15;
                    grid_SwipeBar.Margin = new Thickness(0, 100, 0, 80);
                    grid_SwipeBar.HorizontalOptions = LayoutOptions.End;
                    grid_SwipeBar.VerticalOptions = LayoutOptions.Fill;
                    grid_SwipeBar.GestureRecognizers.Clear();
                    SwipeGestureRecognizer swipeGestureRecognizer = new SwipeGestureRecognizer();
                    swipeGestureRecognizer.Direction = SwipeDirection.Left;
                    swipeGestureRecognizer.Swiped += Page_ManipulationCompleted;
                    grid_SwipeBar.GestureRecognizers.Add(swipeGestureRecognizer);
                }
                else if (SwipeBarDirection == 3)
                {
                    //Bottom to top
                    grid_SwipeBar.HeightRequest = 15;
                    grid_SwipeBar.WidthRequest = -1;
                    grid_SwipeBar.Margin = new Thickness(40, 0, 40, 0);
                    grid_SwipeBar.HorizontalOptions = LayoutOptions.Fill;
                    grid_SwipeBar.VerticalOptions = LayoutOptions.End;
                    grid_SwipeBar.GestureRecognizers.Clear();
                    SwipeGestureRecognizer swipeGestureRecognizer = new SwipeGestureRecognizer();
                    swipeGestureRecognizer.Direction = SwipeDirection.Up;
                    swipeGestureRecognizer.Swiped += Page_ManipulationCompleted;
                    grid_SwipeBar.GestureRecognizers.Add(swipeGestureRecognizer);
                }
            }
            catch { }
        }

        //Handle user touch swipe
        async void Page_ManipulationCompleted(object sender, SwipedEventArgs e)
        {
            try
            {
                await ClosePopup();
            }
            catch { }
        }
    }
}