﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Controls.Primitives;

namespace AaltoWindraw
{
    public partial class HomePage
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HomePage()
        {
            InitializeComponent();
			HomeGlobalLayout.Children.Add(new AboutPopup());
        }

      
        private void OnClickCloseButton(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).Close();
        }

        private void OpenDrawingWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).SetPage(new DrawingPage());
        }

        private void OnClickAboutUsButton(object sender, RoutedEventArgs e)
        {
            //((MainWindow)this.Parent).SetPage(new AboutPopup());
        }

        private void OpenGuessingWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).SetPage(new GuessingPage());
        }
		
    }
}