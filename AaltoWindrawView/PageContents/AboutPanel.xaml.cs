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
	public partial class AboutPanel : UserControl
    {
        #region static properties
        public static String TitleContent = "About";
        public static String SubTitleContent = "this game";
        public static Boolean GoAbout = false;
        #endregion

        #region Properties
        private String title;
        public String subTitle;

        public String Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        public String SubTitle
        {
            get { return this.subTitle; }
            set { this.subTitle = value; }
        }
        #endregion Properties
        
        public AboutPanel()
		{
			this.InitializeComponent();
			//TODO Remplace with content from license file
			AboutContent.Text = "This project is part of the course T-111.5350 Multimedia Programming at Aalto University. "
			+ "Basically, it is a port of classic boardgame Pictionary to the Aalto Window platform. "
			+ "It is kind of similar to some other game: Draw Something (Android, Iphone).\n"
			+ "\n"						
			+ "Aalto Window Platforms are Microsoft PixelSense tables buffed up with some awesome tools ( Kinect, APIs and so on) and located on the three campus of Aalto University. "
			+ "They prove to be the archetypal platform for the kind of game that AaltoWindraw is.";
			
			AboutLicense.Text = "This program is free software: you can redistribute "
            +"it and/or modify it under the terms of the GNU General Public License "
            +"as published by the Free Software Foundation, either version 3 of the License, "
            +"or (at your option) any later version.\n"
            +"\n"
            +"You should have received a copy of the GNU General Public License"
            +"along with this program.\nIf not, see http://www.gnu.org/licenses/";
		}
	}
}