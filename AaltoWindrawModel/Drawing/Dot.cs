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
        class Dot : ISerializable
        {
            private readonly Point position; // Coordinates of the point on the whiteboard
            private readonly Color color;    // Color of the point
            private readonly Double radius;  // Radius of the point
            private readonly Double opacity;  // Opacity of the point


            // Disable default constructor
            private Dot() { }

            public Dot(Double pX, Double pY, Color pColor, Double pRadius, Double pOpacity)
            {
                position = new Point(pX, pY);
                color = pColor;
                radius = pRadius;
                opacity = pOpacity;
            }

            public Dot( SerializationInfo info, StreamingContext ctxt )
            {
                position = (Point)info.GetValue("Position", typeof(Point));
                color = (Color)info.GetValue("Color", typeof(Color));
                radius = info.GetDouble("Radius");
                opacity = info.GetDouble("Opacity");
            }

            public Point Position
            { get { return position; } }

            public Color Color
            { get { return color; } }

            public Double Radius
            { get { return radius; } }

            public Double Opacity
            { get { return opacity; } }

            public void GetObjectData( SerializationInfo info, StreamingContext ctxt )
            {
              info.AddValue("Position", this.position, typeof(Point));
              info.AddValue("Color", this.color, typeof(Color));
              info.AddValue("Radius", this.radius);
              info.AddValue("Opacity", this.opacity);
            }
          }


        }
    }

