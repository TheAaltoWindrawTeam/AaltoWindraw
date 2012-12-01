using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AaltoWindraw;
using AaltoWindraw.Properties;
using AaltoWindraw.Network;

namespace AaltoWindraw.Server
{
	class Server
	{
		// Server and associated configuration objects
		NetServer server;
		NetPeerConfiguration config;

		List<string> connectedTables;

        //TODO remove me when DB is ready!
        Drawing.Drawing drawing = new Drawing.Drawing("server_test");


		public Server()
		{
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

			NetIncomingMessage incomingMsg;

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

				    if(!connectedTables.Contains(campus))
    					this.connectedTables.Add(campus);
					
	    			break;  // End of Approval part


				case NetIncomingMessageType.Data:

                    switch (incomingMsg.ReadByte())
                    {
                        case (byte)Network.Commons.PacketType.DRAWING_REQUEST:

                            // Create new message
                            NetOutgoingMessage outmsg = server.CreateMessage();

							// Write drawing data
                            //TODO implement real drawing search
							outmsg.WriteAllProperties(drawing);

                            //Write a hash of the drawing to check if communication was successful (maybe overkill?)
                            outmsg.Write(Utilities.Hash.ComputeHash(drawing));

                            // Send message to client (in reliable order, channel 0)
                            server.SendMessage(outmsg, incomingMsg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);

                            break;
					}

					break;  // End of Data part


                case NetIncomingMessageType.StatusChanged:

                    // For now, only write status changes, later on act when necessary
                    Console.WriteLine("New state for "
                    + incomingMsg.SenderConnection.Owner.Configuration.AppIdentifier
                    + ": "
                    + incomingMsg.SenderConnection.Status);

                    switch (incomingMsg.SenderConnection.Status)
                    {
                        case NetConnectionStatus.Connected:
                            break;
                        case NetConnectionStatus.Connecting:
                            break;
                        case NetConnectionStatus.Disconnected:
                            break;
                        case NetConnectionStatus.Disconnecting:
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
	}

}
