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
    public partial class DrawingPanel : UserControl
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

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DrawingPanel(String itemToDraw)
        {
            InitializeComponent();
            item = itemToDraw;
            currentDrawing = new Drawing.Drawing(item);
            DrawingToDraw.Text = item;

            saveTimer = new System.Windows.Threading.DispatcherTimer();
            saveTimer.Tick += new EventHandler(SaveFrame);
            saveTimer.Interval = new TimeSpan(0, 0, 0, 0, REFRESH_TIME_SAVE);
            drawTimer = new System.Windows.Threading.DispatcherTimer();
            drawTimer.Tick += new EventHandler(DrawFrame);
            drawTimer.Interval = new TimeSpan(0, 0, 0, 0, REFRESH_TIME_DRAW);

            ChangeBrushColor(Colors.Black);
        }

        #region Mouse & Touch EventListeners
        // // // // --------------- \\ \\ \\ \\
        // // // // Event Listeners \\ \\ \\ \\
        // // // // --------------- \\ \\ \\ \\

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            position = e.GetPosition(canvas);
        }

        private void onTouchMove(object sender, TouchEventArgs e)
        {
            position = e.GetTouchPoint(canvas).Position;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            Start_Drawing(sender, e);
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            Start_Drawing(sender, e);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            Stop_Drawing();
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            Stop_Drawing();
        }
        #endregion Mouse & Touch EventListeners

        #region OnClick methods

        private void OnClickDrawAnother(object sender, RoutedEventArgs e)
        {
            //Not really a clean way to do it, but we just load again the page.
            ((MainWindow)Application.Current.MainWindow).PreviousPage();
            ((MainWindow)Application.Current.MainWindow).PreviousPage();
            ((MainWindow)Application.Current.MainWindow).NextPage(new RandomCardPanel(), "Draw", "First, pick a card", true);
        }

        private void OnClickAddANewOne(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).NextPage(new AddingDrawingPanel(), "Draw", "Add your masterpiece to the database", true);
        }

        private void OnClickResetBoard(object sender, RoutedEventArgs e)
        {
            ClearBoard();
        }

        private void OnClickSaveDrawing(object sender, RoutedEventArgs e)
        {
            DoSaveDrawing();
        }

        private void OnClickChangeBrushColor(object sender, RoutedEventArgs e)
        {
            //TODO Bind with actual button color
            PrintDebug("sender=" + sender.GetType());
        }

        #endregion OnClick methods

        #region DrawCurrentDrawing
        // // // // -------------------- \\ \\ \\ \\
        // // // // Draw Current drawing \\ \\ \\ \\ TEMPORARY PART
        // // // // -------------------- \\ \\ \\ \\

        private void Draw(object sender, RoutedEventArgs e)
        {
            currentDrawing.Save();
            DoDraw();
        }

        private void DoDraw()
        {
            ClearBoard();
            canvas.EditingMode = SurfaceInkEditingMode.None;
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
        #endregion DrawCurrentDrawing

        #region ClearTheBoard
        // // // // --------------- \\ \\ \\ \\
        // // // // Clear the board \\ \\ \\ \\
        // // // // --------------- \\ \\ \\ \\

        private void Reset(object sender, RoutedEventArgs e)
        {
            ClearBoard();
            drawTimer.Stop();
            canvas.EditingMode = SurfaceInkEditingMode.Ink;
            currentDrawing = new Drawing.Drawing(item);
            currentDrawing.Background = ((SolidColorBrush)canvas.Background).Color;
        }

        private void ClearBoard()
        {
            PrintDebug("Cleared");
            canvas.Strokes.Clear();
            counter = 0;
        }
        #endregion ClearTheBoard

        #region SaveDrawing
        // // // // ------------ \\ \\ \\ \\
        // // // // Save Drawing \\ \\ \\ \\ ADD DATABASE
        // // // // ------------ \\ \\ \\ \\

        private void Start_Drawing(object sender, EventArgs e)
        {
            if (currentDrawing.ReadOnly) return;
            // Save the current point once, and the timer will save it again every REFRESH_TIME_SAVE seconds
            SaveFrame(sender, e);
            saveTimer.Start();
        }

        private void Stop_Drawing()
        {
            if (currentDrawing.ReadOnly) return;
            saveTimer.Stop();
            currentDrawing.NextStroke();
        }

        // TODO : add the color (+ radius, + opacity)
        private void SaveFrame(object sender, EventArgs e)
        {
            DebugText2.Text = "save " + counter++ + " " + position.X + " " + position.Y;
            Drawing.Dot p = new Drawing.Dot(position.X, position.Y, canvas.DefaultDrawingAttributes.Color, canvas.DefaultDrawingAttributes.Width);
            currentDrawing.AddDot(p);
        }

        private Boolean DoSaveDrawing()
        {
            Boolean authorNameWritten = assignAuthorToDrawing();
            if (authorNameWritten)
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
            else
            {
                return false;
            }
        }

        private Boolean assignAuthorToDrawing()
        {
            String author = FieldName.Text.Trim();
            author = author.Replace('_',' ');
            if (author == String.Empty)
            {
                //TODO Custom more beautiful MessageBox
                MessageBox.Show("Please enter your name.");
                return false;
            }
            else
            {
                currentDrawing.Author = author;
                return true;
            }
        }
        #endregion SaveDrawing

        #region ChangeBackground
        // // // // ----------------- \\ \\ \\ \\
        // // // // Change background \\ \\ \\ \\
        // // // // ----------------- \\ \\ \\ \\

        private void ChangeBackgroundColor(Color c)
        {
            if (currentDrawing.ReadOnly) return;
            canvas.Background = new SolidColorBrush(c);
            currentDrawing.Background = c;
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
        #endregion ChangeBackground

        #region ChangeBrushColor
        // // // // ------------------ \\ \\ \\ \\
        // // // // Change brush color \\ \\ \\ \\
        // // // // ------------------ \\ \\ \\ \\

        private void ChangeBrushColor(Color c)
        {
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
        #endregion ChangeBrushColor

        #region ChangeBrushSize
        // // // // ----------------- \\ \\ \\ \\
        // // // // Change brush size \\ \\ \\ \\
        // // // // ----------------- \\ \\ \\ \\

        private void SetBrushRadius(double radius)
        {
            Double factor = 0.5;
            Double newSize = Math.Round(radius * factor, 0);
            PrintDebug("Selected Size=" + newSize);
            canvas.DefaultDrawingAttributes.Width = newSize;
            canvas.DefaultDrawingAttributes.Height = newSize;
            canvas.UsesTouchShape = false;
        }

        private void OnSlideValueChanged(object sender, EventArgs e)
        {
            SetBrushRadius(BrushRadiusSlider.Value);
        }
        #endregion ChangeBrushSize

        #region Miscellaneous
        // // // // ------------- \\ \\ \\ \\
        // // // // Miscellaneous \\ \\ \\ \\
        // // // // ------------- \\ \\ \\ \\

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
        #endregion Miscellaneous



    }
}