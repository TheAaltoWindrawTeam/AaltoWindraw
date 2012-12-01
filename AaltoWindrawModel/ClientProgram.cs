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

            // Create new client, with previously created configs
            Client client = new Client();

            // Start client
            client.Start();

            client.Stop();

            Console.WriteLine("That's all, folks! Type ENTER to continue...");
            Console.ReadLine();

        }

    }
}
