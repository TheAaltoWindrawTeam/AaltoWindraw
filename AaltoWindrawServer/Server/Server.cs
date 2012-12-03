using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AaltoWindraw;
using AaltoWindraw.Properties;
using AaltoWindraw.Network;
using AaltoWindraw.Highscores;
using AaltoWindraw.Utilities;

namespace AaltoWindraw.Server
{
	class Server
	{
		// Server and associated configuration objects
		NetServer server;
		NetPeerConfiguration config;

        Dictionary<System.Net.IPEndPoint, string> campusLookup;
        List<string> connectedTables;

        // Incoming message, for client to server communication
		NetIncomingMessage incomingMsg;


        //TODO remove me when DB is ready!
        Drawing.Drawing drawing = new Drawing.Drawing("server_test");


		public Server()
		{
			campusLookup = new Dictionary<System.Net.IPEndPoint,string>();
            connectedTables = new List<string>();
		}

		public void start()
		{
			config = new NetPeerConfiguration(Properties.Resources.application_protocol_name);
			config.Port = int.Parse(Properties.Resources.default_port);
			config.MaximumConnections = int.Parse(Properties.Resources.maximum_connections);
			config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
			
			server = new NetServer(config);

			server.Start();
			
			Log("AaltoWindraw server started successfully");

			while (true)
			{
				if ((incomingMsg = server.ReadMessage()) != null)
				{
					AnalyzeMessage(incomingMsg);
				}

				// Let's not eat all CPU
				System.Threading.Thread.Sleep(45);
			}
		}

		private void AnalyzeMessage(NetIncomingMessage incomingMsg)
		{
			// There are two kinds of message here:
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
                    byte[] buf = new byte[incomingMsg.LengthBytes];
                    incomingMsg.ReadBytes(buf, 0, incomingMsg.LengthBytes);
                    Log(incomingMsg.MessageType+": "+System.Text.Encoding.ASCII.GetString(buf));
					break;
			}
		}

        private NetOutgoingMessage AnalyzeDataMessage(NetIncomingMessage inMsg)
        {

            NetOutgoingMessage outMsg = server.CreateMessage();

            switch (inMsg.ReadByte())
            {
                case (byte)Network.Commons.PacketType.DRAWING_REQUEST:

                    string drawingName = inMsg.ReadString();

                    // TODO get one correct drawing from drawingName
                    // Sth like:
                    // drawing = db.things.find({name:drawingName}).toArray()[rand.Next(0, n)];

                    // Write drawing data
                    outMsg.Write(NetSerializer.Serialize(drawing));

                    //Write a hash of the drawing to check if communication was successful (maybe overkill?)
                    outMsg.Write(Utilities.Hash.ComputeHash(drawing));

                    break;  // End of DRAWING_REQUEST part

                case (byte)Network.Commons.PacketType.SEND_SCORE:

                    Highscore highscore = NetSerializer.DeSerialize<Highscore>(inMsg.ReadString());

                    //TODO implement storing a score...
                    bool scoreCorrectlyStored = true;

                    outMsg.Write(scoreCorrectlyStored ?
                        (byte)Network.Commons.PacketType.SCORE_STORED :
                        (byte)Network.Commons.PacketType.SCORE_NOT_STORED);

                    break;  // End of SEND_SCORE part

                case (byte)Network.Commons.PacketType.WHO_REQUEST:

                    outMsg.Write(connectedTables.Count);

                    connectedTables.ForEach(table => outMsg.Write(table));

                    break;  // End of WHO_REQUEST part

                case (byte)Network.Commons.PacketType.SEND_DRAWING:

                    drawing = NetSerializer.DeSerialize<Drawing.Drawing>(inMsg.ReadString());

                    string hash = inMsg.ReadString();

                    string localHash = Utilities.Hash.ComputeHash(drawing);


                    if (hash.Equals(localHash))
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.DRAWING_STORED);

                        //TODO add drawing to DB
                    }
                    else
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.DRAWING_NOT_STORED);
                    }

                    break;  // End of SEND_DRAWING part


                case (byte)Network.Commons.PacketType.ITEMS_REQUEST:

                    List<string> items = GetItemsList();

                    outMsg.Write(items.Count);

                    items.ForEach(it => outMsg.Write(it));

                    break;  // End of ITEMS_REQUEST part


                case (byte)Network.Commons.PacketType.IS_HIGHSCORE_REQUEST:

                    string item = inMsg.ReadString();
                    string author = inMsg.ReadString();
                    DateTime timestamp = NetSerializer.DeSerialize<DateTime>(inMsg.ReadString());
                    ulong score = inMsg.ReadUInt64();

                    // Check into DB
                    bool isHighscore = true;

                    if (isHighscore)
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.IS_HIGHSCORE);
                    }
                    else
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.IS_NOT_HIGHSCORE);
                    }

                    break;  // End of IS_HIGHSCORE_REQUEST part


                case (byte)Network.Commons.PacketType.SEND_ITEM:

                    string itemSent = inMsg.ReadString();

                    // Check if item is already in the database
                    // (or if there is a similar enough one)
                    bool isAdded = true;


                    if (isAdded)
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.ITEM_SAVED);
                        // Add item to DB
                    }
                    else
                    {
                        outMsg.Write((byte)Network.Commons.PacketType.ITEM_NOT_SAVED);
                    }

                    break;  // End of SEND_ITEM part


                case (byte)Network.Commons.PacketType.HIGHSCORES_REQUEST:

                    outMsg.Write(NetSerializer.Serialize(GetHighscoresList()));

                    break;  // End of HIGHSCORES_REQUEST part
            }

            return outMsg;
        }

        //TODO implement proper highscore lookup
        private List<Highscore> GetHighscoresList()
        {
            List<Highscore> result = new List<Highscore>();
            return result;
        }

        //TODO implement proper item lookup
        private List<string> GetItemsList()
        {
            List<string> result = new List<string>();
            result.Add("Batman");
            result.Add("Mickey Mouse");
            result.Add("A cat");
            result.Add("Tintin");
            result.Add("Donald Duck");
            result.Add("A wild Pikachu");

            return result;
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
                Log("Connection opened from " + table);
            }
        }

        private void RemoveTable(System.Net.IPEndPoint ip)
        {
            string table = GetTableName(ip);
            if (connectedTables.Contains(table))
            {
                this.connectedTables.Remove(table);
                Log("Connection closed from " + table);
            }
        }

        private void Log(string s)
        {
            Console.WriteLine(s);
        }
	}

}
