using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace AaltoWindraw
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private String someText = "default";

        public String SomeText
        {
            get { return this.someText; }
            set { this.someText = value; }
        }

        public void CloseApp()
        {
            MainWindow.Close();
        }
    }
}