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
            // Start client
            Console.WriteLine("connected:" + App.client.IsConnected());
            App.client.Start();
            InitializeButtons(App.client.IsConnected());
		}

        #region OnClick Methods
        private void OpenHighScoresWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new HighScoresPanel(), "High Scores", "Hall of fame", true);
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

        private void OnClickCheckAgain(object sender, RoutedEventArgs e)
        {
            if (App.client.IsConnected())
            {
                InitializeButtons(App.client.CheckServerAvailability());
            }
            else
            {
                InitializeButtons(App.client.Start());
            }
        }

        #endregion OnClick Methods

        private void InitializeButtons(bool clientConnected)
        {
            if (clientConnected)
            {
                IconServerDown.Visibility = System.Windows.Visibility.Collapsed;
                IconServerUp.Visibility = System.Windows.Visibility.Visible;
                Home_DrawButton.IsEnabled = true;
                Home_GuessButton.IsEnabled = true;
                Home_PlayOnlineButton.IsEnabled = true;
                Home_HighScoresButton.IsEnabled = true;
            }
            else
            {
                IconServerDown.Visibility = System.Windows.Visibility.Visible;
                IconServerUp.Visibility = System.Windows.Visibility.Collapsed;
                Home_DrawButton.IsEnabled = false;
                Home_GuessButton.IsEnabled = false;
                Home_PlayOnlineButton.IsEnabled = false;
                Home_HighScoresButton.IsEnabled = false;
            }
        }
    }
}