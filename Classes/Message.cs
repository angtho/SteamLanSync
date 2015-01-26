using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace SteamLanSync.Messages
{
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

    public class MessageReceivedEventArgs : EventArgs
    {
        public Message ReceivedMessage;
        public IPAddress SenderAddress;

        public MessageReceivedEventArgs(Message msg, IPAddress senderAddr) {
            ReceivedMessage = msg;
            SenderAddress = senderAddr;
        }
    }

    public abstract class Message
    {
        public const string MESSAGE_START_DELIMITER = "\n\n";

        protected readonly string _name;
        protected Message(string name)
        {
            _name = name;
        }

        public string Serialize()
        {
            return MESSAGE_START_DELIMITER + _name + "\n" + JsonConvert.SerializeObject(this);
        }
    }

    public class HelloMessage : Message
    {
        public int listenPort;
        public string hostname;

        public HelloMessage() : base("HELLO") { }
    }

    public class RequestAppListMessage : Message
    {
        public RequestAppListMessage() : base("REQUEST_APP_LIST") { }

        // empty
    }

    public class AppListMessage : Message
    {
        public List<AppInfo> availableApps = new List<AppInfo>();

        public AppListMessage() : base("APP_LIST") 
        {
            
        }

    }

    public class RequestAppTransferMessage : Message
    {
        public string appId;
        public string transferId;
        public int listenPort;

        public RequestAppTransferMessage() : base("REQUEST_APP_TRANSFER") { }
    }

    public class StartAppTransferMessage : Message
    {
        public AppManifest manifest;
        public string transferId;

        public StartAppTransferMessage() : base("START_APP_TRANSFER") { }
    }

    public class CancelAppTransferMessage : Message
    {
        public string transferId;
        public string reason;

        public CancelAppTransferMessage() : base("CANCEL_APP_TRANSFER") { }
    }

    public class GoodbyeMessage : Message
    {
        public GoodbyeMessage() : base("GOODBYE") { }
    }
}
