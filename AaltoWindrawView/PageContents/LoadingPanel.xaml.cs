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
using System.Windows.Controls.Primitives;

namespace AaltoWindraw
{
	/// <summary>
	/// Logique d'interaction pour LoadingPopup.xaml
	/// </summary>
	public partial class LoadingPanel : UserControl
	{
        #region static properties
        public static String TitleContent = "Loading";
        public static String SubTitleContent = "Please wait";
        public static Boolean GoAbout = true;
        #endregion

		public LoadingPanel()
		{
			this.InitializeComponent();
			
			// Insérez le code requis pour la création d’objet sous ce point.
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{

		}
	}
}