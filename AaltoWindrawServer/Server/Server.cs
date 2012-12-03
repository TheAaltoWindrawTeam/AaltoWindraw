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

        // Outgoing message, used for communication from server to clients
        NetOutgoingMessage outMsg;

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

                    switch (incomingMsg.ReadByte())
                    {
                        case (byte)Network.Commons.PacketType.DRAWING_REQUEST:

                            string drawingName = incomingMsg.ReadString();

                            // TODO get one correct drawing from drawingName
                            // Sth like:
                            // drawing = db.things.find({name:drawingName}).toArray()[rand.Next(0, n)];

                            // Create new message
                           outMsg = server.CreateMessage();

							// Write drawing data
                            outMsg.Write(NetSerializer.Serialize(drawing));

                            //Write a hash of the drawing to check if communication was successful (maybe overkill?)
                            outMsg.Write(Utilities.Hash.ComputeHash(drawing));

                            // Send message to client (in reliable order, channel 0)
                            server.SendMessage(outMsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;  // End of DRAWING_REQUEST part

                        case (byte)Network.Commons.PacketType.SEND_SCORE:

                            Highscore highscore = NetSerializer.DeSerialize<Highscore>(incomingMsg.ReadString());

                            //TODO implement storing a score...
                            bool scoreCorrectlyStored = true;

                            outMsg = server.CreateMessage();

                            outMsg.Write(scoreCorrectlyStored ?
                                (byte)Network.Commons.PacketType.SCORE_STORED :
                                (byte)Network.Commons.PacketType.SCORE_NOT_STORED);

                            server.SendMessage(outMsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;  // End of SEND_SCORE part

                        case (byte)Network.Commons.PacketType.WHO_REQUEST:

                            // Send a list of connected users (tables actually)
                            outMsg = server.CreateMessage();

                            outMsg.Write(connectedTables.Count);

                            IEnumerator<string> enumerator = connectedTables.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                outMsg.Write(enumerator.Current);
                            }

                            server.SendMessage(outMsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;  // End of WHO_REQUEST part

                        case (byte)Network.Commons.PacketType.SEND_DRAWING:

                            drawing = NetSerializer.DeSerialize<Drawing.Drawing>(incomingMsg.ReadString());

                            string hash = incomingMsg.ReadString();

                            string localHash = Utilities.Hash.ComputeHash(drawing);


                            // Send acknowledgement (Drawing successfully received or not)
                            outMsg = server.CreateMessage();

                            if(hash.Equals(localHash))
                            {
                                outMsg.Write((byte)Network.Commons.PacketType.DRAWING_STORED);

                                //TODO add drawing to DB
                            }
                            else
                            {
                                outMsg.Write((byte)Network.Commons.PacketType.DRAWING_NOT_STORED);
                            }

                            server.SendMessage(outMsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;  // End of SEND_DRAWING part


                        case (byte)Network.Commons.PacketType.ITEMS_REQUEST:

                            List<string> items = GetItemsList();

                            // Send a list of available items
                            outMsg = server.CreateMessage();

                            outMsg.Write(items.Count);

                            IEnumerator<string> itemEnumerator = items.GetEnumerator();
                            while (itemEnumerator.MoveNext())
                            {
                                outMsg.Write(itemEnumerator.Current);
                            }

                            server.SendMessage(outMsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;  // End of ITEMS_REQUEST part


                        case (byte)Network.Commons.PacketType.IS_HIGHSCORE_REQUEST:

                            string item = incomingMsg.ReadString();
                            string author = incomingMsg.ReadString();
                            DateTime timestamp = NetSerializer.DeSerialize<DateTime>(incomingMsg.ReadString());
                            ulong score = incomingMsg.ReadUInt64();

                            // Check into DB
                            bool isHighscore = true;

                            
                            // Send answer
                            outMsg = server.CreateMessage();

                            if (isHighscore)
                            {
                                outMsg.Write((byte)Network.Commons.PacketType.IS_HIGHSCORE);
                            }
                            else
                            {
                                outMsg.Write((byte)Network.Commons.PacketType.IS_NOT_HIGHSCORE);
                            }

                            server.SendMessage(outMsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;  // End of IS_HIGHSCORE_REQUEST part


                        case (byte)Network.Commons.PacketType.SEND_ITEM:

                            string itemSent = incomingMsg.ReadString();

                            // Check if item is already in the database
                            // (or if there is a similar enough one)
                            bool isAdded = true;


                            outMsg = server.CreateMessage();


                            if (isAdded)
                            {
                                outMsg.Write((byte)Network.Commons.PacketType.ITEM_SAVED);
                                // Add item to DB
                            }
                            else
                            {
                                outMsg.Write((byte)Network.Commons.PacketType.ITEM_NOT_SAVED);
                            }

                            server.SendMessage(outMsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;  // End of SEND_ITEM part


                        case (byte)Network.Commons.PacketType.HIGHSCORES_REQUEST:

                            List<Highscore> highscores = GetHighscoresList();

                            // Send a list of highscores
                            outMsg = server.CreateMessage();

                            outMsg.Write(highscores.Count);

                            foreach(Highscore hs in highscores)
                            {
                                outMsg.Write(NetSerializer.Serialize(hs));
                            }

                            server.SendMessage(outMsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;  // End of HIGHSCORES_REQUEST part
					}

					break;  // End of Data part


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
