using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace AaltoWindraw
{
    namespace Drawing
    {
        [Serializable]
        public class Dot : ISerializable
        {
            private Point position; // Coordinates of the point on the whiteboard
            private byte colorA;    // Color of the point (alpha)
            private byte colorR;    // Color of the point (red)
            private byte colorG;    // Color of the point (green)
            private byte colorB;    // Color of the point (blue)
            private Double radius;  // Radius of the point


            // Disable default constructor
            private Dot() { }

            public Dot(Double pX, Double pY, Color pColor, Double pRadius)
            {
                position = new Point(pX, pY);
                colorA = pColor.A;
                colorR = pColor.R;
                colorG = pColor.G;
                colorB = pColor.B;
                radius = pRadius;
            }

            public Dot(SerializationInfo info, StreamingContext ctxt)
            {
                position = (Point)info.GetValue("Position", typeof(Point));
                byte[] colorBytes = (byte[])info.GetValue("Color", typeof(byte[]));
                colorA = colorBytes[0];
                colorR = colorBytes[1];
                colorG = colorBytes[2];
                colorB = colorBytes[3];
                radius = info.GetDouble("Radius");
            }

            public Point Position
            {
                get { return position; }
                set { position = value; }
            }

            public byte ColorA
            {
                get { return colorA; }
                set { colorA = value; }
            }

            public byte ColorR
            {
                get { return colorR; }
                set { colorR = value; }
            }

            public byte ColorG
            {
                get { return colorG; }
                set { colorG = value; }
            }

            public byte ColorB
            {
                get { return colorB; }
                set { colorB = value; }
            }

            public Color GetColor()
            {
                return Color.FromArgb(colorA, colorR, colorG, colorB);
            }
            
            public Double Radius
            {
                get { return radius; }
                set { radius = value; }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
            {
                info.AddValue("Position", this.position, typeof(Point));
                byte[] colorBytes = new byte[] { colorA, colorR, colorG, colorB };
                info.AddValue("Color", colorBytes, typeof(byte[]));
                info.AddValue("Radius", this.radius);
            }
        }


    }
}

