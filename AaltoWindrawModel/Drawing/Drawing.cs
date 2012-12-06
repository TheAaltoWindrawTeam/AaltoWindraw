using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Windows;

namespace AaltoWindraw
{
    namespace Drawing
    {
        [Serializable]
        public class Drawing : ISerializable
        {
            // Extension of a Drawing file (used for storage only)
            public const string FILE_EXTENSION = ".draw";
            public const string FILE_FORMAT = "{0}_{1}_{2:00}{3:00}{4:00}{5:00}{6:00}{7:00}";

            private string name;    // Name of the drawing
            private string author;  // Name of the author of the drawing
            private DateTime timestamp; // Date of the drawing
            private string item;    // Item represented by the drawing
            private Color background;    // Background of the drawing
            private List<SampledStroke> strokes;   // Set of Dots forming the animation


            // Following attributes should not be serialized
            private List<SampledStroke> currentStrokes;
            private int currentFrame;

            /*
             * Read-only attribute is to be used as a lock on the Drawing.
             * 
             * In case of a new drawing, readOnly is set to false and the
             * user can add Dots to the Drawing by clicking.
             * When the user chooses to save, the attribute is set to true
             * and no further modifications to the Drawing should be 
             * expected (nor allowed).
             * 
             * In case of reading an existing drawing, the attribute is set
             * to true and the Drawing should not be modified in any way.
             */
            private bool readOnly;

            public Drawing(string item)
            {
                this.item = item;

                currentFrame = 0;
                strokes = new List<SampledStroke>();
                currentStrokes = new List<SampledStroke>();
                readOnly = false;
                this.author = null;

                //TODO replace following placeholders with relevant values
                this.author = "Foo";
                this.background = Colors.WhiteSmoke;
            }

            // Constructor for deserialization - should be used by serializer
            public Drawing(SerializationInfo info, StreamingContext ctxt)
            {
                name = info.GetString("Name");
                author = info.GetString("Author");
                timestamp = (DateTime)info.GetValue("Timestamp", typeof(DateTime));
                item = info.GetString("Item");
                byte[] tempBgColor = (byte[])info.GetValue("Background", typeof(byte[]));
                background = Color.FromArgb(tempBgColor[0], tempBgColor[1], tempBgColor[2], tempBgColor[3]);
                strokes = (List<SampledStroke>)info.GetValue("Strokes", typeof(List<SampledStroke>));

                readOnly = true;
            }

            // Getter and setter for name attribute
            public string Name
            {
                set { name = value; }
                get { return name; }
            }

            // Getter and setter for author attribute
            public string Author
            {
                set { author = value; }
                get { return author; }
            }

            // Getter and setter for timestamp attribute
            public DateTime Timestamp
            {
                set { timestamp = value; }
                get { return timestamp; }
            }

            // Getter and setter for item attribute
            public string Item
            {
                set { item = value; }
                get { return item; }
            }

            // Getter and setter for background attribute
            public Color Background
            {
                set { background = value; }
                get { return background; }
            }

            // Returns the file name used for storing the drawing into a file
            public string FileName()
            {
                return this.readOnly ?
                    this.Name + FILE_EXTENSION
                    : null;
            }

            // Getter for strokes (TODO: is an enumerator enough ?)
            public IEnumerator<SampledStroke> EnumStrokes
            {
                get { return strokes.GetEnumerator(); }
            }

            public bool ReadOnly
            {
                get { return readOnly; }
            }

            public int CurrentFrame
            {
                get { return currentFrame; }
            }

            // TODO : to be deleted (still useful for drawing but there are probably other better way)
            public void NewFrame()
            {
                this.currentFrame++;
            }

            // TODO : to be deleted (same as NewFrame)
            public void reinit()
            {
                currentFrame = 0;
            }

            private int FindClosest(Point d)
            {
                int val = 0;
                double dist = Math.Sqrt(Math.Pow(this.currentStrokes[0].Position.X - d.X, 2) +
                                        Math.Pow(this.currentStrokes[0].Position.Y - d.Y, 2));
                // we assume that there is at least one element in currentStrokes
                // ie BeginStroke has been called more than CompleteStroke

                for (int i = 1; i < currentStrokes.Count; i++)
                {
                    double temp = Math.Sqrt(Math.Pow(this.currentStrokes[i].Position.X - d.X, 2) +
                                            Math.Pow(this.currentStrokes[i].Position.Y - d.Y, 2));
                    if (temp < dist)
                    {
                        dist = temp;
                        val = i;
                    }
                }

                return val;
            }

            public void BeginStroke(Point d)
            {
                currentStrokes.Add(new SampledStroke(currentFrame, d));
            }

            public void CompleteStroke(Point d)
            {
                if (currentStrokes.Count == 0) return; // should never happen
                int index = FindClosest(d);
                strokes.Add(currentStrokes[index]);
                currentStrokes.RemoveAt(index);
            }

            // TODO : add condition to be sure we ignore weird points
            public void MoveStroke(Point d)
            {
                if (currentStrokes.Count == 0) return; // should never happen
                int index = FindClosest(d);
                currentStrokes[index].Position = d;
            }

            public void SaveFrame(Color pColor, double pRadius)
            {
                foreach (SampledStroke ss in currentStrokes) {
                    ss.NewDot(pColor, pRadius);
                }
                NewFrame();
            }

            public bool IsPaused() {
                return currentStrokes.Count == 0;
            }


            // Save the drawing in its current state (should not be updated after this!)
            public void Save()
            {
                if (!readOnly)
                {
                    strokes = strokes.OrderBy(x => x.Beginning).ToList();
                    timestamp = DateTime.Now;
                    name = DefineName();
                    readOnly = true;
                    reinit();
                    Console.WriteLine(timestamp);
                    Console.WriteLine(name);
                    Console.WriteLine(FileName());
                }
            }

            private string DefineName()
            {
                return String.Format(FILE_FORMAT,
                    this.item, this.author,
                    this.timestamp.Year, this.timestamp.Month, this.timestamp.Day,
                    this.timestamp.Hour, this.timestamp.Minute, this.timestamp.Second);
            }

            // Serialize the drawing (used by serializer)
            public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
            {
                info.AddValue("Name", name);
                info.AddValue("Author", author);
                info.AddValue("Timestamp", timestamp, typeof(DateTime));
                info.AddValue("Item", item);
                byte[] tempBgColor = { background.A, background.R, background.G, background.B };
                info.AddValue("Background", tempBgColor, typeof(byte[]));
                info.AddValue("Strokes", strokes, typeof(List<SampledStroke>));
            }
        }
    }
}
