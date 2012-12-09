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

namespace AaltoWindraw
{
	/// <summary>
	/// Logique d'interaction pour BeforeGuessingPanel.xaml
	/// </summary>
	public partial class BeforeGuessingPanel : UserControl
	{
		public BeforeGuessingPanel()
		{
			this.InitializeComponent();
		}
		
		private void OpenGuessingWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new GuessingPanel(), "Guess", "Try to guess as quickly as possible", true);
        }
	}
}