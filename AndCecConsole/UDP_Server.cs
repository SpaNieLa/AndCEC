using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AndCecConsole
{

    /// <summary>
    /// UDP Server to receive input event wherever
    /// </summary>
    class UDP_Server
    {


        private static int UDP_port;
        private static UdpClient listener;
        List<IPEndPoint> clients;
        IPEndPoint groupEP;

        private List<string> events;

        public UDP_Server(int port)
        {
            UDP_port = port;
            listener = new UdpClient(UDP_port);
            // Probably one is enough
            clients = new List<IPEndPoint>();
            groupEP = new IPEndPoint(IPAddress.Any, UDP_port);
            // Event queue
            this.events = new List<string>();
            this.events.Add("Init");
        }

        // Returns current list of unhandled events
        public List<string> GetEvents()
        {
            return this.events;
        }

        // Remove seen events from queue
        public void Remove(int seen)
        {
            this.events.RemoveAt(seen);
        }

        // Start listening incoming packets
        public void RunServer()
        {
            try
            {
                while (true)
                {
                    byte[] bytes = listener.Receive(ref groupEP);
                    // Add new client to group
                    if (!clients.Contains(groupEP)) clients.Add(groupEP);

                    Console.WriteLine("{0} : {1}\n", groupEP.ToString(),
                    Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                    events.Add(Encoding.ASCII.GetString(bytes, 0, bytes.Length));

                    // open new socket t relay message
                    /*Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    // Send received message to others
                    foreach (IPEndPoint ep in clients)
                    {
                        if (!groupEP.Equals(ep)) s.SendTo(bytes, ep);
                    }*/
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
            }
        }
    }
}



