using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Windows;
using AaltoWindraw.Properties;
using MongoDB.Bson.Serialization.Attributes;

namespace AaltoWindraw
{
    namespace Drawing
    {
        [Serializable]
        public class Drawing : ISerializable
        {
            private string id;  // ID of the drawing
            private string name;    // Name of the drawing
            private string author;  // Name of the author of the drawing
            private DateTime timestamp; // Date of the drawing
            private string item;    // Item represented by the drawing
            private byte backgroundA;    // Background of the drawing (alpha)
            private byte backgroundR;    // Background of the drawing (red)
            private byte backgroundG;    // Background of the drawing (green)
            private byte backgroundB;    // Background of the drawing (blue)
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
                this.author = "Anonymous";
                this.SetBackgroundAsColor(Colors.White);
            }

            // Constructor for deserialization - should be used by serializer
            public Drawing(SerializationInfo info, StreamingContext ctxt)
            {
                name = info.GetString("Name");
                author = info.GetString("Author");
                timestamp = (DateTime)info.GetValue("Timestamp", typeof(DateTime));
                item = info.GetString("Item");
                byte[] bgBytes = (byte[])info.GetValue("Background", typeof(byte[]));
                backgroundA = bgBytes[0];
                backgroundR = bgBytes[1];
                backgroundG = bgBytes[2];
                backgroundB = bgBytes[3];
                strokes = (List<SampledStroke>)info.GetValue("Strokes", typeof(List<SampledStroke>));
                readOnly = true;
            }

            // Getter and setter for id attribute
            [BsonId]
            public string ID
            {
                set { id = value; }
                get { return id; }
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

            // Getter and setter for background alpha attribute
            public byte BackgroundA
            {
                set { backgroundA = value; }
                get { return backgroundA; }
            }

            // Getter and setter for background red attribute
            public byte BackgroundR
            {
                set { backgroundR = value; }
                get { return backgroundR; }
            }

            // Getter and setter for background green attribute
            public byte BackgroundG
            {
                set { backgroundG = value; }
                get { return backgroundG; }
            }

            // Getter and setter for background blue attribute
            public byte BackgroundB
            {
                set { backgroundB = value; }
                get { return backgroundB; }
            }

            // Getter and setter for background strokes attribute
            public List<SampledStroke> Strokes
            {
                set { strokes = value; }
                get { return strokes; }
            }

            public void SetBackgroundAsColor(Color value)
            {
                backgroundA = value.A;
                backgroundR = value.R;
                backgroundG = value.G;
                backgroundB = value.B;
            }

            public Color GetBackgroundAsColor()
            {
                return Color.FromArgb(backgroundA, backgroundR, backgroundG, backgroundB);
            }

            // Returns the file name used for storing the drawing into a file
            public string FileName()
            {
                return this.readOnly ?
                    this.Name + Properties.Resources.drawing_file_extension : null;
            }

            // Getter for strokes (TODO: is an enumerator enough ?)
            public IEnumerator<SampledStroke> EnumStrokes
            {
                get { if (strokes == null) strokes = new List<SampledStroke>(); return strokes.GetEnumerator(); }
            }

            public int StrokesCount()
            {
                return strokes.Count;
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
                double dist = Math.Sqrt(Math.Pow(this.currentStrokes[0].PositionX - d.X, 2) +
                                        Math.Pow(this.currentStrokes[0].PositionY - d.Y, 2));
                // we assume that there is at least one element in currentStrokes
                // ie BeginStroke has been called more than CompleteStroke

                for (int i = 1; i < currentStrokes.Count; i++)
                {
                    double temp = Math.Sqrt(Math.Pow(this.currentStrokes[i].PositionX - d.X, 2) +
                                            Math.Pow(this.currentStrokes[i].PositionY - d.Y, 2));
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
                currentStrokes[index].PositionX = d.X;
                currentStrokes[index].PositionY = d.Y;
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
                }
            }

            private string DefineName()
            {
                return String.Format(Properties.Resources.drawing_file_format,
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
                byte[] bgBytes = new byte[] { backgroundA, backgroundR, backgroundG, backgroundB };
                info.AddValue("Background", bgBytes, typeof(byte[]));
                info.AddValue("Strokes", strokes, typeof(List<SampledStroke>));
            }
        }
    }
}
