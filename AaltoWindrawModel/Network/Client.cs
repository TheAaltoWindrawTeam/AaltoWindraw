using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AaltoWindraw;
using AaltoWindraw.Properties;
using AaltoWindraw.Network;
using AaltoWindraw.Utilities;
using System.Net;
using System.Net.Sockets;

namespace AaltoWindraw.Network
{
    public class Client
    {
        NetClient client;
        NetPeerConfiguration config;
        NetOutgoingMessage outMsg;
        NetIncomingMessage inMsg;
        IPEndPoint serverEndPoint;

        private bool connected;

        public Client()
        {
            connected = false;

            // Remember that appIdentifier has to be the same between client and server
            config = new NetPeerConfiguration(Properties.Resources.application_protocol_name);
            client = new NetClient(config);

            serverEndPoint = new IPEndPoint(NetUtility.Resolve(Properties.Resources.server_address),
                               Int32.Parse(Properties.Resources.default_port));
        }

        public bool IsConnected()
        {
            return this.connected;
        }

        public bool Start()
        {
            if (connected)
            {
                Console.WriteLine("Client is already connected");
                return false;
            }
            
            if (!CheckServerAvailability())
            {
                Console.WriteLine("Server seems to be down... Try again later!");
                return false;
            }
            
            Console.WriteLine("Connection to " 
                + Properties.Resources.server_address 
                + ", sending local name (" 
                + Properties.Resources.client_name 
                + ")...");

            client.Start();

            outMsg = client.CreateMessage();

            // Write campus name
            outMsg.Write(Properties.Resources.client_name);

            NetConnection nc = client.Connect(serverEndPoint, outMsg);
            connected = true;
            return connected;
        }
        
        public bool Stop()
        {
            if (!connected)
                return false;

            Console.WriteLine("Gracefully closing the connection...");

            client.Disconnect(Properties.Resources.bye_message);
            client.Shutdown(Properties.Resources.bye_message);
            connected = false;

            Console.WriteLine("The connection was successfully closed");
            return !connected;
        }

        public bool CheckServerAvailability()
        {
            try
            {
                TcpClient ucs = new TcpClient();
                ucs.Connect(serverEndPoint);
                ucs.Close();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public List<string> GetItemsFromServer()
        {
            outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.ITEMS_REQUEST);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            inMsg = NextDataMessageFromServer();

            return NetSerializer.DeSerialize<List<string>>(inMsg.ReadString());
        }

        public Drawing.Drawing GetDrawingFromServer(string item)
        {
            // Send request to server
            outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.DRAWING_REQUEST);
            outMsg.Write(item);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (and drawing inside it)
            inMsg = NextDataMessageFromServer();
            if (inMsg.ReadByte().Equals((byte)Network.Commons.PacketType.NO_DRAWING_FOUND))
                return null;

            // Somehow serialization do not take into account the ID...
            // So here's a hack
            string id = inMsg.ReadString();
            Drawing.Drawing drawing = NetSerializer.DeSerialize<Drawing.Drawing>(inMsg.ReadString());
            drawing.ID = id;

            return drawing;
        }

        public Drawing.Drawing GetDrawingFromServerById(string drawingID)
        {
            // Send request to server
            outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.DRAWING_BY_ID_REQUEST);
            outMsg.Write(drawingID);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (and drawing inside it)
            inMsg = NextDataMessageFromServer();
            if (inMsg.ReadByte().Equals((byte)Network.Commons.PacketType.NO_DRAWING_FOUND))
                return null;
            // Somehow serialization do not take into account the ID...
            // So here's a hack
            string id = inMsg.ReadString();
            Drawing.Drawing drawing = NetSerializer.DeSerialize<Drawing.Drawing>(inMsg.ReadString());
            drawing.ID = id;
            
            return drawing;
        }

        public bool SaveScoreToServer(Drawing.Drawing drawing, string scorer, ulong score)
        {
            Highscores.Highscore highscore = new Highscores.Highscore(drawing, scorer, score, DateTime.Now);

            // Send request to server
            outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.SEND_SCORE);
            outMsg.Write(NetSerializer.Serialize(highscore));
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (check if sending ok)
            inMsg = NextDataMessageFromServer();

            return inMsg.ReadByte() == (byte)Commons.PacketType.SCORE_STORED;
        }

        public List<string> GetConnectedTablesFromServer()
        {
            // Send request to server
            outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.WHO_REQUEST);
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (list of tables)
            inMsg = NextDataMessageFromServer();
            return NetSerializer.DeSerialize<List<string>>(inMsg.ReadString());
        }

        public bool SaveDrawingToServer(Drawing.Drawing drawing)
        {
            drawing.Save();

            // Send request to server
            outMsg = client.CreateMessage();
            outMsg.Write((byte)Commons.PacketType.SEND_DRAWING);
            outMsg.Write(NetSerializer.Serialize(drawing));
            client.SendMessage(outMsg, NetDeliveryMethod.ReliableOrdered);

            // Read response (check if sending ok)
            inMsg = NextDataMessageFromServer();

            return inMsg.ReadByte() == (byte)Commons.PacketType.DRAWING_STORED;
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
            this.outMsg.Write(drawing.ID);
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

    }
}
