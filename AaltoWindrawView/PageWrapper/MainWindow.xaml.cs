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
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Threading;
using AaltoWindraw.Network;

namespace AaltoWindraw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {
        #region Attributes & Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            // First page the user sees is HomePage
            GoToHomePage();
        }

        #endregion Attributes & Constructor

        #region EventsHandlers
        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }
        #endregion EventsHandlers

        #region SetPage

        /// <summary>
        /// Change the page on the screen. Works like this :
        /// - MainWindow uses a BasicPage that contains already basic GUI
        /// - The page in argument is actually a UserControl, it will be placed
        /// in the pageTransitionControl of the BasicPage
        /// </summary>
        public void NextPage(UserControl page)
        {
            //Prepare Wrapper (GUI navigation tools)
            BasicPage wrapper = new BasicPage();
            //Add window content
            wrapper.PageContent.Content = page;
            pageTransitionControl.NextPage(wrapper);
        }

        public void NextPage(UserControl page, String title, String subTitle, Boolean possibleClickAbout)
        {
            BasicPage wrapper = new BasicPage();
            wrapper.setTitle(title);
            wrapper.setSubTitle(subTitle);
            wrapper.setPossibleGoBack(false);
            wrapper.setPossibleClickAbout(possibleClickAbout);
            wrapper.PageContent.Content = page;
            pageTransitionControl.NextPage(wrapper);
        }

        public void PreviousPage()
        {
            pageTransitionControl.PreviousPage();
        }

        #endregion SetPage

        #region Pages

        public void GoToHomePage()
        {
            pageTransitionControl.EmptyPageStack();
            NextPage(new HomePanel(), HomePanel.TitleContent, HomePanel.SubTitleContent, HomePanel.GoAbout);
        }

        public void GoToAboutPage()
        {
            NextPage(new AboutPanel(), AboutPanel.TitleContent, AboutPanel.SubTitleContent, AboutPanel.GoAbout);
        }

        public void GoToRandomCardPage()
        {
            NextPage(new RandomCardPanel(), RandomCardPanel.TitleContent, RandomCardPanel.SubTitleContent, RandomCardPanel.GoAbout);
        }

        public void GoToOnlinePage()
        {
            NextPage(new OnlinePanel(), OnlinePanel.TitleContent, OnlinePanel.SubTitleContent, OnlinePanel.GoAbout);
        }

        public void GoToHighScoresPage()
        {
            NextPage(new HighScoresPanel(), HighScoresPanel.TitleContent, HighScoresPanel.SubTitleContent, HighScoresPanel.GoAbout);
        }

        public void GoToBeforeGuessingPage()
        {
            NextPage(new BeforeGuessingPanel(), BeforeGuessingPanel.TitleContent, BeforeGuessingPanel.SubTitleContent, BeforeGuessingPanel.GoAbout);
        }

        public void GoToAfterGuessingPage(AfterGuessingPanel aftergp)
        {
            NextPage(aftergp, AfterGuessingPanel.TitleContent, AfterGuessingPanel.SubTitleContent, AfterGuessingPanel.GoAbout);
        }

        public void GoToGuessingPanel(GuessingPanel gp)
        {
            NextPage(gp, GuessingPanel.TitleContent, GuessingPanel.SubTitleContent, GuessingPanel.GoAbout);
        }

        public void GoToDrawingPanel(String item, Boolean AddingNewDrawing)
        {
            NextPage(new DrawingPanel(item, AddingNewDrawing), DrawingPanel.TitleContent, DrawingPanel.SubTitleContent, DrawingPanel.GoAbout);
        }
        #endregion
    }
}