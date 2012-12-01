using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AaltoWindraw.Highscores
{
    public class Highscore
    {
        public ulong score { get; set; }
        public string drawingAuthor { get; set; }
        public string drawingItem { get; set; }
        public string scorerName { get; set; }

        public Highscore(string item, string author, string user, ulong highscore)
        {
            this.score = highscore;
            this.drawingAuthor = author;
            this.drawingItem = item;
            this.scorerName = user;
        }
    }
}
