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
        bool remainingStrokes;
        private IEnumerator<Drawing.SampledStroke> strokesEnum; // return the next sampledStroke to start to be drawn
        private List<IEnumerator<Drawing.Dot>> dotPointerList;  // countain the current SampledStrokes, to be finished to be drawn

        private const int REFRESH_TIME_DRAW = 10;
        private const string DRAWING_FOLDER = @"..\..\..\Drawings\";

        //TODO
        private String[] arrayOfRandomWords;

        private string item;
		
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
            canvas.EditingMode = SurfaceInkEditingMode.None;
            canvas.Background = new SolidColorBrush(currentDrawing.Background);
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
            GuessTimer.Text = String.Format("{0:00}:{1:00}:{2:00}", currentDrawing.CurrentFrame / 6000,
                                                                   (currentDrawing.CurrentFrame % 6000 / 100),
                                                                    currentDrawing.CurrentFrame % 100);

            if (!remainingStrokes && dotPointerList.Count() == 0)
            {
                drawTimer.Stop();
                StartStop.Content = "End...";
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