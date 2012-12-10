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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

namespace AaltoWindraw
{
	/// <summary>
	/// Logique d'interaction pour BeforeGuessingPanel.xaml
	/// </summary>
	public partial class BeforeGuessingPanel : UserControl
	{

        #region static properties
        public static String TitleContent = "Guess";
        public static String SubTitleContent = "as quick as you can";
        public static Boolean GoAbout = true;
        #endregion

        private int _time;
        private DispatcherTimer _countdownTimer;
        private volatile bool drawingLoaded = false;
        private Drawing.Drawing drawingToGuess;

		public BeforeGuessingPanel()
		{
			this.InitializeComponent();
            FinalCountdown.Visibility = System.Windows.Visibility.Collapsed;
            Thread loadDrawingThread = new Thread(new ThreadStart(LoadDrawing));
            loadDrawingThread.Name = "loadDrawingThread";
            loadDrawingThread.Start();
		}
		
		private void OpenGuessingWindow()
        {
            if (drawingLoaded)
            {
                GuessingPanel gp = new GuessingPanel(drawingToGuess);
                ((MainWindow)Application.Current.MainWindow).GoToGuessingPanel(gp);
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).GoToHomePage();
            }
        }

        private void OnClickGo(object sender, RoutedEventArgs e)
        {
            ButtonGo.IsEnabled = false;
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = new TimeSpan(0, 0, 1);
            _countdownTimer.Tick += new EventHandler(CountdownTimerStep);
            _time = 3;
            _countdownTimer.Start();
            FinalCountdown.Visibility = System.Windows.Visibility.Visible;
        }

        public void LoadDrawing()
        {
            // Get items from server
            String[] items = App.client.GetItemsFromServer().ToArray();

            // Shuffle elements of items
            Random random = new Random();
            for (int i = 0; i < items.Length-1; i += 1)
            {
                int swapIndex = random.Next(i + 1, items.Length);
                String temp = items[i];
                items[i] = items[swapIndex];
                items[swapIndex] = temp;
            }

            // Check if a drawing exists for this item
            foreach (String item in items)
            {
                drawingToGuess = App.client.GetDrawingFromServer(item);
                Console.Write("Check drawing for " + item+"? ");
                Console.WriteLine(drawingToGuess != null);
                if (drawingToGuess != null)
                    break;
            }
            drawingLoaded = (drawingToGuess!=null);
        }

        private void CountdownTimerStep(object sender, EventArgs e)
        {
            if (_time > 0)
            {
                _time--;
                this.FinalCountdown.Text = _time.ToString();
            }
            else
            {
                _countdownTimer.Stop();
                OpenGuessingWindow();
            }
        }
	}
}