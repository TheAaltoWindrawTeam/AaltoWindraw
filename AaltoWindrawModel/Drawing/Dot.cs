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
            // Coordinates of the point on the whiteboard
            private double x;
            private double y;
            private byte colorA;    // Color of the point (alpha)
            private byte colorR;    // Color of the point (red)
            private byte colorG;    // Color of the point (green)
            private byte colorB;    // Color of the point (blue)
            private Double radius;  // Radius of the point


            // Disable default constructor
            private Dot() { }

            public Dot(Double pX, Double pY, Color pColor, Double pRadius)
            {
                x = pX;
                y = pY;
                colorA = pColor.A;
                colorR = pColor.R;
                colorG = pColor.G;
                colorB = pColor.B;
                radius = pRadius;
            }

            public Dot(SerializationInfo info, StreamingContext ctxt)
            {
                x = info.GetDouble("X");
                y = info.GetDouble("Y");
                byte[] colorBytes = (byte[])info.GetValue("Color", typeof(byte[]));
                colorA = colorBytes[0];
                colorR = colorBytes[1];
                colorG = colorBytes[2];
                colorB = colorBytes[3];
                radius = info.GetDouble("Radius");
            }

            public double X
            {
                get { return x; }
                set { x = value; }
            }

            public double Y
            {
                get { return y; }
                set { y = value; }
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

            public Point GetPosition()
            {
                return new Point(x, y);
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
                info.AddValue("X", this.x);
                info.AddValue("Y", this.y);
                byte[] colorBytes = new byte[] { colorA, colorR, colorG, colorB };
                info.AddValue("Color", colorBytes, typeof(byte[]));
                info.AddValue("Radius", this.radius);
            }
        }


    }
}

