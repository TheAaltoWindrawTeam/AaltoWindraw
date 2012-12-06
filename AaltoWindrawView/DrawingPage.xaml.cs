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

        // Variable for printing the saved drawing
        private System.Windows.Threading.DispatcherTimer drawTimer;
        bool remainingStrokes;
        private IEnumerator<SampledStroke> strokesEnum; // return the next sampledStroke to start to be drawn
        private List<IEnumerator<Dot>> dotPointerList;  // countain the current SampledStrokes, to be finished to be drawn

        //private List<Drawing.Dot>.Enumerator currentStrokeDotEnumerator;

        // TODO replace by proper entry in Properties or such
        // Time elapsed between two save/draw (in milliseconds)
        private const int REFRESH_TIME_SAVE = 10;
        private const int REFRESH_TIME_DRAW = 10;
        private const string DRAWING_FOLDER = @"..\..\..\Drawings\";

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
            arrayOfRandomWords = new String[] { "Batman", "Mickey Mouse", "A cat", "Tintin", "Donald Duck", "A wild Pikachu" };
            PickRandomName();

            currentDrawing = new Drawing.Drawing(item);

            saveTimer = new System.Windows.Threading.DispatcherTimer();
            saveTimer.Tick += new EventHandler(SaveFrame);
            saveTimer.Interval = new TimeSpan(0, 0, 0, 0, REFRESH_TIME_SAVE);
            drawTimer = new System.Windows.Threading.DispatcherTimer();
            drawTimer.Tick += new EventHandler(DrawFrame);
            drawTimer.Interval = new TimeSpan(0, 0, 0, 0, REFRESH_TIME_DRAW);
            DrawingGlobalLayout.Children.Add(new AboutPopup());

            ChangeBrushColor(Colors.Black);
        }

        // // // // --------------- \\ \\ \\ \\
        // // // // Event Listeners \\ \\ \\ \\
        // // // // --------------- \\ \\ \\ \\

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                currentDrawing.MoveStroke(e.GetPosition(canvas));
        }

        private void onTouchMove(object sender, TouchEventArgs e)
        {
            currentDrawing.MoveStroke(e.GetTouchPoint(canvas).Position);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            currentDrawing.BeginStroke(e.GetPosition(canvas));
            Start_Drawing(sender, e);
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            currentDrawing.BeginStroke(e.GetTouchPoint(canvas).Position);
            Start_Drawing(sender, e);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            currentDrawing.CompleteStroke(e.GetPosition(canvas));
            Stop_Drawing();
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            currentDrawing.CompleteStroke(e.GetTouchPoint(canvas).Position);
            Stop_Drawing();
        }

        private void OnClickCloseButton(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).Close();
        }

        private void OnClickHomeButton(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).SetPage(new HomePage());
        }

        private void LoadAnotherDrawing(object sender, RoutedEventArgs e)
        {
            PrintDebug("Load Another Drawing (not implemented)");
            ClearBoard();
            PickRandomName();
        }


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

        //clear the board
        private void ClearBoard()
        {
            PrintDebug("Cleared");
            canvas.Strokes.Clear();
        }


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
            //newStroke = true;
            //frameEnumerator = currentDrawing.Frames.GetEnumerator();
            strokesEnum = currentDrawing.EnumStrokes;
            remainingStrokes = strokesEnum.MoveNext();
            dotPointerList = new List<IEnumerator<Dot>>();
            drawTimer.Start();
            currentDrawing.reinit();
        }

        // TODO : add opacity
        private void DrawFrame(Object sender, EventArgs e)
        {
            // Test if there are new strokes to draw, not started yet
            while (remainingStrokes && strokesEnum.Current.Beginning == currentDrawing.CurrentFrame)
            {
                IEnumerator<Dot> temp = strokesEnum.Current.Enum;
                temp.MoveNext();
                dotPointerList.Add(temp);
                remainingStrokes = strokesEnum.MoveNext();
            }

            List<IEnumerator<Dot>> toBeRemoved = new List<IEnumerator<Dot>>();

            // Draw the current dots, linked to the previous ones
            foreach (IEnumerator<Dot> Enumerator in dotPointerList)
            {
                Dot d = Enumerator.Current;
                if (Enumerator.MoveNext())
                {
                    Dot d2 = Enumerator.Current;
                    var strokePoints = new StylusPointCollection();
                    strokePoints.Add(new StylusPoint(d.Position.X, d.Position.Y));
                    strokePoints.Add(new StylusPoint(d2.Position.X, d2.Position.Y));
                    var drawingAttributes = new System.Windows.Ink.DrawingAttributes();
                    drawingAttributes.Color = d.Color;
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

            if (!remainingStrokes && dotPointerList.Count() == 0)
            {
                drawTimer.Stop();
            }

        }



        // // // // ------------ \\ \\ \\ \\
        // // // // Save Drawing \\ \\ \\ \\ ADD DATABASE
        // // // // ------------ \\ \\ \\ \\


        //TODO: replace with popup and database
        // Choose the subject to draw
        private void PickRandomName()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, arrayOfRandomWords.Length);
            DrawingToGuess.Text = arrayOfRandomWords[randomNumber];
            item = arrayOfRandomWords[randomNumber];
        }

        private void Start_Drawing(object sender, EventArgs e)
        {
            if (currentDrawing.ReadOnly) return;
            // Save the current point once, and the timer will save it again every REFRESH_TIME_SAVE seconds
            if (!saveTimer.IsEnabled)
            {
                SaveFrame(sender, e);
                saveTimer.Start();
            }
        }

        // useless for now, but will be useful later for completing the end of the strokes
        private void Stop_Drawing()
        {
            if (currentDrawing.ReadOnly) return;
            if (currentDrawing.IsPaused())
            {
                //saveTimer.Stop();
                //currentDrawing.NextStroke();
            }
        }

        // TODO : add the color (+ radius, + opacity)
        private void SaveFrame(object sender, EventArgs e)
        {
            //DebugText2.Text = "save " + counter++ + " " + position.X + " " + position.Y;
            //Drawing.Dot p = new Drawing.Dot(position.X, position.Y, canvas.DefaultDrawingAttributes.Color, canvas.DefaultDrawingAttributes.Width);
            //currentDrawing.AddDot(p);
            currentDrawing.SaveFrame(canvas.DefaultDrawingAttributes.Color, canvas.DefaultDrawingAttributes.Width);
            if (currentDrawing.IsPaused())
            {
                saveTimer.Stop();
            }
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

        // // // // ------------ \\ \\ \\ \\
        // // // // Open Drawing \\ \\ \\ \\ TEMPORARY PART
        // // // // ------------ \\ \\ \\ \\

        private void OpenDrawing(object sender, RoutedEventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(DRAWING_FOLDER);
            FileInfo[] files = dir.GetFiles("*" + Drawing.Drawing.FILE_EXTENSION);

            Random random = new Random();
            int randomNumber = random.Next(0, files.Length);

            // TODO : check if the folder is empty
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

        // // // // ----------------- \\ \\ \\ \\
        // // // // Change brush size \\ \\ \\ \\
        // // // // ----------------- \\ \\ \\ \\

        private void SetBrushRadius(double radius)
        {
            Double newSize = Math.Round(radius, 0);
            PrintDebug("Selected Size=" + newSize);
            canvas.DefaultDrawingAttributes.Width = newSize;
            canvas.DefaultDrawingAttributes.Height = newSize;
            canvas.UsesTouchShape = false;
        }

        private void OnSlideValueChanged(object sender, EventArgs e)
        {
            SetBrushRadius(BrushRadiusSlider.Value);
        }


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



    }
}