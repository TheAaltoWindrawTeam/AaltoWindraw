using System;
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
using System.Windows.Ink;
using AaltoWindraw.Utilities;

namespace AaltoWindraw
{
	public partial class GuessingPanel : UserControl
	{
        #region static properties
        public static String TitleContent = "Guess";
        public static String SubTitleContent = "as quick as you can";
        public static Boolean GoAbout = true;
        #endregion

		// Global
        private AaltoWindraw.Drawing.Drawing currentDrawing;
        private Boolean timerIsOver = true;

        // Variable for printing the saved drawing
        private System.Windows.Threading.DispatcherTimer drawTimer;
        bool remainingStrokes;
        private IEnumerator<Drawing.SampledStroke> strokesEnum; // return the next sampledStroke to start to be drawn
        private List<IEnumerator<Drawing.Dot>> dotPointerList;  // countain the current SampledStrokes, to be finished to be drawn

        private const int REFRESH_TIME_DRAW = 20;

        private ulong Milliseconds()
        {
            return (ulong)currentDrawing.CurrentFrame * (ulong)REFRESH_TIME_DRAW;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GuessingPanel(Drawing.Drawing drawingToGuess)
        {
            InitializeComponent();
            currentDrawing = drawingToGuess;
            drawTimer = new System.Windows.Threading.DispatcherTimer();
            drawTimer.Tick += new EventHandler(DrawFrame);
            drawTimer.Interval = new TimeSpan(0, 0, 0, 0, REFRESH_TIME_DRAW);
            timerIsOver = false;
            // Hide buttons and everything
            ButtonPlayAgain.Visibility = System.Windows.Visibility.Collapsed;
            ButtonCheckScore.Visibility = System.Windows.Visibility.Collapsed;
            ButtonTryAgain.Visibility = System.Windows.Visibility.Collapsed;
            UserAttemptFeedbackRight.Visibility = System.Windows.Visibility.Collapsed;
            UserAttemptFeedbackWrong.Visibility = System.Windows.Visibility.Collapsed;
            UserAttemptFeedbackTooLate.Visibility = System.Windows.Visibility.Collapsed;

            DoDraw();

            // No drawing in this phase
            canvas.EditingMode = SurfaceInkEditingMode.None;
        }


        #region OnClick
        private void OnClickIKnow(object sender, RoutedEventArgs e)
        {
            bool distant = verifyUserInput();

            // If close to right answer
            if (!distant)
            {
                ButtonIKnow.Visibility = System.Windows.Visibility.Collapsed;
                ButtonTryAgain.Visibility = System.Windows.Visibility.Collapsed;
                UserAttemptFeedbackRight.Visibility = System.Windows.Visibility.Visible;
                UserAttemptFeedbackWrong.Visibility = System.Windows.Visibility.Collapsed;
                UserAttemptFeedbackTooLate.Visibility = System.Windows.Visibility.Collapsed;
                ButtonCheckScore.Visibility = System.Windows.Visibility.Visible;
                UserAttempt.IsEnabled = false;
                
            }
            // If not close to right answer
            else
            {
                //ButtonTryAgain.Visibility = System.Windows.Visibility.Visible;
                UserAttemptFeedbackRight.Visibility = System.Windows.Visibility.Collapsed;
                UserAttemptFeedbackWrong.Visibility = System.Windows.Visibility.Visible;
                UserAttemptFeedbackTooLate.Visibility = System.Windows.Visibility.Collapsed;
                ButtonCheckScore.Visibility = System.Windows.Visibility.Collapsed;
                UserAttempt.Text = "";
                Keyboard.Focus(UserAttempt);
                drawTimer.Start();
            }
        }

        private void OnClickTryAgain(object sender, RoutedEventArgs e)
        {
            // Change buttons visibility
            ButtonTryAgain.Visibility = System.Windows.Visibility.Collapsed;
            ButtonIKnow.Visibility = System.Windows.Visibility.Visible;
            UserAttemptFeedbackRight.Visibility = System.Windows.Visibility.Collapsed;
            UserAttemptFeedbackWrong.Visibility = System.Windows.Visibility.Collapsed;
            // Focus keyboard for user input
            UserAttempt.Text = "";
            UserAttempt.IsEnabled = true;
            FocusTextBox(UserAttempt, e);
            // Restart timer
            drawTimer.Start();
        }

        private ulong ComputeUserScore()
        {
            return Milliseconds();
        }

        private void OnClickPlayAgain(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GoToHomePage();
            ((MainWindow)Application.Current.MainWindow).GoToBeforeGuessingPage();
        }

        private void OnClickCheckMyScore(object sender, RoutedEventArgs e)
        {
            AfterGuessingPanel aftergp = new AfterGuessingPanel(currentDrawing, ComputeUserScore());
            ((MainWindow)Application.Current.MainWindow).GoToAfterGuessingPage(aftergp);
        }

        private bool verifyUserInput()
        {
            String attempt = UserAttempt.Text;
            String rightAnswer = currentDrawing.Item;
            Console.WriteLine("Compare userattempt="+attempt+" to rightAnswer="+rightAnswer);
            bool distant = StringDistanceEvaluator.Distant(attempt, rightAnswer);
            Console.WriteLine("Distant? "+distant);
            return distant;
        }


        private void OnClickTryAnother(Object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GoToHomePage();
            ((MainWindow)Application.Current.MainWindow).GoToBeforeGuessingPage();
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                OnClickIKnow(sender, e);
            }
            else
            {
                // Stop timer
                drawTimer.Stop();
            }
        }

        private void OnTouchEnter(object sender, TouchEventArgs e)
        {
            drawTimer.Stop();
            UserAttempt.IsEnabled = true;
            Keyboard.Focus((TextBox)UserAttempt);
        }

        #endregion


        private void FocusTextBox(Object sender, EventArgs e)
        {
            //((TextBox)sender).Focus();
            Keyboard.Focus((TextBox)sender);
        }

        #region Draw

        private void DoDraw()
        {
            ClearBoard();
            canvas.EditingMode = SurfaceInkEditingMode.None;
            canvas.Background = new SolidColorBrush(currentDrawing.GetBackgroundAsColor());
            //newStroke = true;
            //frameEnumerator = currentDrawing.Frames.GetEnumerator();
            strokesEnum = currentDrawing.EnumStrokes;
            remainingStrokes = strokesEnum.MoveNext();
            dotPointerList = new List<IEnumerator<Drawing.Dot>>();

            drawTimer.Start();
            currentDrawing.reinit();
        }


        private void DrawFrame(Object sender, EventArgs e)
        {
            // Test if there are new strokes to draw, not started yet
            while (remainingStrokes && strokesEnum.Current.Beginning == currentDrawing.CurrentFrame)
            {
                IEnumerator<Drawing.Dot> temp = strokesEnum.Current.Enum;
                temp.MoveNext();
                dotPointerList.Add(temp);
                remainingStrokes = strokesEnum.MoveNext();
            }

            List<IEnumerator<Drawing.Dot>> toBeRemoved = new List<IEnumerator<Drawing.Dot>>();

            // Draw the current dots, linked to the previous ones
            foreach (IEnumerator<Drawing.Dot> Enumerator in dotPointerList)
            {
                Drawing.Dot d = Enumerator.Current;
                if (Enumerator.MoveNext())
                {
                    Drawing.Dot d2 = Enumerator.Current;
                    var strokePoints = new StylusPointCollection();
                    strokePoints.Add(new StylusPoint(d.X, d.Y));
                    strokePoints.Add(new StylusPoint(d2.X, d2.Y));
                    var drawingAttributes = new System.Windows.Ink.DrawingAttributes();
                    drawingAttributes.Color = d.GetColor();
                    drawingAttributes.Width = d.Radius;
                    drawingAttributes.Height = d.Radius;
                    Stroke stroke = new Stroke(strokePoints, drawingAttributes);
                    canvas.Strokes.Add(stroke);
                }
                else
                {
                    toBeRemoved.Add(Enumerator);
                }
            }

            // Remove the finished strokes
            dotPointerList.RemoveAll(x => toBeRemoved.Contains(x));
            currentDrawing.NewFrame();

            ulong time = Milliseconds();
            GuessTimer.Text = String.Format("{0:00}:{1:00}:{2:00}", time / 60000,
                                                                   (time / 1000) % 60,
                                                                    (time / 10) % 100);

            if (!remainingStrokes && dotPointerList.Count() == 0)
            {
                drawTimer.Stop();
                timerIsOver = true;
                DisableEverythingWhenDrawingIsOver();
                UserAttemptFeedbackTooLate.Visibility = System.Windows.Visibility.Visible;
                ButtonPlayAgain.Visibility = System.Windows.Visibility.Visible;
            }

        }

        private void DisableEverythingWhenDrawingIsOver()
        {
            ButtonIKnow.Visibility = System.Windows.Visibility.Collapsed;
            ButtonTryAgain.Visibility = System.Windows.Visibility.Collapsed;
            UserAttemptFeedbackRight.Visibility = System.Windows.Visibility.Collapsed;
            UserAttemptFeedbackWrong.Visibility = System.Windows.Visibility.Collapsed;
            if (timerIsOver)
            {
                UserAttempt.IsEnabled = false;
            }
        }

        //clear the board
        private void ClearBoard()
        {
            canvas.Strokes.Clear();
        }

        #endregion

    }
}