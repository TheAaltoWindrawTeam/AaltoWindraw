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
	public partial class HighScoresPanel : UserControl
	{
        #region static properties
        public static String TitleContent = "High scores";
        public static String SubTitleContent = "of the champions";
        public static Boolean GoAbout = true;
        #endregion

		public HighScoresPanel()
		{
			this.InitializeComponent();

			// Insérez le code requis pour la création d’objet sous ce point.
		}
	}
}