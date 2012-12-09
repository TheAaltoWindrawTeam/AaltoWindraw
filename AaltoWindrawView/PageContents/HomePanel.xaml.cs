using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AaltoWindraw
{
	public partial class HomePanel : UserControl
	{
        
        public HomePanel()
		{
			this.InitializeComponent();
		}

        #region OnClick Methods
        private void OpenHighScoresWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new HighScoresPanel(), "High Scores", "Hall of fame", true);
        }

        private void OpenGuessingWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new GuessingPanel(), "Guess", "Try to guess as quickly as possible", true);
        }

        private void OpenBeforeGuessingWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new BeforeGuessingPanel(), "Guess", "Try to guess as quickly as possible", true);
        }

        private void OpenRandomCardWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new RandomCardPanel(), "Draw", "First, pick a card", true);
        }

        private void OpenOnlineWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new OnlinePanel(), "Play Online", "against people on another AaltoWindow", true);
        }

        #endregion OnClick Methods
    }
}