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
	/// Logique d'interaction pour OnlinePanel.xaml
	/// </summary>
	public partial class OnlinePanel : UserControl
	{
        #region static properties
        public static String TitleContent = "Play online";
        public static String SubTitleContent = "against the world";
        public static Boolean GoAbout = true;
        #endregion

		public OnlinePanel()
		{
			this.InitializeComponent();
		}
	}
}