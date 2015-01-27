using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace SteamLanSync
{
    

    public class TransferAgent
    {
        private TransferInfo _transfer;
        private Thread _thread;

        public TransferInfo Transfer
        {
            get { return _transfer;  }
        }

        private void checkTransfer(TransferInfo transfer)
        {
            if (!transfer.ManifestRoot.Exists)
            {
                throw new TransferCannotStartException("ManifestRoot directory does not exist");
            }

            if (transfer.Peer == null)
            {
                throw new TransferCannotStartException("Peer cannot be null");
            }

            if (transfer.Port == 0)
            {
                throw new TransferCannotStartException("Port must be defined");
            }

            if (transfer.App == null)
            {
                throw new TransferCannotStartException("App cannot be null");
            }
        }

        public void StartReceive(TransferInfo transfer)
        {
            checkTransfer(transfer);
            
            _transfer = transfer;
            _transfer.Agent = this;
            _transfer.State = TransferState.WaitingForManifest;
            _thread = new Thread(new ThreadStart(doReceive));
            transfer.Thread = _thread;
            _thread.Start();
            
        }

        private void doReceive()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _transfer.Port);
            TcpClient client = null;
            try
            {
                listener.Start();
                client = listener.AcceptTcpClient(); // blocks until the remote peer connects
                listener.Stop();
            }
            catch (SocketException ex)
            {
                Debug.Write(ex);
                _transfer.StateChangeReason = "Could not connect to peer";
                _transfer.State = TransferState.Failed;
                if (client != null && client.Connected)
                {
                    client.Close();
                }
                return;
            }
            
            // wait until the manifest has been received (on main thread, not this one)
            _transfer.State = TransferState.WaitingForManifest;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool abort = false;
            while (true)
            {
                lock (_transfer)  // this will also be accessed by the main thread
                {
                    if (_transfer.Manifest != null && _transfer.Manifest.files.Count > 0)
                    {
                        break;
                    }
                    Debug.WriteLine("Waiting for manifest...");
                    Thread.Sleep(250);

                    if (sw.ElapsedMilliseconds > 30000)
                    {
                        Debug.WriteLine("Timed out after 30 seconds waiting for manifest");
                        _transfer.StateChangeReason = "Timed out after 30 seconds waiting for manifest";
                        _transfer.State = TransferState.Failed;
                        abort = true;
                        break;
                    }
                }
            }

            if (abort)
            {
                client.Close();
                return;
            }

            _transfer.State = TransferState.InProgress;
            NetworkStream ns = client.GetStream();
            byte[] buf = new byte[64 * 1024];
            int fileIndex = 0;
            int bytesRead = 0;
            long totalBytesExpected = 0;
            long totalBytesWritten = 0;

            // get total bytes we expect to receive
            List<AppManifestFile> files = _transfer.Manifest.files;
            foreach (AppManifestFile mf in _transfer.Manifest.files)
            {
                totalBytesExpected += mf.size;
            }
            _transfer.TotalBytes = totalBytesExpected;
            Stopwatch stopwatch = new Stopwatch();

            for (fileIndex = 0; fileIndex < _transfer.Manifest.files.Count; fileIndex++)
            {
                // get info about next file in the manifest
                AppManifestFile currentFile = files[fileIndex];
                string rootDir = _transfer.ManifestRoot.FullName;
                string filePath;
                Utility.EnsureEndsWithSlash(ref rootDir);

                // create directory if needed
                filePath = rootDir + currentFile.path; // todo ensure this is inside Library path (..\ may escape it)
                FileInfo fi = new FileInfo(filePath);
                if (!fi.Directory.Exists)
                {
                    try
                    {
                        Directory.CreateDirectory(fi.DirectoryName);
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex);
                        _transfer.StateChangeReason = "Could not create directory " + fi.DirectoryName;
                        _transfer.State = TransferState.Failed;

                        ns.Close();
                        if (client.Connected)
                        {
                            client.Close();
                        }

                        return;
                    }
                    
                }

                // open the file for writing, replacing any existing file with same name
                FileStream fs;
                long bytesRemaining = currentFile.size;
                try
                {
                    fs = File.Create(filePath);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex);
                    _transfer.StateChangeReason = "Could not create file " + filePath;
                    _transfer.State = TransferState.Failed;

                    ns.Close();
                    if (client.Connected)
                    {
                        client.Close();
                    }

                    return;
                }
                
                Debug.WriteLine("Receiving [" + filePath + "]");
                
                
                // determine the number of bytes remaining to write to the current file, and read up to that many bytes (or buffer length,
                // whichever is smaller) from the network
                try
                {
                    stopwatch.Start();
                    while (bytesRemaining > 0 && (bytesRead = ns.Read(buf, 0, (int)Utility.Min((long)buf.Length, bytesRemaining))) != 0)
                    {
                        fs.Write(buf, 0, bytesRead);
                        totalBytesWritten += bytesRead;
                        bytesRemaining -= bytesRead;
                        _transfer.BytesTransferred += bytesRead;
                        if (stopwatch.ElapsedMilliseconds > 200)
                        {
                            _transfer.FireDataTransferredEvent();
                            stopwatch.Restart();
                        }
                    }
                    stopwatch.Stop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception while reading data from peer");
                    Debug.WriteLine(ex);

                    _transfer.StateChangeReason = "Error reading data from peer";
                    _transfer.State = TransferState.Failed;

                    ns.Close();
                    fs.Close();
                    if (client.Connected)
                    {
                        client.Close();
                    }

                    return;
                }
                
                // this file is done, move on to the next one
                fs.Close();

                if (bytesRead == 0 && bytesRemaining > 0)  // bytesRead can be 0 even while data remains on the stream, if bytesRemaining was also 0
                {
                    break;
                }
            }

            

            if (totalBytesWritten < totalBytesExpected)
            {
                Debug.WriteLine(String.Format("Expected {0} bytes but only received {1} bytes", totalBytesExpected, totalBytesWritten));
                _transfer.StateChangeReason = String.Format(new FileSizeFormatProvider(), "Peer closed connection after {0:fs} transferred (expected {1:fs})", totalBytesWritten, totalBytesExpected);
                _transfer.State = TransferState.Failed;
            }
            else
            {
                _transfer.State = TransferState.Complete;
            }

            ns.Close();
            client.Close();
            
            
        }

        public void StartSend(TransferInfo transfer)
        {
            checkTransfer(transfer);

            _transfer = transfer;
            _transfer.Agent = this;
            Thread t = new Thread(new ThreadStart(doSend));
            transfer.Thread = t;
            t.Start();
        }

        public void doSend()
        {
            NetworkStream ns = null;
            FileStream fs = null;
            TcpClient client = new TcpClient();

            try
            {
                client.Connect(_transfer.Peer.Address, _transfer.Port);
                ns = client.GetStream();

                List<AppManifestFile> files = _transfer.Manifest.files;
                string rootDir = _transfer.ManifestRoot.FullName;
                Utility.EnsureEndsWithSlash(ref rootDir);
                string filePath;
                byte[] buf = new byte[64 * 1024];

                _transfer.State = TransferState.InProgress;

                for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
                {
                    AppManifestFile currentFile = files[fileIndex];
                    filePath = rootDir + currentFile.path;
                    fs = File.OpenRead(filePath);

                    Debug.WriteLine("Sending [" + filePath + "]");

                    int bytesRead;


                    while ((bytesRead = fs.Read(buf, 0, buf.Length)) != 0)
                    {
                        ns.Write(buf, 0, bytesRead);
                    }
                    fs.Close();
                }

                _transfer.State = TransferState.Complete;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception while sending data to peer");
                Debug.WriteLine(ex);

                _transfer.StateChangeReason = "Error sending data to peer";
                _transfer.State = TransferState.Failed;

            }
            finally
            {
                if (ns != null)
                    ns.Close();

                if (fs != null)
                    fs.Close();

                if (client != null && client.Connected)
                    client.Close();
            }
        }

        public void CancelTransfer() {
            if (_thread != null)
            {
                _thread.Abort();
            }
        }

    }
}
