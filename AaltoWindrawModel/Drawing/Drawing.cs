using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Runtime.Serialization;
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
            private List<List<Dot>> frames;   // Set of Dots forming the animation


            // Following attributes should not be serialized
            private List<Dot> currentStroke;
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
                frames = new List<List<Dot>>();
                currentStroke = new List<Dot>();
                readOnly = false;
                this.author = null;
                
                //TODO replace following placeholders with relevant values
                this.author = "Foo";
                this.SetBackgroundAsColor(Colors.WhiteSmoke);
            }

            // Constructor for deserialization - should be used by serializer
            public Drawing( SerializationInfo info, StreamingContext ctxt )
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
                frames = (List<List<Dot>>)info.GetValue("Frames", typeof(List<List<Dot>>));
                
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
                    this.Name + Properties.Resources.drawing_file_extension
                    : null;
            }

            // Getter for frames
            // First, we just store one Dot per frame in currentDots 
            // and we need it to be sure it's done correctly
            public List<List<Dot>> Frames
            {
                get { return frames; }
            }

            public bool ReadOnly
            {
                get { return readOnly; }
            }

            public void AddDot(Dot dot)
            {
                currentStroke.Add(dot);
            }

            public void NextStroke()
            {
                this.frames.Add(currentStroke);
                this.currentStroke = new List<Dot>();
                this.currentFrame++;
            }

            // Save the drawing in its current state (should not be updated after this!)
            public void Save()
            {
                if(!readOnly)
                {
                    timestamp = DateTime.Now;
                    name = DefineName();
                    readOnly = true;
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
                info.AddValue("Frames", frames, typeof(List<List<Dot>>));
            }
          }
    }
}
