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
        #region static properties
        public static String TitleContent = "Draw";
        public static String SubTitleContent = "and save your masterpiece";
        public static Boolean GoAbout = true;
        #endregion

        // Global
        private Drawing.Drawing currentDrawing;

        // Variable for saving the drawing
        private System.Windows.Threading.DispatcherTimer saveTimer;

        // Variable for printing the saved drawing (TODO : remove/comment it and the drawing methods)
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
        private bool addingNewDrawing = false;
        private string drawingTitle;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DrawingPanel(String itemToDraw, Boolean addingNewDrawing)
        {
            InitializeComponent();
            item = itemToDraw;
            this.addingNewDrawing = addingNewDrawing;
            currentDrawing = new Drawing.Drawing(item);
            
            // Hide buttons and everything
            SaveFeedbackOK.Visibility = System.Windows.Visibility.Collapsed;
            SaveFeedbackNOTOK.Visibility = System.Windows.Visibility.Collapsed;
            ButtonGoBackHome.Visibility = System.Windows.Visibility.Collapsed;


            // If the user wants to add his own title
            if (addingNewDrawing)
            {
                DrawingToDraw.Visibility = System.Windows.Visibility.Collapsed;
                FieldTitle.Visibility = System.Windows.Visibility.Visible;
                Console.WriteLine("Entering Draw mode with adding New Drawing");
            }
            // If the user uses what was written on the card
            else
            {
                DrawingToDraw.Visibility = System.Windows.Visibility.Visible;
                FieldTitle.Visibility = System.Windows.Visibility.Collapsed;
                DrawingToDraw.Text = item;
                Console.WriteLine("Entering Draw mode with item given on card");
            }

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
        #endregion Mouse & Touch EventListeners

        #region OnClick methods

        private void OnClickDrawAnother(object sender, RoutedEventArgs e)
        {
            //Not really a clean way to do it, but we just load again the page.
            ((MainWindow)Application.Current.MainWindow).GoToHomePage();
            ((MainWindow)Application.Current.MainWindow).GoToRandomCardPage();
        }

        private void OnClickAddANewOne(object sender, RoutedEventArgs e)
        {
            DrawingToDraw.Visibility = System.Windows.Visibility.Collapsed;
            FieldTitle.Visibility = System.Windows.Visibility.Visible;
            addingNewDrawing = true;
            //((MainWindow)Application.Current.MainWindow).NextPage(new AddingDrawingPanel(), "Draw", "Add your masterpiece to the database", true);
        }

        private void OnClickResetBoard(object sender, RoutedEventArgs e)
        {
            Reset(sender,e);
        }

        private void OnClickSaveDrawing(object sender, RoutedEventArgs e)
        {
            bool success = DoSaveDrawing();
            Console.WriteLine("Saving drawing. Success =" + success);
            if (success)
            {
                SaveFeedbackOK.Visibility = System.Windows.Visibility.Visible;
                SaveFeedbackNOTOK.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                SaveFeedbackNOTOK.Visibility = System.Windows.Visibility.Visible;
                SaveFeedbackOK.Visibility = System.Windows.Visibility.Collapsed;
            }
            ButtonGoBackHome.Visibility = System.Windows.Visibility.Visible;
        }

        private void OnClickGoBackHome(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).GoToHomePage();
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
            canvas.Background = new SolidColorBrush(currentDrawing.GetBackgroundAsColor());
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

            if (!remainingStrokes && dotPointerList.Count() == 0)
            {
                drawTimer.Stop();
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
            currentDrawing.SetBackgroundAsColor(((SolidColorBrush)canvas.Background).Color);
        }

        private void ClearBoard()
        {
            PrintDebug("Cleared");
            canvas.Strokes.Clear();
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

        private Boolean DoSaveDrawing()
        {
            if (assignAuthorToDrawing() && assignTitleToDrawing())
            {
                return App.client.SaveDrawingToServer(currentDrawing);
            }
            else
            {
                return false;
            }
        }

        // Assign title
        private bool assignTitleToDrawing()
        {
            // If user is adding new item
            if (addingNewDrawing)
            {
                String title = FieldTitle.Text.Trim();
                if (title == String.Empty)
                {
                    //TODO Custom more beautiful MessageBox
                    MessageBox.Show("Please enter a title.");
                    return false;
                }
                else
                {
                    drawingTitle = title;
                    currentDrawing.Item = drawingTitle;
                    App.client.AddItemToServer(drawingTitle);
                    return true;
                }
            }
            // If user is using the item on the card
            else
            {
                drawingTitle = item;
                currentDrawing.Item = drawingTitle;
                return true;
            }
        }

        // Assign author
        private Boolean assignAuthorToDrawing()
        {
            String author = FieldName.Text.Trim();
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
            currentDrawing.SetBackgroundAsColor(c);
        }

        private void BackgroundBlack(object sender, RoutedEventArgs e)
        {
            ChangeBackgroundColor(((SolidColorBrush)FindResource("BrushColor-Black")).Color);
        }

        private void BackgroundWhite(object sender, RoutedEventArgs e)
        {
            ChangeBackgroundColor(((SolidColorBrush)FindResource("BrushColor-White")).Color);
        }

        private void BackgroundPaleYellow(object sender, RoutedEventArgs e)
        {
            ChangeBackgroundColor(((SolidColorBrush)FindResource("BrushColor-PaleYellow")).Color);
        }

        private void BackgroundSkyBlue(object sender, RoutedEventArgs e)
        {
            ChangeBackgroundColor(((SolidColorBrush)FindResource("BrushColor-SkyBlue")).Color);
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

        private void BrushBlack(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Black")).Color);
        }

        private void BrushGrey(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Grey")).Color);
        }

        private void BrushWhite(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-White")).Color);
        }

        private void BrushPink(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Pink")).Color);
        }

        private void BrushMauve(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Mauve")).Color);
        }

        private void BrushPurple(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Purple")).Color);
        }

        private void BrushIndigo(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Indigo")).Color);
        }

        private void BrushMarineBlue(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-MarineBlue")).Color);
        }

        private void BrushBlue(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Blue")).Color);
        }

        private void BrushSkyBlue(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-SkyBlue")).Color);
        }

        private void BrushBlueGreen(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-BlueGreen")).Color);
        }

        private void BrushDarkGreen(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-DarkGreen")).Color);
        }

        private void BrushGreen(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Green")).Color);
        }

        private void BrushGreenYellow(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-GreenYellow")).Color);
        }

        private void BrushPaleYellow(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-PaleYellow")).Color);
        }

        private void BrushYellow(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Yellow")).Color);
        }

        private void BrushAmbre(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Ambre")).Color);
        }

        private void BrushOrange(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Orange")).Color);
        }

        private void BrushRed(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Red")).Color);
        }

        private void BrushBrown(object sender, RoutedEventArgs e)
        {
            ChangeBrushColor(((SolidColorBrush)FindResource("BrushColor-Brown")).Color);
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

        private void BgRed_TouchEnter(object sender, TouchEventArgs e)
        {

        }



    }
}