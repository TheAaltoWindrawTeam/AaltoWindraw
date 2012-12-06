using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AaltoWindraw.Network;

namespace AaltoWindraw
{
    class ClientProgram
    {

        private Client client;

        public ClientProgram()
        {
            client = new Client();
            client.Start();
        }

        public void AddItem(string item)
        {
            Console.WriteLine("Add "+item+": " + client.AddItemToServer(item));
        }

        public void SaveDrawing(string item)
        {
            Drawing.Drawing drawing = new Drawing.Drawing(item);
            Console.Write("Saving drawing of "+item+": ");
            Console.WriteLine(client.SaveDrawingToServer(drawing));
        }

        public void ShowItems()
        {
            Console.WriteLine("Showing all items:");
            List<string> items = client.GetItemsFromServer();
            foreach (string it in items)
            {
                Console.WriteLine(it);
            }
        }

        public Drawing.Drawing GetDrawing(string item)
        {
            Console.WriteLine("Get drawing for item " + item);
            Drawing.Drawing drawing2 = client.GetDrawingFromServer(item);
            if (drawing2 != null)
            {
                Console.WriteLine("Drawing found for " + drawing2.Item);
                return drawing2;
            }
            else
            {
                Console.WriteLine("No drawing found for " + item);
                return null;
            }
        }

        public Drawing.Drawing GetDrawingById(string id)
        {
            Console.WriteLine("Get drawing with id " + id);
            return client.GetDrawingFromServerById(id);
        }

        public void SaveScore(Drawing.Drawing drawing, int score)
        {
            Console.Write("Saving score "+score+" to "+drawing.ID+": ");
            Console.WriteLine(client.SaveScoreToServer(drawing, "foo", 12));
        }

        public void ShowTables()
        {
            Console.WriteLine("Showing list of connected tables:");
            List<string> clients = client.GetConnectedTablesFromServer();

            foreach (string s in clients)
            {
                Console.WriteLine(s);
            }
        }

        public void ShowHighscores()
        {
            Console.WriteLine("Showing current list of highscores:");
            List<Highscores.Highscore> hs = client.GetHighscoresFromServer();
            hs.ForEach(h => Console.WriteLine("["+h.id+"] "+h.drawingItem+" => "+h.score));
        }

        public void CheckScore(Drawing.Drawing d, ulong score)
        {
            Console.WriteLine("is "+score+" highscore for "+d.ID+"? " + client.CheckScore(d, score));
        }

        static void Main(string[] args)
        {
            ClientProgram cp = new ClientProgram();

            List<string> result = new List<string>();
            result.Add("Batman");
            result.Add("Mickey Mouse");
            result.Add("A cat");
            result.Add("Tintin");
            result.Add("Donald Duck");
            result.Add("A wild Pikachu");
            result.ForEach(it => cp.AddItem(it));

            cp.SaveDrawing(result.First());

            cp.ShowItems();

            Drawing.Drawing d = cp.GetDrawing(result.First());
            cp.GetDrawing(result.Last());

            Drawing.Drawing d2 = cp.GetDrawingById(d.ID);
            cp.GetDrawingById("foobar");

            Console.WriteLine("id equal ? " + (d.ID == d2.ID));
            Console.WriteLine("object equal ? " + d.Equals(d2));

            cp.SaveScore(d, 12);


            cp.ShowTables();

            cp.ShowHighscores();

            cp.CheckScore(d, 1);
            cp.CheckScore(d, 14355);

            Console.WriteLine("That's all, folks! Type ENTER to continue...");
            Console.ReadLine();
            cp.client.Stop();

        }

    }
}
