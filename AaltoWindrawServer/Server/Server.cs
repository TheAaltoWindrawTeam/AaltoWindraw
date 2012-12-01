using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AaltoWindraw;
using AaltoWindraw.Properties;
using AaltoWindraw.Network;
using AaltoWindraw.Highscores;

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
			
			Console.WriteLine("AaltoWindraw server started successfully");

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
				   
				    Console.WriteLine("Connection opened from "+campus);

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
                            // Sth like db.things.find({name:drawingName}).toArray()[rand.Next(0, n)];

                            // Create new message
                           outMsg = server.CreateMessage();

							// Write drawing data
                            //TODO implement real drawing search
							outMsg.WriteAllProperties(drawing);

                            //Write a hash of the drawing to check if communication was successful (maybe overkill?)
                            outMsg.Write(Utilities.Hash.ComputeHash(drawing));

                            // Send message to client (in reliable order, channel 0)
                            server.SendMessage(outMsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;  // End of DRAWING_REQUEST part

                        case (byte)Network.Commons.PacketType.SEND_SCORE:

                            Highscore highscore = new Highscore();
                            incomingMsg.ReadAllProperties(highscore);

                            //TODO implement storing a score...

                            outMsg = server.CreateMessage();

                            outMsg.Write((byte)Network.Commons.PacketType.SCORE_RECEIVED);

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

                            drawing = new Drawing.Drawing();

                            incomingMsg.ReadAllProperties(drawing);

                            string hash = incomingMsg.ReadString();

                            string localHash = Utilities.Hash.ComputeHash(drawing);


                            // Send acknowledgement (Drawing successfully received or not)
                            outMsg = server.CreateMessage();

                            if(hash.Equals(localHash))
                            {
                                outMsg.Write((byte)Network.Commons.PacketType.DRAWING_RECEIVED);

                                //TODO add drawing to DB
                            }
                            else
                            {
                                outMsg.Write((byte)Network.Commons.PacketType.DRAWING_NOT_RECEIVED);
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
					}

					break;  // End of Data part


                case NetIncomingMessageType.StatusChanged:

                    //TODO get rid of following line the day it gets useless
                    Console.WriteLine("New state for "
                    + GetTableName(incomingMsg.SenderEndpoint)
                    + ": "
                    + incomingMsg.SenderConnection.Status);

                    switch (incomingMsg.SenderConnection.Status)
                    {
                        case NetConnectionStatus.Connected:
                            AddTable(incomingMsg.SenderEndpoint);
                            break;
                        case NetConnectionStatus.Connecting:
                            AddTable(incomingMsg.SenderEndpoint);
                            break;
                        case NetConnectionStatus.Disconnected:
                            RemoveTable(incomingMsg.SenderEndpoint);
                            break;
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
					Console.WriteLine("Not Important Message? "+incomingMsg.ToString());
                    byte[] buf = new byte[incomingMsg.LengthBytes];
                    incomingMsg.ReadBytes(buf, 0, incomingMsg.LengthBytes);
                    Console.WriteLine("Content: "+System.Text.Encoding.ASCII.GetString(buf) + "\n");
					break;
			}
		}

        private List<string> GetItemsList()
        {
            throw new NotImplementedException();
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
                connectedTables.Add(table);
        }

        private void RemoveTable(System.Net.IPEndPoint ip)
        {
            string table = GetTableName(ip);
            if (connectedTables.Contains(table))
                this.connectedTables.Remove(table);
        }
	}

}
