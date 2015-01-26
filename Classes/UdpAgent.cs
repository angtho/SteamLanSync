using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SteamLanSync.Messages;
using System.Threading;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace SteamLanSync
{
    /**
     * UdpAgent runs in a separate thread and performs the following tasks:
     * - sends periodic HELLO messages
     * - receives HELLO messages and updates the Application's peer list
     */
    
    
    public class UdpAgent
    {
        public int Port;
        public event MessageReceivedEventHandler OnMessageReceived;

        private UdpClient udpClient;
        private IAsyncResult _ar;
        private Dictionary<string, string> _rxMsgBuf;
        private System.Collections.Hashtable _localAddresses = new System.Collections.Hashtable();
        private bool _stopping = false;
        private Thread _sendThread;

        public UdpAgent(int port)
        {
            Port = port;
            udpClient = new UdpClient(Port);
            _rxMsgBuf = new Dictionary<string,string>();
            GetLocalIpAddresses();
        }

        public void Start()
        {
            
            _sendThread = new Thread(new ThreadStart(SendHello));
            _sendThread.Start();

            Listen();
        }

        public void Stop()
        {
            _stopping = true; // this will tell the sending thread to stop as well
            _sendThread.Interrupt(); // if it was sleeping, wake it up
            if (udpClient != null)
            {
                udpClient.Close();
            }
        }

        public void HurrySendHello()
        {
            _sendThread.Interrupt();
        }

        private void Listen() {
            if (_stopping) { return;  }
            _ar = udpClient.BeginReceive(Receive, new object());
        }

        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Port);
            byte[] bytes;

            try
            {
                bytes = udpClient.EndReceive(ar, ref ip); // todo handle exceptions
            }
            catch (ObjectDisposedException)
            {
                // this happens when we call udpClient.Close()
                // can't see an easy way to prevent this exception -- we can't avoid calling BeginReceive()
                // because we need to wait for data to come in (i.e. be listening), but we might want to 
                // abort while waiting for that data.
                return;
            }
            
            string message = Encoding.ASCII.GetString(bytes);
            //Debug.WriteLine("From {0} received: {1}", ip.Address.ToString(), message);
            
            // UDP broadcasts are received on local adapter as well, so filter these out
            if (_localAddresses.ContainsKey(ip.Address))
            {
                //Debug.WriteLine("Ignored packet from self");
                Listen();
                return;
            }
            
            string outStr = "";
            Messages.Message parsedMessage;

            // get current receive buffer for this IP address
            if (_rxMsgBuf.TryGetValue(ip.Address.ToString(), out outStr))
            {
                outStr += message;
            }
            else
            {
                outStr = message;
            }

            // check if we get a valid message after adding the received bytes
            parsedMessage = Messages.MessageParser.Parse(ref outStr);
            _rxMsgBuf[ip.Address.ToString()] = outStr;

            if (parsedMessage != null)
            { // if so, handle it
                //Debug.WriteLine("Received a valid message!");
                // notify listeners that we received a message
                MessageReceivedEventHandler handler = OnMessageReceived;
                if (handler != null)
                    handler(this, new MessageReceivedEventArgs(parsedMessage, ip.Address)); 
            }


            Listen();
        }

        private void SendHello()
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("255.255.255.255"), Port);
            HelloMessage msg = new HelloMessage();
            Random rand = new Random();

            msg.hostname = System.Environment.MachineName;
            msg.listenPort = Port;

            byte[] bytes = Encoding.ASCII.GetBytes(msg.Serialize());

            while (true)
            {
                client.Send(bytes, bytes.Length, ip);
                try
                {
                    // wait between 3.5 and 6.5 seconds before sending next hello. This prevents all hello
                    // messages on the network from becoming synchronized.
                    Thread.Sleep(rand.Next(3500, 6500));
                }
                catch (ThreadInterruptedException)
                {
                    // keep going -- this was just a prompt to either send next message immediately, or hurry up and exit if we're stopping
                }

                if (_stopping) // send goodbye message so other peers can proactively remove us from their peer list
                {
                    try
                    {
                        byte[] goodbyeBytes = Encoding.ASCII.GetBytes(new GoodbyeMessage().Serialize());
                        client.Send(goodbyeBytes, goodbyeBytes.Length, ip);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Caught exception while sending goodbye message");
                        Debug.WriteLine(ex);
                    }
                    break;
                }
            }
            Debug.WriteLine("SendHello() is exiting");
            client.Close();
            
        }

        private void GetLocalIpAddresses()
        {
            //http://stackoverflow.com/a/5272099
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                Debug.WriteLine("Name: " + netInterface.Name);
                Debug.WriteLine("Description: " + netInterface.Description);
                Debug.WriteLine("Addresses: ");
                IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    _localAddresses.Add(addr.Address, true);

                    Debug.WriteLine(" " + addr.Address.ToString());
                }
                Debug.WriteLine("");
            }
        }

    }
}
