using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AaltoWindraw.Server;

namespace AaltoWindraw
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            Server.Server server = new Server.Server();
            Console.WriteLine("Welcome to AaltoWindraw server application");
            server.start();
            
            Console.WriteLine("That's all, folks! Type ENTER to continue...");
            Console.ReadLine();
        }
    }
}
