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
            private readonly Color color;    // Color of the point
            private readonly Double radius;  // Radius of the point


            // Disable default constructor
            private Dot() { }

            public Dot(Double pX, Double pY, Color pColor, Double pRadius)
            {
                position = new Point(pX, pY);
                color = pColor;
                radius = pRadius;
            }

            public Dot(SerializationInfo info, StreamingContext ctxt)
            {
                position = (Point)info.GetValue("Position", typeof(Point));
                byte[] tempColor = (byte[])info.GetValue("Color", typeof(byte[]));
                color = Color.FromArgb(tempColor[0], tempColor[1], tempColor[2], tempColor[3]);
                radius = info.GetDouble("Radius");
            }

            public Point Position
            { get { return position; } }

            public Color Color
            { get { return color; } }

            public Double Radius
            { get { return radius; } }

            public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
            {
                info.AddValue("Position", this.position, typeof(Point));
                byte[] tempColor = { this.color.A, this.color.R, this.color.G, this.color.B };
                info.AddValue("Color", tempColor, typeof(byte[]));
                info.AddValue("Radius", this.radius);
            }
        }


    }
}

