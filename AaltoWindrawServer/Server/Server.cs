﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AaltoWindraw;
using AaltoWindraw.Properties;
using AaltoWindraw.Network;
using AaltoWindraw.Highscores;
using AaltoWindraw.Utilities;
using AaltoWindraw.Database;
using System.Net.Sockets;
using System.Threading;

namespace AaltoWindraw.Server
{
	class Server
	{
		// Server and associated configuration objects
		private NetServer server;
        private NetPeerConfiguration config;

        private Dictionary<System.Net.IPEndPoint, string> campusLookup;
        private List<string> connectedTables;

        // Incoming message, for client to server communication
        private NetIncomingMessage incomingMsg;

        private MongoDBManager db;

        private Random rand;

		public Server()
		{
			campusLookup = new Dictionary<System.Net.IPEndPoint,string>();
            connectedTables = new List<string>();
            db = new MongoDBManager();
            rand = new Random();
		}

        public void Init()
        {
            bool dbOk = db.Start();
            if (dbOk)
            {
                Console.WriteLine("Database loaded successfully");
            }
            else
            {
                Console.WriteLine("Database could not be loaded...");
                return;
            }

            // Start server
            config = new NetPeerConfiguration(Properties.Resources.application_protocol_name);
            config.Port = int.Parse(Properties.Resources.default_port);
            config.MaximumConnections = int.Parse(Properties.Resources.maximum_connections);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            server = new NetServer(config);

            server.Start();

        }

        public void Start()
        {        
            Thread upThread = new Thread(new ThreadStart(TellAvailability));
            upThread.Start();

            while (true)
            {
                if ((incomingMsg = server.ReadMessage()) != null)
                {
                    AnalyzeMessage(incomingMsg);
                    server.Recycle(incomingMsg);
                }

                // Let's not eat all CPU
                System.Threading.Thread.Sleep(45);
            }
        }

        public void Stop()
        {
            server.Shutdown(Properties.Resources.bye_message);
            db.Stop();
        }

        public MongoDBManager DB()
        {
            return db;
        }

		private void AnalyzeMessage(NetIncomingMessage incomingMsg)
		{
			// There are several kinds of message here:
            // ConnectionApproval, when client connects
            // Data, for manual sendings
            // StatusChanged, when a client's status changes
			switch (incomingMsg.MessageType)
			{
                    
				// Very first message sent from client
				case NetIncomingMessageType.ConnectionApproval:

                    // Let's add this connection to the list...
				    string campus = incomingMsg.ReadString();

				    // Approve client's connection (a kind of agreement)
				    incomingMsg.SenderConnection.Approve();

                    AddToLookup(incomingMsg.SenderEndpoint, campus);
					
	    			break;  // End of Approval part
                

				case NetIncomingMessageType.Data:

                    // Send proper response to client (in reliable order, channel 0)
                    server.SendMessage(AnalyzeDataMessage(incomingMsg), incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                    break;

                case NetIncomingMessageType.StatusChanged:

                    switch (incomingMsg.SenderConnection.Status)
                    {
                        case NetConnectionStatus.Connected:
                        case NetConnectionStatus.Connecting:
                            AddTable(incomingMsg.SenderEndpoint);
                            break;
                        case NetConnectionStatus.Disconnected:
                        case NetConnectionStatus.Disconnecting:
                            RemoveTable(incomingMsg.SenderEndpoint);
                            break;
                        case NetConnectionStatus.None:
                            break;
                    }

                    break;  // End of StatusChanged part

				default:
					
                    // Messages received but not managed
                    // They are far from scarce!
                    //byte[] buf = new byte[incomingMsg.LengthBytes];
                    //incomingMsg.ReadBytes(buf, 0, incomingMsg.LengthBytes);
                    //Log(incomingMsg.MessageType+": "+System.Text.Encoding.ASCII.GetString(buf));
					break;
			}
		}

        private NetOutgoingMessage AnalyzeDataMessage(NetIncomingMessage inMsg)
        {

            NetOutgoingMessage outMsg = server.CreateMessage();

            switch (inMsg.ReadByte())
            {
                case (byte)Network.Commons.PacketType.DRAWING_REQUEST:
                    Drawing.Drawing[] drawings = db.GetDrawingsByItem(inMsg.ReadString());
                    if (drawings.Length == 0)
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.NO_DRAWING_FOUND);
                    }
                    else
                    {
                        Drawing.Drawing drawing = drawings[rand.Next(0, drawings.Length)];
                        outMsg.Write((byte)Network.Commons.PacketType.DRAWING_FOUND);
                        writeDrawing(drawing, outMsg);
                    }
                    break;  // End of DRAWING_REQUEST part

                case (byte)Network.Commons.PacketType.DRAWING_BY_ID_REQUEST:
                    Drawing.Drawing d = db.GetDrawingById(inMsg.ReadString());
                    if (d == null)
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.NO_DRAWING_FOUND);
                    }
                    else
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.DRAWING_FOUND);
                        writeDrawing(d, outMsg);
                    }
                    break;  // End of DRAWING__BY_ID_REQUEST part

                case (byte)Network.Commons.PacketType.SEND_SCORE:
                    outMsg.Write(db.SaveScore(NetSerializer.DeSerialize<Highscore>(inMsg.ReadString())) ?
                        (byte)Network.Commons.PacketType.SCORE_STORED :
                        (byte)Network.Commons.PacketType.SCORE_NOT_STORED);
                    break;  // End of SEND_SCORE part

                case (byte)Network.Commons.PacketType.WHO_REQUEST:
                    outMsg.Write(NetSerializer.Serialize(connectedTables));
                    break;  // End of WHO_REQUEST part

                case (byte)Network.Commons.PacketType.SEND_DRAWING:
                    outMsg.Write((db.SaveDrawing(NetSerializer.DeSerialize<Drawing.Drawing>(inMsg.ReadString()))) ?
                        (byte)Network.Commons.PacketType.DRAWING_STORED :
                        (byte)Network.Commons.PacketType.DRAWING_NOT_STORED);
                    break;  // End of SEND_DRAWING part

                case (byte)Network.Commons.PacketType.ITEMS_REQUEST:
                    outMsg.Write(NetSerializer.Serialize(db.GetItems()));
                    break;  // End of ITEMS_REQUEST part

                case (byte)Network.Commons.PacketType.HIGHSCORE_REQUEST:
                    string id = inMsg.ReadString();
                    Highscore h = db.GetHighscoreById(id);
                    if (h == null)
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.HIGHSCORE_NOT_FOUND);
                    }
                    else
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.HIGHSCORE_FOUND);
                        outMsg.Write(NetSerializer.Serialize(h));
                    }
                    break;  // End of HIGHSCORE_REQUEST part

                case (byte)Network.Commons.PacketType.SEND_ITEM:
                    outMsg.Write(db.SaveItem(inMsg.ReadString()) ?
                        (byte)Network.Commons.PacketType.ITEM_SAVED :
                        (byte)Network.Commons.PacketType.ITEM_NOT_SAVED);
                    break;  // End of SEND_ITEM part

                case (byte)Network.Commons.PacketType.HIGHSCORES_REQUEST:
                    outMsg.Write(NetSerializer.Serialize(db.GetHighscores()));
                    break;  // End of HIGHSCORES_REQUEST part
            }

            return outMsg;
        }

        private void writeDrawing(Drawing.Drawing d, NetOutgoingMessage outMsg)
        {
            // Somehow serialization do not take into account the ID...
            // So here's a hack
            outMsg.Write(d.ID);

            // Write drawing data
            outMsg.Write(NetSerializer.Serialize(d));
        }

        private string GetTableName(System.Net.IPEndPoint ip)
        {
            return campusLookup.ContainsKey(ip) ? campusLookup[ip] : ip.ToString();
        }

        private void AddToLookup(System.Net.IPEndPoint ip, string table)
        {
            if (!campusLookup.ContainsKey(ip))
                this.campusLookup.Add(ip, table);
        }

        private void AddTable(System.Net.IPEndPoint ip)
        {
            string table = GetTableName(ip);
            if (!connectedTables.Contains(table))
            {
                connectedTables.Add(table);
            }
        }

        private void RemoveTable(System.Net.IPEndPoint ip)
        {
            string table = GetTableName(ip);
            if (connectedTables.Contains(table))
            {
                this.connectedTables.Remove(table);
            }
        }

        private static void TellAvailability()
        {
            TcpListener listener = new TcpListener(NetUtility.Resolve(Properties.Resources.server_address), int.Parse(Properties.Resources.default_port));
            listener.Start();

            while (true)
            {
                // Is someone trying to call us? Well answer!
                TcpClient tempClient = listener.AcceptTcpClient();
                tempClient.Close();
                Thread.Sleep(1200);
            }
        }
	}

}
