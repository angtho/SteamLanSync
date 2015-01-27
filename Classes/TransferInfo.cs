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

        private long _lastBytesTransferred = 0;
        private long _lastMillis = 0;
        
        private Stopwatch _speedStopwatch = null;
        private Queue<long> _speedHistory = new Queue<long>(10);

        public long ElapsedMilliseconds
        {
            get
            {
                if (_speedStopwatch == null)
                    return 0;
                return _speedStopwatch.ElapsedMilliseconds;
            }
        }

        public TransferState State
        {
            get { return _state; }
            set
            {
                if (value == _state)
                    return; 
                
                TransferStateChangedEventArgs args = new TransferStateChangedEventArgs();
                args.OldState = _state;
                args.NewState = value;
                args.Reason = StateChangeReason;
                StateChangeReason = "";
                _state = value;

                TransferStateChangedEventHandler handler = OnTransferStateChanged;
                if (handler != null)
                    handler(this, args);


                if (args.NewState == TransferState.InProgress)
                {
                    // start internal timer
                    if (_speedStopwatch == null)
                        _speedStopwatch = new Stopwatch();
                    
                    _speedStopwatch.Restart();
                }

                if (args.OldState == TransferState.InProgress)
                {
                    // stop internal timer
                    if (_speedStopwatch != null)
                    {
                        _speedStopwatch.Stop();
                    }
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
            long byteThisInterval = BytesTransferred - _lastBytesTransferred;
            long millis = _speedStopwatch.ElapsedMilliseconds - _lastMillis;

            DataTransferredEventHandler handler = OnDataTransferred;
            if (handler != null)
                handler(this, new DataTransferredEventArgs(byteThisInterval, millis));

            _lastBytesTransferred = BytesTransferred;
            _lastMillis = _speedStopwatch.ElapsedMilliseconds;
            if (millis > 0)
            {
                lock (_speedHistory)
                {
                    _speedHistory.Enqueue(1000 * byteThisInterval / millis);
                }
            }
        }

        

        /// <summary>
        /// The transfer speed in bytes per second. If the transfer state is TransferState.InProgress,
        /// this is the average transfer speed calculated over the last 10 DataTransferred events.
        /// Otherwise, it is calculated over the entire transfer time.
        /// </summary>
        public long TransferSpeed
        {
            get
            {
                if (State != TransferState.InProgress)
                {
                    if (_speedStopwatch != null && _speedStopwatch.ElapsedMilliseconds > 0)
                    {
                        return 1000 * BytesTransferred / _speedStopwatch.ElapsedMilliseconds;
                    }
                    return 0;
                }

                if (_speedHistory.Count == 0)
                    return 0;
                
                long retVal = 0;
                lock (_speedHistory) 
                {
                    retVal = _speedHistory.Sum() / _speedHistory.Count;
                }

                return retVal;
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
