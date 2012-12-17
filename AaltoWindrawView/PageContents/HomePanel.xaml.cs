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
        #region static properties
        public static String TitleContent = "Choose";
        public static String SubTitleContent = "a game mode";
        public static Boolean GoAbout = true;
        #endregion

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
            ((MainWindow)Application.Current.MainWindow).GoToHighScoresPage();
        }

        private void OpenBeforeGuessingWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GoToBeforeGuessingPage();
        }

        private void OpenRandomCardWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GoToRandomCardPage();
        }

        private void OpenOnlineWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GoToOnlinePage();
        }

        private void OnClickCheckAgain(object sender, RoutedEventArgs e)
        {
            CheckAgainServerAvailability();
        }

        #endregion OnClick Methods

        #region UI modifications
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
        #endregion

        private void CheckAgainServerAvailability()
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
    }
}