using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using AaltoWindraw.Drawing;
using AaltoWindraw.Utilities;
using System.IO;

namespace AaltoWindraw
{
    /// <summary>
    /// Interaction logic for DrawingPage.xaml
    /// </summary>
    public partial class DrawingPage
    {

        // Global
        private Drawing.Drawing currentDrawing;

        // Variable for saving the drawing
        private System.Windows.Threading.DispatcherTimer saveTimer;
        private Point position;

        // Variable for printing the saved drawing
        private System.Windows.Threading.DispatcherTimer drawTimer;
        private StylusPoint lastPointDrawn;
        private bool newStroke;
        private IEnumerator<List<Drawing.Dot>> frameEnumerator;
        private IEnumerator<Drawing.Dot> currentStrokeDotEnumerator;
        
        //private List<Drawing.Dot>.Enumerator currentStrokeDotEnumerator;

        // TODO replace by proper entry in Properties or such
        // Time elapsed between two save/draw (in milliseconds)
        private const int REFRESH_TIME_SAVE = 10;
        private const int REFRESH_TIME_DRAW = 10;
        private const string DRAWING_FOLDER = @"..\..\..\Drawings\";

        // for testing
        private int counter = 0;

        //TODO replace following placeholders by relevant values

        /* The item attribute should be defined beforehand by the user.
         * Sequencing is as follow:
         * - User chooses to draw (from HomeWindow)
         * - A new DrawingWindow appears with an overlay
         * - The overlay shows some cards (say around 4)
         * - After the user has chosen a card (by clicking) the overlay disappears
         * - The item attribute is defined by user's choice
         * - Let's roll (DrawingWindow is visible and usable)
         */
        private string item;

        /* For the moment, hard coded, but actually needs to be from a database
         * (or file if we are lazy)
         */
        //TODO
        private String[] arrayOfRandomWords;
		
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DrawingPage()
        {
            InitializeComponent();
            
            //TODO: replace by Database
            arrayOfRandomWords=new String[]{"Batman","Mickey Mouse","A cat","Tintin","Donald Duck", "A wild Pikachu"};
            PickRandomName();

            currentDrawing = new Drawing.Drawing(item);
            
            saveTimer = new System.Windows.Threading.DispatcherTimer();
            saveTimer.Tick += new EventHandler(SaveFrame);
            saveTimer.Interval = new TimeSpan(0, 0, 0, 0, REFRESH_TIME_SAVE);
            drawTimer = new System.Windows.Threading.DispatcherTimer();
            drawTimer.Tick += new EventHandler(DrawFrame);
            drawTimer.Interval = new TimeSpan(0, 0, 0, 0, REFRESH_TIME_DRAW);
			DrawingGlobalLayout.Children.Add(new AboutPopup());
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            //DebugText.Text += "HAAAAAAAA";
            position = e.GetPosition(canvas);
        }

        private void OnMouseDown(object sender, RoutedEventArgs e)
        {
            if (currentDrawing.ReadOnly) return;
            SaveFrame(sender, e);
            saveTimer.Start();
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (currentDrawing.ReadOnly) return;
            saveTimer.Stop();
            currentDrawing.NextStroke();
        }

        // TODO : add the color (+ radius, + opacity)
        private void SaveFrame(object sender, EventArgs e)
        {
            DebugText2.Text = "save "+counter++ + " " + position.X + " " + position.Y;
            Drawing.Dot p = new Drawing.Dot(position.X, position.Y, canvas.DefaultDrawingAttributes.Color, canvas.DefaultDrawingAttributes.Width);
            currentDrawing.AddDot(p);
        }

		private void OnClickCloseButton(object sender, RoutedEventArgs e){
            ((MainWindow)this.Parent).Close();
		}

        private void OnClickHomeButton(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).SetPage(new HomePage());
        }
		
        private void Draw(object sender, RoutedEventArgs e)
        {
            currentDrawing.Save();
			DoDraw();
		}

        private void LoadAnotherDrawing(object sender, RoutedEventArgs e)
        {
            PrintDebug("Load Another Drawing (not implemented)");
            ClearBoard();
            PickRandomName();
        }
		
		private void DoDraw()
        {
            ClearBoard();
            canvas.Background = new SolidColorBrush(currentDrawing.Background);
            newStroke = true;
            frameEnumerator = currentDrawing.Frames.GetEnumerator();
            drawTimer.Start();
            counter = 0;
        }

        // TODO : add opacity
        private void DrawFrame(object sender, EventArgs e)
        {
            if (newStroke && frameEnumerator.MoveNext())
            {
                currentStrokeDotEnumerator = frameEnumerator.Current.GetEnumerator();
                currentStrokeDotEnumerator.MoveNext();
                Drawing.Dot d = currentStrokeDotEnumerator.Current;
                this.lastPointDrawn = new StylusPoint(d.Position.X, d.Position.Y);
                newStroke = false;
            }
            else if (newStroke)
            {
                drawTimer.Stop();
            }
            else
            {
                Drawing.Dot d = currentStrokeDotEnumerator.Current;
                var strokePoints = new StylusPointCollection();
                strokePoints.Add(this.lastPointDrawn);
                var newPointDrawn = new StylusPoint(d.Position.X, d.Position.Y);
                strokePoints.Add(newPointDrawn);
                var drawingAttributes = new System.Windows.Ink.DrawingAttributes();
                drawingAttributes.Color = d.Color;
                drawingAttributes.Width = d.Radius;
                drawingAttributes.Height = d.Radius;
                Stroke stroke = new Stroke(strokePoints, drawingAttributes);
                this.lastPointDrawn = newPointDrawn;
                canvas.Strokes.Add(stroke);

                newStroke = !currentStrokeDotEnumerator.MoveNext();

                DebugText2.Text = "draw " + counter++ + " " + d.Radius;
            }
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            ClearBoard();
            drawTimer.Stop();
            currentDrawing = new Drawing.Drawing(item);
            currentDrawing.Background = ((SolidColorBrush)canvas.Background).Color;

        }

        //clear the board
        private void ClearBoard()
        {
            PrintDebug("Cleared");
            canvas.Strokes.Clear();
            counter = 0;
        }

        private void SaveDrawing(object sender, RoutedEventArgs e)
        {
            DoSaveDrawing();
        }

        private Boolean DoSaveDrawing()
        {
            currentDrawing.Save();
            try
            {
                FileSerializer<Drawing.Drawing>.Serialize(DRAWING_FOLDER + @currentDrawing.FileName(), currentDrawing);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        private void OpenDrawing(object sender, RoutedEventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(DRAWING_FOLDER);
            FileInfo[] files = dir.GetFiles("*"+Drawing.Drawing.FILE_EXTENSION);

            Random random = new Random();
            int randomNumber = random.Next(0, files.Length);

            if (DoOpenDrawing(files[randomNumber].Name))
                DoDraw();
        }

        private Boolean DoOpenDrawing(string fileName)
        {
            // TODO check if currentDrawing == null ?

            try
            {
                currentDrawing = FileSerializer<Drawing.Drawing>.DeSerialize(DRAWING_FOLDER + @fileName);
            }
            catch (Exception ex)
            {
                PrintDebug(ex.ToString());
                return false;
            }
            return true;
        }

        private void BackgroundBlue(object sender, RoutedEventArgs e)
        {
            ChangeBackgroundColor(Colors.Blue);
        }

        private void BackgroundRed(object sender, RoutedEventArgs e)
        {
            ChangeBackgroundColor(Colors.Red);
        }

        private void BackgroundOrange(object sender, RoutedEventArgs e)
        {
            ChangeBackgroundColor(Colors.Orange);
        }

        private void BackgroundYellow(object sender, RoutedEventArgs e)
        {
            ChangeBackgroundColor(Colors.Yellow);
        }

        private void ChangeBackgroundColor(Color c)
        {
            if (currentDrawing.ReadOnly) return;
            canvas.Background = new SolidColorBrush(c);
            currentDrawing.Background = c;
        }

        private void ChangeBrushColor(Color c)
        {
            if (currentDrawing.ReadOnly) return;
            canvas.DefaultDrawingAttributes.Color = c;
        }

        private void BrushYellow(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(Colors.Yellow);
        }

        private void BrushOrange(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(Colors.Orange);
        }

        private void BrushRed(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(Colors.Red);
        }

        private void BrushBlue(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(Colors.Blue);
        }

        private void SetBrushRadius(double radius)
        {
			Double newSize = Math.Round(radius,0);
            PrintDebug("Selected Size="+ newSize);
            canvas.DefaultDrawingAttributes.Width  = newSize;
            canvas.DefaultDrawingAttributes.Height = newSize;
            canvas.UsesTouchShape = false;
        }

        private void OnSlideValueChanged(object sender, EventArgs e)
        {
            SetBrushRadius(BrushRadiusSlider.Value);
        }

        // Print in the text panel in the application
        private void PrintDebug(String s)
        {
            try
            {
                DebugText.Text += s + "\n";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //TODO: replace with popup and database
        private void PickRandomName()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, arrayOfRandomWords.Length);
            DrawingToGuess.Text = arrayOfRandomWords[randomNumber];
            item = arrayOfRandomWords[randomNumber];
        }
       
    }
}