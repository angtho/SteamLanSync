using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SteamLanSync.Messages;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace SteamLanSync
{
    

    public struct TcpPeerConnection
    {
        public byte[] buffer;
        public string messageBuffer;
        public TcpClient client;
        public NetworkStream stream;
        public IPAddress remoteAddress;

        public TcpPeerConnection(TcpClient _client, NetworkStream _stream, IPAddress _remoteAddress)
        {
            buffer = new byte[4096];
            messageBuffer = "";
            client = _client;
            stream = _stream;
            remoteAddress = _remoteAddress;
        }
        
    }

    public class TcpAgent
    {
        public int Port;
        public event MessageReceivedEventHandler OnMessageReceived;

        private TcpListener _listener;
        private IAsyncResult _listenerAsyncResult;
        private Dictionary<IPAddress, TcpPeerConnection> _peerConns = new Dictionary<IPAddress, TcpPeerConnection>();

        private Dictionary<IPAddress, Queue<Message>> _sendQueue = new Dictionary<IPAddress, Queue<Message>>();

        public void Start(int port)
        {
            Port = port;
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            // asynchronously listen for incoming connections. When a connection is made, AcceptClient() will be called
            _listenerAsyncResult = _listener.BeginAcceptTcpClient(AcceptClient, new object());
        }

        public void Stop()
        {
            foreach (TcpPeerConnection conn in _peerConns.Values)
            {
                conn.stream.Close();
                conn.client.Close();
            }
            _peerConns.Clear();

            _listener.Stop();
        }

        private void AcceptClient(IAsyncResult ar)
        {
            TcpClient newClient;
            try
            {
                // a new connection has been accepted
                newClient = _listener.EndAcceptTcpClient(ar);
                Debug.WriteLine("Listener established TCP connection with " + ((IPEndPoint)newClient.Client.RemoteEndPoint).Address.ToString());
            }
            catch (ObjectDisposedException)
            {
                // occurs if we close the TCP listener while waiting for data
                return;
            }
            catch (IOException ex)
            {
                Debug.WriteLine("Caught [" + ex.GetType().ToString() + "]\n\n" + ex.StackTrace);
                return;
            }
            catch (SocketException ex)
            {
                Debug.WriteLine("Caught [" + ex.GetType().ToString() + "]\n\n" + ex.StackTrace);
                return;
            }

            // keep listening for more connections
            _listenerAsyncResult = _listener.BeginAcceptTcpClient(AcceptClient, new object());
           

            NetworkStream ns = newClient.GetStream();
            IPAddress addr = ((IPEndPoint)newClient.Client.RemoteEndPoint).Address;
            TcpPeerConnection conn = new TcpPeerConnection(newClient, ns, addr);
            
            // add connection to the peer list
            if (_peerConns.ContainsKey(addr)) {
                try
                {
                    _peerConns[addr].stream.Close();
                    _peerConns[addr].client.Close();
                }
                catch (Exception e)
                {
                    // eat exception - exceptions likely when closing a dead connection that's been reopened
                    Debug.WriteLine("Exception while closing connection assumed to be dead");
                    Debug.WriteLine(e);
                }
                _peerConns[addr] = conn;
            }
            else 
            {
                _peerConns.Add(addr, conn);
            }

            // asynchronously read data from the connection. When data arrives, ReceiveData() will be called
            // todo this can throw an exception
            ns.BeginRead(conn.buffer, 0, conn.buffer.Length, ReceiveData, conn);
        }

        private void ReceiveData(IAsyncResult ar)
        {
            TcpPeerConnection conn = (TcpPeerConnection)ar.AsyncState; // ar.AsyncState is the TcpPeerConnection we passed in to BeginRead()
            int i;
            
            try
            {
                i = conn.stream.EndRead(ar);
            }
            catch (ObjectDisposedException)
            {
                // occurs if we close the TCP connection while waiting for data
                // can't see an easy way to prevent this exception -- we can't avoid calling BeginReceive()
                // because we need to wait for data to come in (i.e. be listening), but we might want to 
                // abort while waiting for that data.
                return;
            }
            catch (IOException ex)
            {
                Debug.WriteLine("Caught [" + ex.GetType().ToString() + "]\n\n" + ex.StackTrace);
                return;
            }
            catch (SocketException ex)
            {
                Debug.WriteLine("Caught [" + ex.GetType().ToString() + "]\n\n" + ex.StackTrace);
                return;
            }

            if (i == 0)
            {
                // connection was closed
                IPAddress addr = ((IPEndPoint)conn.client.Client.RemoteEndPoint).Address;
                Debug.WriteLine("TCP Connection with " + addr.ToString() + " was closed");
                conn.stream.Close(); // acknowledge remote endpoint closing the connection
                conn.client.Close();
                _peerConns.Remove(addr);
                return;
            }

            // decode receive bytes and append to rx string buffer
            conn.messageBuffer += System.Text.Encoding.ASCII.GetString(conn.buffer, 0, i);

            Debug.WriteLine("TcpAgent buffer = " + conn.messageBuffer);

            // see if the resulting string is a valid message
            try
            {
                Message msg = MessageParser.Parse(ref (conn.messageBuffer));
                if (msg != null)
                {
                    MessageReceivedEventHandler handler = OnMessageReceived;
                    if (handler != null)
                        handler(this, new MessageReceivedEventArgs(msg, ((IPEndPoint)conn.client.Client.RemoteEndPoint).Address));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Caught exception while parsing message");
                Debug.WriteLine(ex);
                throw;
            }
           
            

            // read more data
            conn.stream.BeginRead(conn.buffer, 0, conn.buffer.Length, ReceiveData, conn);
        }

        public void SendMessage(Message msg, IPAddress addr) 
        {
            // see if we have an existing connection to this peer
            TcpPeerConnection conn;
            if (_peerConns.ContainsKey(addr))
            {
                // convert message to bytes and send it
                conn = _peerConns[addr];
                try
                {
                    byte[] msgBytes = Encoding.ASCII.GetBytes(msg.Serialize());
                    conn.stream.BeginWrite(msgBytes, 0, msgBytes.Length, DataSent, conn);
                    return;
                }
                catch (SocketException ex)
                {
                    Debug.Write("Caught SocketException in SendMessage");
                    Debug.Write(ex);
                    try
                    {
                        conn.stream.Close();
                        conn.client.Close();
                    }
                    catch (Exception)
                    {
                        // have to eat it
                    }

                    // fall through queue the message instead 
                }
                catch (IOException ex)
                {
                    Debug.Write("Caught IOException in SendMessage");
                    Debug.Write(ex);
                    try
                    {
                        conn.stream.Close();
                        conn.client.Close();
                    }
                    catch (Exception)
                    {
                        // have to eat it
                    }

                    // fall through queue the message instead 
                }
                catch (ObjectDisposedException ex)
                {
                    // occurs if we close the TCP connection while trying to send data
                    Debug.Write("Caught ObjectDisposedException in SendMessage");
                    Debug.Write(ex);
                    return;
                }
            }

            // either we didn't have an active connection, or we tried to send on our active connection and failed,
            // so add the message to the queue
            if (!_sendQueue.ContainsKey(addr)) { _sendQueue.Add(addr, new Queue<Message>());  }
            _sendQueue[addr].Enqueue(msg);

            // connect to the peer
            conn = new TcpPeerConnection(null, null, addr);
            TcpClient client = new TcpClient();
            conn.client = client;
            client.BeginConnect(addr, Port, ConnectedToPeer, conn);
                
            // ConnectedToPeer() will send the message once we're connected
            
            
        }

        private void DataSent(IAsyncResult ar)
        {
            TcpPeerConnection conn = (TcpPeerConnection)ar.AsyncState;
            try
            {
                conn.stream.EndWrite(ar);
            }
            catch (SocketException ex)
            {
                Debug.Write("Caught SocketException in DataSent");
                Debug.Write(ex);
                try
                {
                    conn.stream.Close();
                    conn.client.Close();
                }
                catch (Exception)
                {
                    // have to eat it
                }

            }
            catch (IOException ex)
            {
                Debug.Write("Caught IOException in DataSent");
                Debug.Write(ex);
                try
                {
                    conn.stream.Close();
                    conn.client.Close();
                }
                catch (Exception)
                {
                    // have to eat it
                }
            }
            catch (ObjectDisposedException ex)
            {
                // occurs if we close the TCP connection while trying to send data
                Debug.Write("Caught ObjectDisposedException in DataSent");
                Debug.Write(ex);
                return;
            }
        }

        private void ConnectedToPeer(IAsyncResult ar)
        {
            TcpPeerConnection conn = (TcpPeerConnection)ar.AsyncState;
            IPAddress addr = conn.remoteAddress;

            // check if connection was successful
            if (!conn.client.Connected)
            {
                Debug.WriteLine("Failed to connect to " + addr.ToString());
                return;
            }

            conn.stream = conn.client.GetStream();

            // see if we already have a connection to this peer -- if so, close it
            if (_peerConns.ContainsKey(addr))
            {
                TcpPeerConnection oldConn = _peerConns[addr];
                try
                {
                    oldConn.stream.Close();
                    oldConn.client.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Caught " + ex.ToString() + " while closing old connection");
                }
                finally
                {
                    _peerConns.Remove(addr);
                }
            }

            // store the peer in our peers list
            _peerConns.Add(addr, conn);

            
            Debug.WriteLine("Established connection to " + addr.ToString());

            // see if the peer has any data to send us
            conn.stream.BeginRead(conn.buffer, 0, conn.buffer.Length, ReceiveData, conn);

            // send any queued messages to the peer
            Queue<Message> q = _sendQueue[addr];
            Message sendThis = q.Dequeue();
            if (sendThis != null)
            {
                SendMessage(sendThis, addr);
            }
        }
    }
}
