﻿using System;
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
	/// Logique d'interaction pour AfterGuessingPanel.xaml
	/// </summary>
	public partial class AfterGuessingPanel : UserControl
	{
        private Drawing.Drawing currentDrawing;
        private ulong userScore;

		public AfterGuessingPanel(Drawing.Drawing drawingGuessed, ulong userScore)
		{
			this.InitializeComponent();
            this.userScore = userScore;
            bool isHighscore = false;
            IsHighScoreFeedback.Visibility = System.Windows.Visibility.Collapsed;
            IsNotHighScoreFeedback.Visibility = System.Windows.Visibility.Collapsed;
            currentDrawing = drawingGuessed;
            UserScore.Text = FormatScore(userScore);
            Highscores.Highscore high = App.client.GetHighscoreFromServer(drawingGuessed);
            if (high != null)
            {
                ChampionScore.Text = FormatScore(high.score);
                ChampionIdentity.Text = "By " + high.scorerName + " at " + high.scoreTimestamp;
                isHighscore = Highscores.Highscore.CompareScores(userScore, high.score);
            }
            else
            {
                ChampionScore.Text = FormatScore(userScore);
                ChampionIdentity.Text = "";
                ChampionIdentity.Visibility = System.Windows.Visibility.Collapsed;
                isHighscore = true;
            }

            if (isHighscore)
            {
                IsHighScoreFeedback.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                IsNotHighScoreFeedback.Visibility = System.Windows.Visibility.Visible;
            }

		}

        private String FormatScore(ulong userScore)
        {
            return userScore.ToString()+" ms";
        }

        private String GetScorerName()
        {
            return FieldName.Text.Trim();
        }

		private void OnClickSave(Object sender, RoutedEventArgs e)
		{
            App.client.SaveScoreToServer(currentDrawing, GetScorerName(), this.userScore);
            ((MainWindow)Application.Current.MainWindow).GoToHomePage();
		}
		
		private void OnClickTryAgain(Object sender, RoutedEventArgs e)
		{
            ((MainWindow)Application.Current.MainWindow).GoToHomePage();
            ((MainWindow)Application.Current.MainWindow).NextPage(new BeforeGuessingPanel(), "Guess", "Try to guess as quickly as possible", true);
		}
	}
}