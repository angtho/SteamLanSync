using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace SteamLanSync
{
    public class TransferCannotStartException : Exception
    {
        public TransferCannotStartException(string message) : base(message) { }
    }

    public enum TransferState
    {
        NotStarted,
        BuildingManifest,
        WaitingForManifest,
        InProgress,
        Complete,
        Failed
    }

    public class TransferStateChangedEventArgs : EventArgs
    {
        public TransferState OldState;
        public TransferState NewState;
        public string Reason;
    }

    public class DataTransferredEventArgs : EventArgs
    {
        public long BytesTransferred;
        public long Milliseconds;

        public DataTransferredEventArgs(long bytes, long millis)
        {
            BytesTransferred = bytes;
            Milliseconds = millis;
        }
    }

    public delegate void TransferStateChangedEventHandler(object sender, TransferStateChangedEventArgs e);
    public delegate void DataTransferredEventHandler(object sender, DataTransferredEventArgs e);

    public class TransferInfo
    {
        private TransferState _state;
        public string StateChangeReason = "";

        public string TransferId;
        public AppInfo App;
        public SyncPeer Peer;
        public AppManifest Manifest;
        public DirectoryInfo ManifestRoot;
        public bool IsSending;
        public int Port;
        public Thread Thread;
        public TransferAgent Agent;
        public long TotalBytes = 0;
        public long BytesTransferred = 0;

        private long _bytesTransferredAtLastUpdate = 0;
        private long _ticksAtLastUpdate = 0;

        private Queue<long> _speedHistory = new Queue<long>(10);

        public TransferState State
        {
            get { return _state; }
            set
            {
                TransferStateChangedEventArgs args = new TransferStateChangedEventArgs();
                args.OldState = _state;
                args.NewState = value;
                args.Reason = StateChangeReason;
                StateChangeReason = "";
                _state = value;

                if (args.OldState != args.NewState)
                {
                    TransferStateChangedEventHandler handler = OnTransferStateChanged;
                    if (handler != null)
                        handler(this, args);
                }
            }
        }

        public event TransferStateChangedEventHandler OnTransferStateChanged;
        public event DataTransferredEventHandler OnDataTransferred;

        public TransferInfo(string transferId)
        {
            TransferId = transferId;
            State = TransferState.NotStarted;
        }

        public TransferInfo()
        {
            TransferId = generateTransferId();
            State = TransferState.NotStarted;
        }

        public void FireDataTransferredEvent()
        {
            long bytesTransferred = BytesTransferred - _bytesTransferredAtLastUpdate;
            long ticks = DateTime.Now.Ticks - _ticksAtLastUpdate;

            DataTransferredEventHandler handler = OnDataTransferred;
            if (handler != null)
                handler(this, new DataTransferredEventArgs(bytesTransferred, ticks / 10000));

            _bytesTransferredAtLastUpdate = BytesTransferred;
            _ticksAtLastUpdate = DateTime.Now.Ticks;
            long millis = ticks/10000;
            if (millis > 0)
                _speedHistory.Enqueue(1000 * bytesTransferred / millis);
        }

        /// <summary>
        /// The average of the last 10 recorded transfer speeds. Transfer speed is only measured
        /// at intervals while data is being transferred, so as soon as the transfer stops,
        /// this property will give incorrect results.
        /// </summary>
        public long TransferSpeed
        {
            get
            {
                if (State != TransferState.InProgress)
                    return 0;

                if (_speedHistory.Count == 0)
                    return 0;

                return _speedHistory.Sum() / _speedHistory.Count;
            }
        }

        private string generateTransferId()
        {
            int i;
            char[] result = new char[10];
            char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            Random r = new Random();
            for (i = 0; i < 10; i++)
            {
                result[i] = chars[r.Next(chars.Length - 1)];
            }

            return new string(result);
        }
    }
}
