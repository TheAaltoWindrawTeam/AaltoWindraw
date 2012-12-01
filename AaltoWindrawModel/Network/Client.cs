using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AaltoWindraw;
using AaltoWindraw.Properties;
using AaltoWindraw.Network;

namespace AaltoWindraw.Network
{
    class Client
    {
        
        NetClient client;
        NetPeerConfiguration config;

        public Client()
        {
            // Remember that appIdentifier has to be the same between client and server
            config = new NetPeerConfiguration(Properties.Resources.application_protocol_name);

            client = new NetClient(config);
        }

        public void Start()
        {
            Console.WriteLine("Starting client...");

            client.Start();

            NetOutgoingMessage outmsg = client.CreateMessage();

            // Write campus name
            outmsg.Write(Properties.Resources.client_name);

            Console.WriteLine("Connection to server, sending local name...");

            client.Connect(Properties.Resources.server_address,
                 Int32.Parse(Properties.Resources.default_port)
                 , outmsg);

            Console.WriteLine("Client started successfully");
        }
        
        public void Stop()
        {
            Console.WriteLine("Gracefully closing the client...");

            client.Disconnect(Properties.Resources.bye_message);

            Console.WriteLine("The client was successfully closed");
        }

        //TODO implement this, you fool!
        public List<String> GetDrawingNamesFromServer()
        {
            return null;
        }

        public Drawing.Drawing GetDrawingFromServer(string drawingName)
        {
            Drawing.Drawing drawing = new Drawing.Drawing(drawingName);
            string inHash = null;

            this.Start();

            do
            {

            // Send request to server
            NetOutgoingMessage outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.DRAWING_REQUEST);
            outMsg.Write(drawingName);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (and drawing inside it)
            NetIncomingMessage inMsg = GetNextMessageFromServer();
            inMsg.ReadAllProperties(drawing);
            inHash = inMsg.ReadString();

            }   // Re-do as long as the drawing is not equal to its hash on the server
            while(inHash != Utilities.Hash.ComputeHash(drawing));

            this.Stop();

            return drawing;
        }

        //TODO implement this, you fool!
        public bool SaveScoreToServer(string drawingName, string scorer, int score)
        {
            return false;
        }

        //TODO implement this, you fool!
        public List<String> GetConnectedTablesFromServer()
        {
            return null;
        }

        //TODO implement this, you fool!
        public bool SaveDrawingToServer(Drawing.Drawing drawing)
        {
            return false;
        }

        private NetIncomingMessage GetNextMessageFromServer()
        {
            NetIncomingMessage incomingMsg;
            while ((incomingMsg = client.ReadMessage()) == null)
            {
            }
            return incomingMsg;
        }

    }
}
