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
            private readonly Point position; // Coordinates of the point on the whiteboard
            private readonly byte colorA;    // Color of the point (alpha)
            private readonly byte colorR;    // Color of the point (red)
            private readonly byte colorG;    // Color of the point (green)
            private readonly byte colorB;    // Color of the point (blue)
            private readonly Double radius;  // Radius of the point


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
            { get { return position; } }

            public byte ColorA
            {
                get { return colorA; }
            }

            public byte ColorR
            {
                get { return colorR; }
            }

            public byte ColorG
            {
                get { return colorG; }
            }

            public byte ColorB
            {
                get { return colorB; }
            }

            public Color GetColor()
            {
                return Color.FromArgb(colorA, colorR, colorG, colorB);
            }
            
            public Double Radius
            { get { return radius; } }

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

