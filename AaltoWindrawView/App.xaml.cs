using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using AaltoWindraw.Network;

namespace AaltoWindraw
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Client client = new Client();

        public void CloseApp()
        {
            MainWindow.Close();
        }
    }
}