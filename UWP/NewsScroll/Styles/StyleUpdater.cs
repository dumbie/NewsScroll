﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace NewsScroll.Styles
{
    public class StyleUpdater : INotifyPropertyChanged
    {
        private double priv_TextSizeSmall = 18;
        public double TextSizeSmall
        {
            get { return priv_TextSizeSmall; }
            set
            {
                priv_TextSizeSmall = value;
                NotifyPropertyChanged();
            }
        }

        private double priv_TextSizeMedium = 20;
        public double TextSizeMedium
        {
            get { return priv_TextSizeMedium; }
            set
            {
                priv_TextSizeMedium = value;
                NotifyPropertyChanged();
            }
        }

        private double priv_TextSizeLarge = 24;
        public double TextSizeLarge
        {
            get { return priv_TextSizeLarge; }
            set
            {
                priv_TextSizeLarge = value;
                NotifyPropertyChanged();
            }
        }

        private double priv_TextSizeHuge = 28;
        public double TextSizeHuge
        {
            get { return priv_TextSizeHuge; }
            set
            {
                priv_TextSizeHuge = value;
                NotifyPropertyChanged();
            }
        }

        private ElementTheme priv_ElementRequestedTheme = ElementTheme.Default;
        public ElementTheme ElementRequestedTheme
        {
            get { return priv_ElementRequestedTheme; }
            set
            {
                priv_ElementRequestedTheme = value;
                NotifyPropertyChanged();
            }
        }

        private SolidColorBrush priv_ApplicationBackgroundEnabled;
        public SolidColorBrush ApplicationBackgroundEnabled
        {
            get { return priv_ApplicationBackgroundEnabled; }
            set
            {
                priv_ApplicationBackgroundEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private SolidColorBrush priv_ApplicationBackgroundDisabled;
        public SolidColorBrush ApplicationBackgroundDisabled
        {
            get { return priv_ApplicationBackgroundDisabled; }
            set
            {
                priv_ApplicationBackgroundDisabled = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}