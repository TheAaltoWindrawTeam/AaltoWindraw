using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace AaltoWindraw
{
     namespace Drawing
    {
        [Serializable]
        class Drawing : ISerializable
        {
            private string name;    // Name of the drawing
            private string author;  // Name of the author of the drawing
            private DateTime timestamp; // Date of the drawing
            private string item;    // Item represented by the drawing
            private Color background;    // Background of the drawing
            private Dictionary<int, List<Dot>> frames;    // Set of frames forming the animation (int stands for index - or time)
            private int length;  // Number of frames of the dynamic drawing


            // Following attributes should not be serialized
            private int currentFrame;
            private List<Dot> currentDots;
            private bool readOnly;

            public Drawing()
            {
                currentFrame = 0;
                currentDots = new List<Dot>();
                frames = new Dictionary<int, List<Dot>> ();
                readOnly = false;

//dicMyDic.Add("key",28);
                // Access like this
                //Dot d = frames[12];
            }

            // Constructor for deserialization - should be used by serializer
            public Drawing( SerializationInfo info, StreamingContext ctxt )
            {
                name = info.GetString("Name");
                author = info.GetString("Author");
                timestamp = (DateTime)info.GetValue("Timestamp", typeof(DateTime));
                item = info.GetString("Item");
                background = (Color)info.GetValue("Color", typeof(Color));
                frames = (Dictionary<int, List<Dot>>)info.GetValue("Frames", typeof(Dictionary<int, List<Dot>>));

                //FIXME this thing should not be implementation-specific... Isn't there a mere GetInt() method ?!
                length = info.GetInt32("Length");

                readOnly = true;
            }

            // Getter and setter for name attribute
            public string Name
            { 
                set { name = value; }
                get { return name; }
            }
            
            // Getter and setter for name attribute
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

            // Update the content of the drawing for step to next frame
            public void Update()
            {
                if(!readOnly)
                {
                    frames.Add(currentFrame++, currentDots);
                    currentDots = new List<Dot>();
                }
            }

            public void AddDot(Dot dot)
            {
                currentDots.Add(dot);
            }

            // Save the drawing in its current state (should not be updated after this!)
            public void Save()
            {
                if(!readOnly)
                {
                    length = currentFrame - 1;
                    timestamp = DateTime.Now;
                }
                readOnly = true;
            }

            // Serialize the drawing (used by serializer)
            public void GetObjectData( SerializationInfo info, StreamingContext ctxt )
            {
                info.AddValue("Name", name);
                info.AddValue("Author", author);
                info.AddValue("Timestamp", timestamp, typeof(DateTime));
                info.AddValue("Item", item);
                info.AddValue("Background", background, typeof(Color));
                info.AddValue("Frames", frames, typeof(Dictionary<int, List<Dot>>));
                info.AddValue("Length", length);
            }
          }

        
         
    }
}
