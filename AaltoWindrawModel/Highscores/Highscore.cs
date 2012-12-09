using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AaltoWindraw.Highscores
{
    [Serializable]
    public class Highscore : ISerializable
    {
        public string id { get; set; }
        public ulong score { get; set; }
        public string drawingAuthor { get; set; }
        public string drawingItem { get; set; }
        public string scorerName { get; set; }
        public DateTime scoreTimestamp { get; set; }

        public Highscore()
        {
        }

        public Highscore(string id, string item, string author, string user, ulong highscore, DateTime timestamp)
            : this()
        {
            this.id = id;
            this.score = highscore;
            this.drawingAuthor = author;
            this.drawingItem = item;
            this.scorerName = user;
            this.scoreTimestamp = timestamp;
        }

        public Highscore(Drawing.Drawing drawing, string user, ulong highscore, DateTime timestamp)
            : this(drawing.ID, drawing.Item, drawing.Author, user, highscore, timestamp)
        {
        }

        public Highscore(SerializationInfo info, StreamingContext ctxt)
        {
            id = info.GetString("ID");
            score = info.GetUInt64("Score");
            drawingAuthor = info.GetString("Author");
            drawingItem = info.GetString("Item");
            scorerName = info.GetString("Scorer");
            scoreTimestamp = (DateTime)info.GetValue("Timestamp", typeof(DateTime));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("ID", id);
            info.AddValue("Score", score);
            info.AddValue("Author", drawingAuthor);
            info.AddValue("Item", drawingItem);
            info.AddValue("Scorer", scorerName);
            info.AddValue("Timestamp", scoreTimestamp, typeof(DateTime));
        }
    }
}
