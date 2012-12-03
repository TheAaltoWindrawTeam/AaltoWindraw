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

        static void Main(string[] args)
        {

            Client client = new Client();

            client.Start();

            List<string> items = client.GetItemsFromServer();
            foreach (string item in items)
            {
                Console.WriteLine(item);
            }

            Drawing.Drawing drawing = client.GetDrawingFromServer(items.First());
            Console.WriteLine(drawing.Item);

            Console.Write("Saving score: ");
            Console.WriteLine(client.SaveScoreToServer(drawing, "foo", 12));

            List<string> clients = client.GetConnectedTablesFromServer();

            foreach (string s in clients)
            {
                Console.WriteLine(s);
            }


            Console.Write("Saving drawing: ");
            Console.WriteLine(client.SaveDrawingToServer(drawing));

            List<Highscores.Highscore> hs = client.GetHighscoresFromServer();
            Console.WriteLine(hs.Count);
            hs.ForEach(h => Console.WriteLine(h.score));

            Console.WriteLine("isHiscore? " + client.CheckScore(drawing, 143432));

            Console.WriteLine("finally add item: " + client.AddItemToServer("Basshunter"));

            Console.WriteLine("That's all, folks! Type ENTER to continue...");
            Console.ReadLine();
            client.Stop();

        }

    }
}
