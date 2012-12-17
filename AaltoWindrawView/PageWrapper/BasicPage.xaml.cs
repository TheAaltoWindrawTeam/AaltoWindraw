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
	public partial class BasicPage : UserControl
	{
        private String _title;
        private String _subTitle;
        private Boolean _possibleGoBack;
        private Boolean _possibleClickAbout;

        public BasicPage()
		{
			this.InitializeComponent();
            _title = "AaltoWindraw";
            _subTitle = "";
            _possibleGoBack = false;
            _possibleClickAbout = false;
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region Setters & Getters
        public void setTitle(String title)
        {
            _title = title;
            PageTitle.Title.Text = title;
        }

        public void setSubTitle(String subTitle)
        {
            _subTitle = subTitle;
            PageTitle.SubTitle.Text = subTitle;
        }

        public String getTitle()
        {
            return this._title;
        }

        public String getSubTitle()
        {
            return this._subTitle;
        }

        public void setPossibleGoBack(Boolean possible)
        {
            this._possibleGoBack = possible;
            PageNavigation.Button_Back.IsEnabled = possible;
        }

        public Boolean isPossibleGoBack() 
        {
            return this._possibleGoBack;
        }

        public void setPossibleClickAbout(Boolean possible)
        {
            this._possibleGoBack = possible;
            PageNavigation.Button_About.IsEnabled = possible;
        }

        public Boolean isPossibleClickAbout()
        {
            return this._possibleClickAbout;
        }

        #endregion Setters & Getters

        public void OnClickClose(object sender, RoutedEventArgs e)
		{
            Application.Current.MainWindow.Close();
		}

        public void OnClickBackwards(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).PreviousPage();
            
        }

        public void OnClickAbout(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GoToAboutPage();
        }
    }
}