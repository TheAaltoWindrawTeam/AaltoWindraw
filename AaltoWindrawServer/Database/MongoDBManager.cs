using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

namespace AaltoWindraw.Database
{
    class MongoDBManager
    {
        private MongoClient mongo;
        private MongoDatabase db;
        private MongoCollection<Drawing.Drawing> drawings;
        private MongoCollection<Highscores.Highscore> highscores;
        private MongoCollection<BsonDocument> items;
        private Process mongoProcess;

        public bool Start()
        {
            try
            {
                // Boot up DB
                mongoProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Properties.Resources.mongodb_exe,
                        Arguments = "--journal --dbpath " + Properties.Resources.db_path,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        CreateNoWindow = true
                    }
                };
                mongoProcess.Start();
            }
            catch (Exception)
            {
                // Getting an Exception here commonly means mongod is already up and running
            }

            mongo = new MongoClient();  // connect to localhost
            db = mongo.GetServer().GetDatabase(Properties.Resources.db_name);

            drawings = db.GetCollection<Drawing.Drawing>("drawings");
            highscores = db.GetCollection<Highscores.Highscore>("highscores");
            items = db.GetCollection<BsonDocument>("items");
            return true;
        }

        public bool Stop()
        {
            try
            {
                mongoProcess.Close();
                mongoProcess.Kill();
            }
            catch (System.InvalidOperationException)
            {
                return false;
            }
            return true;
        }

        internal Drawing.Drawing[] GetDrawingsByItem(string item)
        {
            List<Drawing.Drawing> tmpList = new List<Drawing.Drawing>();
            MongoCursor<Drawing.Drawing> cursor = drawings.Find(new QueryDocument("Item", item));
            foreach (Drawing.Drawing drawing in cursor)
            {
                tmpList.Add(drawing);
            }
            return tmpList.ToArray<Drawing.Drawing>();
        }

        internal Drawing.Drawing GetDrawingById(string id)
        {
            return drawings.FindOne(new QueryDocument("_id", id));
        }

        internal bool SaveScore(AaltoWindraw.Highscores.Highscore highscore)
        {
            AaltoWindraw.Highscores.Highscore hs = highscores.FindOne(new QueryDocument("_id", highscore.id));
            try
            {
                if (hs == null)
                {
                    return highscores.Insert<Highscores.Highscore>(highscore).Ok;
                }
                else
                {
                    hs.score = highscore.score;
                    return highscores.Save(hs).Ok;
                } 
            }
            catch (WriteConcernException)
            {
                return false;
            }
        }

        internal bool SaveDrawing(Drawing.Drawing drawing)
        {
            if(drawing.ID != null && !drawing.ID.Equals(""))
                throw new Exception("You are not allowed to insert a pre-existing drawing to the database");
            drawing.ID = ObjectId.GenerateNewId().ToString();
            return drawings.Insert<Drawing.Drawing>(drawing).Ok;
        }

        internal List<string> GetItems()
        {
            List<string> itemList = new List<string>();
            var cursor = items.FindAll();
            // To use in case of too many items (one day maybe)
            //cursor.Skip = 100;
            //cursor.Limit = 10;
            foreach (var item in cursor)
            {
                itemList.Add(item["item"].AsString);
            }
            return itemList;
        }

        internal Highscores.Highscore GetHighscoreById(string drawingId)
        {
            MongoCursor<Highscores.Highscore> hs = highscores.Find(new QueryDocument("_id", drawingId));
            switch (hs.Count())
            {
                case 0:
                    return null;
                case 1:
                    return hs.First();
                default:
                    throw new Exception("Highscore check should read at most one highscore");
            }
        }

        internal List<Highscores.Highscore> GetHighscores()
        {
            List<Highscores.Highscore> highscoreList = new List<Highscores.Highscore>();
            var cursor = highscores.FindAll();
            // To use in case of too many highscores (one day maybe)
            //cursor.Skip = 100;
            //cursor.Limit = 10;
            foreach (Highscores.Highscore hs in cursor)
            {
                highscoreList.Add(hs);
            }
            return highscoreList;
        }
        
        internal bool SaveItem(string itemSent)
        {
            return (items.FindOne(new QueryDocument("item", itemSent)) == null) ?
                items.Insert(new BsonDocument("item", itemSent)).Ok :
                false;
        }

        public bool IsEmpty()
        {
            return items.FindAll().Count() == 0;
        }
    }
}
