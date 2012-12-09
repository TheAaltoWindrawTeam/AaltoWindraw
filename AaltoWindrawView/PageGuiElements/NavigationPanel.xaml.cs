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
	public partial class NavigationPanel : UserControl
	{
		public NavigationPanel()
		{
			this.InitializeComponent();

			// Insérez le code requis pour la création d’objet sous ce point.
		}

        private void OnClickClose(object sender, RoutedEventArgs e)
        {
            Grid parent1 = (Grid)Parent;
            BasicPage parent2 = (BasicPage)parent1.Parent;
            parent2.OnClickClose(sender, e);
           
        }

        private void OnClickAbout(object sender, RoutedEventArgs e)
        {
            Grid parent1 = (Grid)Parent;
            BasicPage parent2 = (BasicPage)parent1.Parent;
            parent2.OnClickAbout(sender, e);
        }

        private void OnClickBackwards(object sender, RoutedEventArgs e)
        {
            Grid parent1 = (Grid)Parent;
            BasicPage parent2 = (BasicPage)parent1.Parent;
            parent2.OnClickBackwards(sender, e);
        }
	}
}