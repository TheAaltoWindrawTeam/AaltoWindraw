using System;
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

namespace AaltoWindraw
{
    /// <summary>
    /// Interaction logic for GuessingPage.xaml
    /// </summary>
    public partial class GuessingPage
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public GuessingPage()
        {
            InitializeComponent();
			GuessingGlobalLayout.Children.Add(new AboutPopup());
        }

        private void OnClickHomeButton(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).SetPage(new HomePage());
        }


        private void OnClickCloseButton(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).Close();
        }

    }
}