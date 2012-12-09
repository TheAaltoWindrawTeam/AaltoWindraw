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
            Console.ReadLine();
            server.Stop();
            Environment.Exit(0);
        }
    }
}
