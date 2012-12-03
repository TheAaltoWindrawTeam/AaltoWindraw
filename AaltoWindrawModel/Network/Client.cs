using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AaltoWindraw;
using AaltoWindraw.Properties;
using AaltoWindraw.Network;
using AaltoWindraw.Utilities;

namespace AaltoWindraw.Network
{
    class Client
    {
        
        NetClient client;
        NetPeerConfiguration config;
        NetOutgoingMessage outMsg;
        NetIncomingMessage inMsg;

        private bool connected;

        public Client()
        {
            connected = false;

            // Remember that appIdentifier has to be the same between client and server
            config = new NetPeerConfiguration(Properties.Resources.application_protocol_name);

            client = new NetClient(config);
        }

        public bool isConnected()
        {
            return this.connected;
        }

        public void Start()
        {
            if (connected)
                return;

            Console.WriteLine("Connection to " 
                + Properties.Resources.server_address 
                + ", sending local name (" 
                + Properties.Resources.client_name 
                + ")...");

            client.Start();

            outMsg = client.CreateMessage();

            // Write campus name
            outMsg.Write(Properties.Resources.client_name);

            client.Connect(Properties.Resources.server_address,
                 Int32.Parse(Properties.Resources.default_port)
                 , outMsg);
            connected = true;

        }
        
        public void Stop()
        {
            if (!connected)
                return;

            Console.WriteLine("Gracefully closing the connection...");

            client.Disconnect(Properties.Resources.bye_message);
            connected = false;

            Console.WriteLine("The connection was successfully closed");
        }

        public List<string> GetItemsFromServer()
        {            
            List<string> items = new List<string>();
            
            outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.ITEMS_REQUEST);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            inMsg = NextDataMessageFromServer();

            int itemsCount = inMsg.ReadInt32();
            for (int i = 0; i < itemsCount; i++)
            {
                items.Add(inMsg.ReadString());
            }

            return items;
        }

        public Drawing.Drawing GetDrawingFromServer(string drawingName)
        {
            Drawing.Drawing drawing = new Drawing.Drawing(drawingName);
            string inHash = "";

            int attempt = 0;

            do
            {

            // Send request to server
            outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.DRAWING_REQUEST);
            outMsg.Write(drawingName);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (and drawing inside it)
            inMsg = NextDataMessageFromServer();
            drawing = NetSerializer.DeSerialize<Drawing.Drawing>(inMsg.ReadString());
            inHash = inMsg.ReadString();

            }   // Re-do as long as the drawing is not equal to its hash on the server
            while (inHash != Utilities.Hash.ComputeHash(drawing) && attempt++ < Int32.Parse(Properties.Resources.maximum_attempts));

            return drawing;
        }

        public bool SaveScoreToServer(Drawing.Drawing drawing, string scorer, ulong score)
        {
            int attempt = 0;
            bool saveOk = false;
            Highscores.Highscore highscore = new Highscores.Highscore(drawing.Item, drawing.Author, scorer, score);

            do{
                // Send request to server
                outMsg = client.CreateMessage();
                outMsg.Write((byte)Commons.PacketType.SEND_SCORE);
                outMsg.Write(NetSerializer.Serialize(highscore));
                client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

                // Read response (check if sending ok)
                inMsg = NextDataMessageFromServer();

                saveOk = inMsg.ReadByte() == (byte)Commons.PacketType.SCORE_STORED;
            }
            while(!saveOk && attempt++ < Int32.Parse(Properties.Resources.maximum_attempts));

            return saveOk;
        }

        public List<string> GetConnectedTablesFromServer()
        {
            List<string> connectedTables = new List<string>();

            // Send request to server
            outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.WHO_REQUEST);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (list of tables)
            inMsg = NextDataMessageFromServer();
            int tablesCount = inMsg.ReadInt32();
            for (int i = 0; i < tablesCount; i++)
            {
                connectedTables.Add(inMsg.ReadString());
            }

            return connectedTables;
        }

        public bool SaveDrawingToServer(Drawing.Drawing drawing)
        {
            int attempt = 0;
            bool saveOk = false;

            do
            {
                // Send request to server
                outMsg = client.CreateMessage();
                outMsg.Write((byte)Commons.PacketType.SEND_DRAWING);
                outMsg.Write(NetSerializer.Serialize(drawing));
                outMsg.Write(Hash.ComputeHash(drawing));
                client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

                // Read response (check if sending ok)
                inMsg = NextDataMessageFromServer();

                saveOk = inMsg.ReadByte() == (byte)Commons.PacketType.DRAWING_STORED;
            }
            while (!saveOk && attempt++ < Int32.Parse(Properties.Resources.maximum_attempts));

            return saveOk;
        }

        /*
         * Check if a score is a highscore for a given drawing
         * Return true if this is a highscore (hence score may be saved once scorer
         * name is known)
         */
        public bool CheckScore(Drawing.Drawing drawing, ulong score)
        {
            // Send request to server
            this.outMsg = client.CreateMessage();
            this.outMsg.Write((byte)Commons.PacketType.IS_HIGHSCORE_REQUEST);
            this.outMsg.Write(drawing.Item);
            this.outMsg.Write(drawing.Author);
            this.outMsg.Write(NetSerializer.Serialize(drawing.Timestamp));
            this.outMsg.Write(score);

            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (check if highscore)
            inMsg = NextDataMessageFromServer();

            return inMsg.ReadByte() == (byte)Commons.PacketType.IS_HIGHSCORE;
        }

        public bool AddItemToServer(string item)
        {
            // Send request to server
            this.outMsg = client.CreateMessage();
            this.outMsg.Write((byte)Commons.PacketType.SEND_ITEM);
            this.outMsg.Write(item);

            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (check if highscore)
            inMsg = NextDataMessageFromServer();

            return inMsg.ReadByte() == (byte)Commons.PacketType.ITEM_SAVED;
        }

        public List<Highscores.Highscore> GetHighscoresFromServer()
        {
            List<Highscores.Highscore> result = new List<Highscores.Highscore>();

            // Send request to server
            this.outMsg = client.CreateMessage();
            this.outMsg.Write((byte)Commons.PacketType.HIGHSCORES_REQUEST);

            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response
            inMsg = NextDataMessageFromServer();

            return NetSerializer.DeSerialize<List<Highscores.Highscore>>(inMsg.ReadString());
        }

        private NetIncomingMessage NextDataMessageFromServer()
        {
            NetIncomingMessage incomingMsg;
            while ((incomingMsg = client.ReadMessage()) == null || incomingMsg.MessageType != NetIncomingMessageType.Data) ;
            return incomingMsg;
        }

        //TODO remove following line when no more debug is intended
        private void DebugNextDataMessageFromServer()
        {
            NetIncomingMessage incomingMsg;
            while ((incomingMsg = client.ReadMessage()) == null || incomingMsg.MessageType != NetIncomingMessageType.Data)
            {
            }
            Console.Write(incomingMsg.LengthBytes+"  ");
            byte[] buf = new byte[incomingMsg.LengthBytes];
            incomingMsg.ReadBytes(buf, 0, incomingMsg.LengthBytes);
            Console.WriteLine("Content: " + System.Text.Encoding.ASCII.GetString(buf) + "\n");
        }

    }
}
