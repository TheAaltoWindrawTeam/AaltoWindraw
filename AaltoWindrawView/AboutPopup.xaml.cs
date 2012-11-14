﻿using System;
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
    /// Interaction logic for AboutPopup.xaml
    /// </summary>
    public partial class AboutPopup : Popup
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AboutPopup()
        {
            InitializeComponent();

            AboutContent.Text =
                "This project is part of the course T-111.5350 Multimedia Programming at Aalto University.\n"+
                "Basically, it is a port of classic boardgame Pictionary to the Aalto Window platform.";
			 AboutLicense.Text = "GNU General Public License.";
        }
        
    }
}