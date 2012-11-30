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

namespace AaltoWindraw
{
    /// <summary>
    /// Interaction logic for GuessingPage.xaml
    /// </summary>
    public partial class GuessingPage
    {

        // Global
        private AaltoWindraw.Drawing.Drawing currentDrawing;

        // Variable for printing the saved drawing
        private System.Windows.Threading.DispatcherTimer drawTimer;
        private StylusPoint lastPointDrawn;
        private bool newStroke;
        private IEnumerator<List<AaltoWindraw.Drawing.Dot>> frameEnumerator;
        private IEnumerator<AaltoWindraw.Drawing.Dot> currentStrokeDotEnumerator;

        private const int REFRESH_TIME_DRAW = 10;
        private const string DRAWING_FOLDER = @"..\..\..\Drawings\";

        //TODO
        private String[] arrayOfRandomWords;

        private string item;
		
        private int counter=0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GuessingPage()
        {
            InitializeComponent();
			GuessingGlobalLayout.Children.Add(new AboutPopup());

            //TODO: replace by Database
            arrayOfRandomWords = new String[] { "Batman", "Mickey Mouse", "A cat", "Tintin", "Donald Duck", "A wild Pikachu" };
            PickRandomName();
            
            drawTimer = new System.Windows.Threading.DispatcherTimer();
            drawTimer.Tick += new EventHandler(DrawFrame);
            drawTimer.Interval = new TimeSpan(0, 0, 0, 0, REFRESH_TIME_DRAW);
            
            OpenDrawing();

            // No drawing in this phase
            canvas.EditingMode = SurfaceInkEditingMode.None;
        }

        private void OnClickHomeButton(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).SetPage(new HomePage());
        }


        private void OnClickCloseButton(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Parent).Close();
        }


        




        private void OnClickStartStop(object sender, RoutedEventArgs e)
        {
            if (String.Equals(StartStop.Content,"I know!"))
            {
                drawTimer.Stop();
                StartStop.Content = "More?";
            }
            else if (String.Equals(StartStop.Content, "More?"))
            {
                drawTimer.Start();
                StartStop.Content = "I know!";
            }
            else if (String.Equals(StartStop.Content, "End...")) { } // do nothing
            else throw new Exception("Incorrect value in button StartStop in Guessing Windows");
        }

        private void DoDraw()
        {
            ClearBoard();
            canvas.Background = new SolidColorBrush(currentDrawing.Background);
            newStroke = true;
            frameEnumerator = currentDrawing.Frames.GetEnumerator();
            drawTimer.Start();
        }

        // TODO : add opacity
        private void DrawFrame(object sender, EventArgs e)
        {
            if (newStroke && frameEnumerator.MoveNext())
            {
                currentStrokeDotEnumerator = frameEnumerator.Current.GetEnumerator();
                currentStrokeDotEnumerator.MoveNext();
                AaltoWindraw.Drawing.Dot d = currentStrokeDotEnumerator.Current;
                this.lastPointDrawn = new StylusPoint(d.Position.X, d.Position.Y);
                newStroke = false;
            }
            else if (newStroke)
            {
                drawTimer.Stop();
                StartStop.Content = "End...";
            }
            else
            {
                AaltoWindraw.Drawing.Dot d = currentStrokeDotEnumerator.Current;
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

                counter++;
                GuessTimer.Text = String.Format("{0:00}:{1:00}:{2:00}", counter / 6000, (counter % 6000 / 100), counter % 100);
            }
        }

        //clear the board
        private void ClearBoard()
        {
            canvas.Strokes.Clear();
        }



        private void OpenDrawing()
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(DRAWING_FOLDER);
            System.IO.FileInfo[] files = dir.GetFiles("*" + AaltoWindraw.Drawing.Drawing.FILE_EXTENSION);

            Random random = new Random();
            int randomNumber = random.Next(0, files.Length);

            //item = "Batman_Foo_20121113195124.draw";

            if (DoOpenDrawing(  files[randomNumber].Name
                              //item
                                ))
                DoDraw();
        }

        private Boolean DoOpenDrawing(string fileName)
        {
            // TODO check if currentDrawing == null ?

            try
            {
                currentDrawing = AaltoWindraw.Utilities.FileSerializer<AaltoWindraw.Drawing.Drawing>.DeSerialize(DRAWING_FOLDER + @fileName);
            }
            catch //(Exception ex)
            {
                return false;
            }
            return true;
        }
        
        //TODO: replace with popup and database
        private void PickRandomName()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, arrayOfRandomWords.Length);
            item = arrayOfRandomWords[randomNumber];
        }
    }
}