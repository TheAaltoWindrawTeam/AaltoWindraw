using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace AaltoWindraw
{
    namespace Drawing
    {
        [Serializable]
        public class SampledStroke : ISerializable
        {

            // This class contain a particular type of stroke
            // The points are ordered but they are sampled : between two successive point, 
            // an interpolation is made to ensure the strokes is continuous

            // /!\ The inner state is not verified :
            //     one must use this class on read-only OR write-only

            private List<Dot> str;          // set of the dots (ordered)
            private readonly int beginning; // frame corresponding to the first point

            private SampledStroke() { }

            public SampledStroke(int b)
            {
                beginning = b;
                str = new List<Dot>();
            }

            public SampledStroke(SerializationInfo info, StreamingContext ctxt)
            {
                beginning = info.GetInt32("beginning");
                str = (List<Dot>)info.GetValue("str", typeof(List<Dot>));
            }

            public int Beginning
            { get { return beginning; } }

            public IEnumerator<Dot> Enum
            { get { return str.GetEnumerator(); } }


            public void AddDot(Dot d)
            {
                str.Add(d);
            }


            public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
            {
                info.AddValue("beginning", beginning);
                info.AddValue("str", str, typeof(List<Dot>));
            }
        }
    }
}
