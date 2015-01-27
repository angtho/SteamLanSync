using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamLanSync.Messages;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace SteamLanSync
{
    public delegate void SyncPeerUpdatedEventHandler(object sender, SyncPeerUpdatedEventArgs e);
    public delegate void SyncManagerUpdatedStatusEventHandler(object sender, SyncManagerUpdatedStatusEventArgs e);
    public delegate void TransferCreatedEventHandler(object sender, TransferEventArgs e);
    public delegate void TransferRemovedEventHandler(object sender, TransferEventArgs e);
    public delegate void AvailableAppsUpdatedEventHandler(object sender, EventArgs e);

    public class AppNotAvailableException : Exception { }

    public enum SyncPeerUpdatedAction 
    {
        Added,
        Updated,
        Removed
    }

    public class SyncPeerUpdatedEventArgs : EventArgs 
    {
        public SyncPeer Peer;
        public SyncPeerUpdatedAction Action;

        public SyncPeerUpdatedEventArgs(SyncPeer peer, SyncPeerUpdatedAction action)
        {
            Peer = peer;
            Action = action;
        }
    }

    public class SyncManagerUpdatedStatusEventArgs : EventArgs
    {
        public string Status;

        public SyncManagerUpdatedStatusEventArgs(string status)
        {
            Status = status;
        }
    }

    public class TransferEventArgs : EventArgs
    {
        public TransferInfo Transfer;

        public TransferEventArgs(TransferInfo transfer)
        {
            Transfer = transfer;
        }
    }

    public class SyncManager
    {
        private UdpAgent _udpAgent;
        private TcpAgent _tcpAgent;
        private List<SyncPeer> _peers = new List<SyncPeer>();
        private System.Timers.Timer _deadPeerTimer = null;


        public AppLibrary Library { get; private set; }
        public List<AppInfo> AvailableApps = new List<AppInfo>();
        public Dictionary<string, TransferInfo> Transfers = new Dictionary<string, TransferInfo>();
        public Queue<TransferInfo> TransferQueue = new Queue<TransferInfo>();
        public int BroadcastPort { get; private set; }
        public int ListenPort{ get; private set; }

        public event SyncPeerUpdatedEventHandler OnSyncPeerUpdated;
        public event TransferCreatedEventHandler OnTransferCreated;
        public event TransferRemovedEventHandler OnTransferRemoved;
        public event SyncManagerUpdatedStatusEventHandler OnStatusUpdated;
        public event AvailableAppsUpdatedEventHandler OnAvailableAppsUpdated;

        public SyncManager(int broadcastPort, int listenPort, string libraryPath)
        {
            BroadcastPort = broadcastPort;
            ListenPort = listenPort;
            Library = AppLibrary.FromDirectory(libraryPath);
            if (Library == null)
            {
                Debug.WriteLine(libraryPath + " is not a valid Steam Library");
                return;
            }
        }

        public void Start()
        {
            if (Library == null)
            {
                throw new InvalidOperationException("Cannot start SyncManager without a valid library");
            }

            if (BroadcastPort == 0)
            {
                throw new InvalidOperationException("Cannot start SyncManager without a valid BroadcastPort");
            }

            if (ListenPort == 0)
            {
                throw new InvalidOperationException("Cannot start SyncManager without a valid ListenPort");
            }

            if (!AppLibrary.CanWriteToDirectory(Library.Path))
            {
                throw new ArgumentException("Cannot write to Library path " + Library.Path);
            }
            
            _udpAgent = new UdpAgent(BroadcastPort);
            _udpAgent.OnMessageReceived += new MessageReceivedEventHandler(handleMessage);
            _udpAgent.Start();

            _tcpAgent = new TcpAgent();
            _tcpAgent.OnMessageReceived += new MessageReceivedEventHandler(handleMessage);
            _tcpAgent.Start(BroadcastPort);

            // every 10 seconds, check for dead peers and remove from our list
            _deadPeerTimer = new System.Timers.Timer(10000);
            _deadPeerTimer.Elapsed += removeDeadPeers;
            _deadPeerTimer.AutoReset = true;
            _deadPeerTimer.Start();

            updateStatus(String.Format("Sharing {0} apps from library {1}", Library.Apps.Count, Library.Path));
        }

        public void Stop()
        {
            if (_udpAgent != null)
            {
                _udpAgent.Stop();
            }
            if (_tcpAgent != null)
            {
                _tcpAgent.Stop();
            }
            if (_deadPeerTimer != null)
            {
                _deadPeerTimer.Stop();
            }

            // cancel pending transfers
            lock (Transfers)
            {
                foreach (TransferInfo t in Transfers.Values)
                {
                    if (t.Agent != null)
                        t.Agent.CancelTransfer();
                }
            }
            
        }

        private void removeDeadPeers(object sender, System.Timers.ElapsedEventArgs e)
        {
            // find peers who have not sent us a "Hello" message within 15 seconds
            List<SyncPeer> deadPeers = _peers.Where
            (
                (peer) => 
                {
                    return DateTime.Now.Subtract(peer.LastResponded).TotalMilliseconds > 15000;
                }
            ).ToList();

            
            foreach (SyncPeer peer in deadPeers)
            {
                // Although this will fire updateAvailableApps() once per peer, the likelihood of
                // multiple peers being removed in one pass is pretty small, so the extra complexity
                // of ensuring only one firing of updateAvailableApps() is not warranted (yet).
                removePeer(peer);
            }

            // tcpAgent will clean up its own connections (they'll time out or be closed)
        }

        private void removePeer(SyncPeer peer)
        {
            // remove the peers
            
            Debug.WriteLine("Removing " + peer.Hostname + " from peers list");
            if (_peers.Contains(peer))
                _peers.Remove(peer);

            // update list of available apps (i.e. remove any that are no longer available)
            updateAvailableApps();

            // notify listeners that the peers have been removed (need to do this after available apps has been updated)
            SyncPeerUpdatedEventHandler handler = OnSyncPeerUpdated;
            if (handler != null)
                handler(this, new SyncPeerUpdatedEventArgs(peer, SyncPeerUpdatedAction.Removed));
            
        }

        private void startQueuedTransfers()
        {
            if (this.TransferQueue.Count == 0)
                return;

            if (this.getReceiveTransfersInProgress() > 0)
                return;

            lock (this.Transfers) 
            {
                TransferInfo queued = TransferQueue.Dequeue();
                startReceiveApp(queued);
            }
        }

        private void updateStatus(string status) 
        {
            Debug.WriteLine("SyncManager updated status: " + status);
            SyncManagerUpdatedStatusEventHandler handler = OnStatusUpdated;
            if (handler != null)
                handler(this, new SyncManagerUpdatedStatusEventArgs(status));
        }

        private void handleMessage(object sender, MessageReceivedEventArgs e)
        {
            Debug.WriteLine("Received " + e.ReceivedMessage.GetType().ToString() + " from " + e.SenderAddress.ToString());

            if (e.ReceivedMessage is HelloMessage)
            {
                handleHelloMessage((HelloMessage)e.ReceivedMessage, e.SenderAddress);
            }
            else if (e.ReceivedMessage is RequestAppListMessage)
            {
                handleRequestAppListMessage((RequestAppListMessage)e.ReceivedMessage, e.SenderAddress);
            }
            else if (e.ReceivedMessage is AppListMessage)
            {
                handleAppListMessage((AppListMessage)e.ReceivedMessage, e.SenderAddress);
            }
            else if (e.ReceivedMessage is RequestAppTransferMessage)
            {
                handleRequestAppTransferMessage((RequestAppTransferMessage)e.ReceivedMessage, e.SenderAddress);
            }
            else if (e.ReceivedMessage is StartAppTransferMessage)
            {
                handleStartAppTransferMessage((StartAppTransferMessage)e.ReceivedMessage, e.SenderAddress);
            }
            else if (e.ReceivedMessage is GoodbyeMessage)
            {
                handleGoodbyeMessage((GoodbyeMessage)e.ReceivedMessage, e.SenderAddress);
            }
        }

        private void handleHelloMessage(HelloMessage msg, IPAddress sender)
        {
            // see if the peers list contains a SyncPeer with the same hostname
            SyncPeer found = _peers.Find((peer) => { return (peer.Hostname == msg.hostname); });

            if (found != null)
            { // we know about this peer, update its last seen time
                found.LastResponded = DateTime.Now;
            }
            else
            { // we don't know about this peer, add it
                SyncPeer newPeer = new SyncPeer();
                newPeer.Port = msg.listenPort;
                newPeer.Hostname = msg.hostname;
                newPeer.Address = sender;
                newPeer.LastResponded = DateTime.Now;
                _peers.Add(newPeer);
                Debug.WriteLine("Added new peer " + msg.hostname);
                
                SyncPeerUpdatedEventHandler handler = OnSyncPeerUpdated;
                if (handler != null)
                    handler(this, new SyncPeerUpdatedEventArgs(found, SyncPeerUpdatedAction.Added));

                // send our hello message quickly (so the peer doesn't have to wait ~5 seconds to get our next hello)
                _udpAgent.HurrySendHello();

                // request the peer's applist
                Debug.WriteLine("Sending RequestAppList to " + sender.ToString());
                RequestAppListMessage sendMsg = new RequestAppListMessage();
                _tcpAgent.SendMessage(sendMsg, sender);
            }
        }

        private void handleRequestAppListMessage(RequestAppListMessage msg, IPAddress sender)
        {
            Debug.WriteLine("Sending AppList to " + sender.ToString());
            AppListMessage sendMsg = new AppListMessage();
            sendMsg.availableApps = Library.Apps.Values.ToList();
            _tcpAgent.SendMessage(sendMsg, sender);
        }

        private void handleAppListMessage(AppListMessage msg, IPAddress sender)
        {
            SyncPeer found = _peers.Find((peer) => { return peer.Address.Equals(sender); });

            if (found == null)
            {
                Debug.WriteLine("Received AppList from unknown peer " + sender.ToString());
            }

            found.Apps = msg.availableApps;
            updateAvailableApps();
            Debug.WriteLine(String.Format("{0} has {1} available apps", found.Hostname, found.Apps.Count));
            SyncPeerUpdatedEventHandler handler = OnSyncPeerUpdated;
            if (handler != null)
                handler(this, new SyncPeerUpdatedEventArgs(found, SyncPeerUpdatedAction.Updated));
        }

        private void handleRequestAppTransferMessage(RequestAppTransferMessage msg, IPAddress sender) {
            // see if we have the app
            AppInfo theApp = Library.Apps[msg.appId];
            if (theApp == null)
            {
                Debug.WriteLine(String.Format("{1} requested AppId {0} but I don't have it", msg.appId, sender.ToString()));
                return;
            }
            
            // find the peer
            SyncPeer peer = _peers.Find((aPeer) => { return aPeer.Address.Equals(sender); });
            Debug.WriteLine(String.Format("I will transfer {0} to {1}", msg.appId, peer.Hostname));

            // build manifest
            AppManifest manifest = AppManifest.FromAppInfo(theApp, Library); // todo - async/threaded

            // verify manifest is valid
            if (manifest == null)
            {
                Debug.WriteLine("Could not build manifest for AppId [" + msg.appId + "], aborting.");
                CancelAppTransferMessage cancelMsg = new CancelAppTransferMessage();
                cancelMsg.transferId = msg.transferId;
                cancelMsg.reason = "Unable to build manifest";
                _tcpAgent.SendMessage(cancelMsg, sender);
                return;
            }

            // send "start transfer" message with manifest
            StartAppTransferMessage newMsg = new StartAppTransferMessage();
            newMsg.manifest = manifest;
            newMsg.transferId = msg.transferId;

            _tcpAgent.SendMessage(newMsg, sender);

            // create transfer
            TransferInfo transfer = new TransferInfo(newMsg.transferId);
            transfer.Manifest = newMsg.manifest;
            transfer.App = theApp;
            transfer.IsSending = true;
            transfer.Peer = peer;
            transfer.Port = msg.listenPort;
            transfer.ManifestRoot = new DirectoryInfo(Library.Path);

            // subscribe to state change notifications (so we can update our status message)
            subscribeTransferEvents(transfer);

            // create agent to send the files
            TransferAgent agent = new TransferAgent();
            agent.StartSend(transfer);

            // fire event to notify listeners a transfer was created
            TransferCreatedEventHandler handler = OnTransferCreated;
            if (handler != null)
                handler(this, new TransferEventArgs(transfer));
        }

        private void handleStartAppTransferMessage(StartAppTransferMessage msg, IPAddress sender)
        {
            TransferInfo theTransfer = null;

            lock (Transfers)
            {
                if (!Transfers.ContainsKey(msg.transferId))
                {
                    Debug.WriteLine("Received StartAppTransferMessage for unknown transfer [" + msg.transferId + "]");
                }
                theTransfer = Transfers[msg.transferId];
            }
            if (theTransfer == null)
                return;
            
            lock (theTransfer) // this will also be accessed by the TransferAgent's receive thread
            {
                theTransfer.Manifest = msg.manifest;
            }
            
        }

        private void handleCancelAppTransferMessage(CancelAppTransferMessage msg, IPAddress sender)
        {
            TransferInfo theTransfer = null;

            lock (Transfers)
            {
                if (!Transfers.ContainsKey(msg.transferId))
                {
                    Debug.WriteLine("Received CancelAppTransferMessage for unknown transfer [" + msg.transferId + "]");
                }
                theTransfer = Transfers[msg.transferId];
            }

            if (theTransfer != null && theTransfer.Agent != null)
            {
                theTransfer.Agent.CancelTransfer();
            }   
        }

        private void handleGoodbyeMessage(GoodbyeMessage msg, IPAddress sender)
        {
            // see if we know about this peer
            SyncPeer _found = _peers.Find((peer) => { return peer.Address.Equals(sender); });
            
            // if so, remove from our peer list
            if (_found != null)
            {
                removePeer(_found);
            }
        }

        private void handleTransferStateChanged(object sender, TransferStateChangedEventArgs e)
        {
            TransferInfo t = (TransferInfo)sender;

            if (t.IsSending && e.NewState == TransferState.InProgress)
            {
                // we just started sending an app to a peer
                updateStatus(String.Format("Sending {0} to {1}", t.App.Name, t.Peer.Hostname));
            }
            else if (!(t.IsSending) && e.NewState == TransferState.Complete)
            {
                // we just finished receiving an app from a peer
                updateStatus(String.Format("Received {0} from {1}", t.App.Name, t.Peer.Hostname));
                
                if (!Library.Apps.ContainsKey(t.App.AppId))
                    Library.Apps.Add(t.App.AppId, t.App);
                
                updateAvailableApps();
            }

            // see if we have anything on the queue that we should start
            startQueuedTransfers();
        }

        private void updateAvailableApps()
        {
            // merge all peers' available apps, deduplicate, then subtract ones we've already got in our library
            HashSet<AppInfo> availableApps = new HashSet<AppInfo>();
            
            foreach (SyncPeer peer in _peers)
            {
                if (peer.Apps != null)
                    availableApps.UnionWith(peer.Apps);
            }
            availableApps.RemoveWhere((app) => { return Library.Apps.ContainsKey(app.AppId); });

            AvailableApps = availableApps.ToList<AppInfo>();

            // notify listeners that the list has changed
            AvailableAppsUpdatedEventHandler handler = OnAvailableAppsUpdated;
            if (handler != null)
                handler(this, new EventArgs());
        }

        private int getReceiveTransfersInProgress()
        {
            int retVal = 0;
            lock (Transfers)
            {
                retVal = (Transfers.Where((tfer) => { return tfer.Value.IsSending == false && tfer.Value.State != TransferState.Failed && tfer.Value.State != TransferState.Complete; })).ToList().Count;
            }
            return retVal;
        }

        public void RequestApp(AppInfo app)
        {
            // determine who has a copy of the app
            SyncPeer whoHasIt = null;
            foreach (SyncPeer peer in _peers)
            {
                if (peer.Apps.Contains(app))
                {
                    whoHasIt = peer;
                    break;
                }
            }

            if (whoHasIt == null)
            {
                throw new AppNotAvailableException();
            }
            
            // create a transfer
            TransferInfo transfer = new TransferInfo(); // generates a transferid
            transfer.IsSending = false;
            transfer.Peer = whoHasIt;
            transfer.App = app;
            transfer.Port = Properties.Settings.Default.ListenPort;
            transfer.ManifestRoot = new DirectoryInfo(Library.Path);
            
            
            if (getReceiveTransfersInProgress() == 0)
            { // start immediately if we're not receiving anything else
                Debug.WriteLine("Starting transfer agent to receive " + transfer.App.Name);
                startReceiveApp(transfer);
            }
            else
            { // otherwise add to the queue
                Debug.WriteLine("Queuing transfer of " + transfer.App.Name);
                TransferQueue.Enqueue(transfer);
            }

        }

        private void startReceiveApp(TransferInfo transfer)
        {
            lock (Transfers)
            {
                Transfers.Add(transfer.TransferId, transfer);
            }
            subscribeTransferEvents(transfer);
            
            // create agent to handle the transfer
            TransferAgent agent = new TransferAgent();
            agent.StartReceive(transfer);

            // fire event to notify listeners a transfer was created
            TransferCreatedEventHandler handler = OnTransferCreated;
            if (handler != null)
                handler(this, new TransferEventArgs(transfer));

            // send the peer a request
            RequestAppTransferMessage newMsg = new RequestAppTransferMessage();
            newMsg.appId = transfer.App.AppId;
            newMsg.transferId = transfer.TransferId;
            newMsg.listenPort = transfer.Port;
            _tcpAgent.SendMessage(newMsg, transfer.Peer.Address);
        }

        private void subscribeTransferEvents(TransferInfo transfer)
        {
            transfer.OnTransferStateChanged += handleTransferStateChanged;
        }
    }
}
