using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AaltoWindraw.Server;
using System.Threading;

namespace AaltoWindraw
{
    class ServerProgram
    {
        static Server.Server server;

        static string[] defaultItems = {
            "wheel",
            "island",
            "turtle",
            "chair",
            "ear",
            "shoe", 
            "basketball",
            "octopus",
            "bed",
            "flag",
            "castle",
            "paint",
            "car",
            "horse", 
            "pinwheel",
            "kite", 
            "safetypin",
            "submarine",
            "watermelon",
            "tea",
            "telephone",
            "whistle",
            "piano",
            "clam",
            "ring",
            "frog",
            "olive",
            "mailman",
            "mountain",
            "camel",
            "wind",
            "summer",
            "green",
            "surfboard",
            "cow",
            "pencil",
            "shower",
            "glasses",
            "stove",
            "chimney",
            "window",
            "rainbow",
            "moon",
            "peacock",
            "sky",
            "ocean", 
            "volcano",
            "dinosaur",
            "whale",
            "elephant",
            "flea",
            "snail",
            "fireplace",
            "forest",
            "spoon",
            "lace",
            "gasoline",
            "rice",
            "honeybee",
            "shoulderpad"
                                       };

        static void Main(string[] args)
        {

            Thread exitThread = new Thread(new ThreadStart(Exit));
            exitThread.Start();

            server = new Server.Server();
            Console.WriteLine();
            Console.WriteLine("Welcome to AaltoWindraw server application!");
            Console.WriteLine();
            Console.WriteLine("** Press ENTER at any moment to exit **");
            Console.WriteLine();
            Console.WriteLine();
            server.Start();
        }

        public static void Exit()
        {
            Thread.Sleep(2000);
            if (server.DB().IsEmpty())
            {
                foreach (string item in defaultItems)
                {
                    server.DB().SaveItem(item);
                }
                Console.WriteLine("Default items loaded into database");
            }
            Console.ReadLine();
            server.Stop();
            Environment.Exit(0);
        }
    }
}
