﻿using System;
using System.Collections;
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
	/// Logique d'interaction pour RandomCardPanel.xaml
	/// </summary>
	public partial class RandomCardPanel : UserControl
	{

        private String item;
        //Provisoire
        private String[] arrayOfRandomWords;
		
        public RandomCardPanel()
		{
			this.InitializeComponent();
            ButtonNo.IsEnabled = false;
            ButtonYes.IsEnabled = false;
            ButtonAdd.IsEnabled = false;
            arrayOfRandomWords = App.client.GetItemsFromServer().ToArray();
		}

        private void OnClickCard1(object sender, RoutedEventArgs e)
        {
            DisableCards();
            PickRandomName();
        }

        private void OnClickCard2(object sender, RoutedEventArgs e)
        {
            DisableCards();
            PickRandomName();
        }

        private void OnClickCard3(object sender, RoutedEventArgs e)
        {
            DisableCards();
            PickRandomName();
        }

        private void OnClickAnotherOne(object sender, RoutedEventArgs e)
        {
            //Not really a clean way to do it, but we just load again the page.
            ((MainWindow)Application.Current.MainWindow).PreviousPage();
            ((MainWindow)Application.Current.MainWindow).NextPage(new RandomCardPanel(), "Draw", "First, pick a card", true);
        }

        //TODO: replace with database
        // Choose the subject to draw
        private void PickRandomName()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, arrayOfRandomWords.Length);
            item = arrayOfRandomWords[randomNumber];
            DrawingToGuess.Text = item;
            ButtonNo.IsEnabled = true;
            ButtonYes.IsEnabled = true;
            ButtonAdd.IsEnabled = true;
        }

        private void DisableCards()
        {
            Card1.IsEnabled = false;
            Card2.IsEnabled = false;
            Card3.IsEnabled = false;
        }

        private void OpenDrawingWindow(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new DrawingPanel(item, false), "Draw", "Paint and save your masterpiece", true);
        }

        private void OnClickDrawWhatIWant(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new DrawingPanel(item, true), "Draw", "Add your masterpiece to the database", true);
        }
	}
}