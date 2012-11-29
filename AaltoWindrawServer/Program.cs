using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AaltoWindrawServer.Server;

namespace AaltoWindrawServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.Server server = new Server.Server();
            Console.WriteLine("Welcome to AaltoWindraw server application");
            if (!server.start())
            {
                Console.WriteLine("Server failed to start");
            }
            Console.WriteLine("That's all, folks! Type anything to continue...");
            Console.ReadLine();
        }
    }
}
