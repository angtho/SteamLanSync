using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SteamLanSync
{
    public class SyncPeer
    {
        public IPAddress Address;
        public int Port;
        public string Hostname;
        public DateTime LastResponded;
        public List<AppInfo> Apps;
    }
}
