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
        public class SampledStroke : ISerializable
        {

            // This class contains a particular type of stroke
            // The points are ordered but they are sampled : between two successive point, 
            // an interpolation is made to ensure the stroke is continuous

            // /!\ The inner state is not verified :
            //     one must use this class on read-only OR write-only

            private List<Dot> str;          // set of the dots (ordered)
            private int beginning; // frame corresponding to the first point
            // position of the next dot (may still change before sampling)
            private double positionX;
            private double positionY;

            private SampledStroke() { }

            public SampledStroke(int b, Point d)
            {
                beginning = b;
                str = new List<Dot>();
                positionX = d.X;
                positionY = d.Y;
            }

            public SampledStroke(SerializationInfo info, StreamingContext ctxt)
            {
                beginning = info.GetInt32("beginning");
                str = (List<Dot>)info.GetValue("str", typeof(List<Dot>));
                positionX = 0;
                positionY = 0;
            }

            public int Beginning
            { 
                get { return beginning; }
                set { beginning = value; }
            }

            public double PositionX
            {
                get { return positionX; }
                set { positionX = value; }
            }

            public double PositionY
            {
                get { return positionY; }
                set { positionY = value; }
            }

            public List<Dot> Str
            {
                get { return str; }
                set { str = value; }
            }

            public Point GetPosition()
            {
                return new Point(positionX, positionY);
            }

            public IEnumerator<Dot> Enum
            { get { return str.GetEnumerator(); } }

            public void NewDot(Color pColor, double pRadius)
            {
                str.Add(new Dot(positionX, PositionY, pColor, pRadius));
            }

            public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
            {
                info.AddValue("beginning", beginning);
                info.AddValue("str", str, typeof(List<Dot>));
            }
        }
    }
}
