﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using SteamLanSync.Messages;
using System.Diagnostics;
using System.IO;

namespace SteamLanSync
{
    public partial class Form1 : Form
    {
        SyncManager manager;
        
        private System.Timers.Timer _updateSpeedTimer;
        
        private delegate void LogDataCallback(string s);
        private delegate void SyncPeerUpdatedCallback(object sender, SyncPeerUpdatedEventArgs e);

        public Form1()
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;
            this.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string libPath = Properties.Settings.Default.LibraryPath;

            if (AppLibrary.IsValidLibraryPath(libPath))
            {
                restartSyncManager();
            }
            else
            {
                string[] libs = AppLibrary.DetermineLibraryLocation();
                if (libs.Length == 1)
                {
                    DialogResult r = MessageBox.Show(String.Format("{0}\n\nIs this your Steam Library location?", libs[0]), "Library Location", MessageBoxButtons.YesNo);
                    if (r == DialogResult.Yes)
                    {
                        // set library location
                        Properties.Settings.Default.LibraryPath = libs[0];
                        Properties.Settings.Default.Save();
                        restartSyncManager();
                    }
                    else
                    {
                        // show "select library location" form
                        // todo provide autodetected paths to settings dialog
                        showSettingsDialog();
                    }
                }
                else
                {
                    // show "select library location" form
                    showSettingsDialog();
                }
            }
            

            this.Show();

            _updateSpeedTimer = new System.Timers.Timer(300);
            _updateSpeedTimer.Elapsed += updateSpeedCounters;
            _updateSpeedTimer.AutoReset = true;
            _updateSpeedTimer.SynchronizingObject = this;
            _updateSpeedTimer.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            if (_updateSpeedTimer != null)
            {
                _updateSpeedTimer.Stop();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (manager != null)
            {
                manager.Stop();
            }
        }


        private void restartSyncManager()
        {
            if (manager != null)
            {
                manager.Stop();
            }

            string libPath = Properties.Settings.Default.LibraryPath;
            if (!AppLibrary.IsValidLibraryPath(libPath))
            {
                statusStripMainLabel.Text = "Cannot sync because [" + libPath + "] is not a valid Steam Library";
                return;
            }

            manager = new SyncManager(Properties.Settings.Default.BroadcastPort, Properties.Settings.Default.ListenPort, Properties.Settings.Default.LibraryPath);
            manager.OnSyncPeerUpdated += managerUpdatedSyncPeer;
            manager.OnTransferCreated += transferCreated;
            manager.OnTransferRemoved += transferRemoved;
            manager.OnStatusUpdated += managerUpdatedStatus;
            manager.OnAvailableAppsUpdated += managerUpdatedAvailableApps;
            manager.Start();
            refreshLibraryListView();
        }

        
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (manager != null)
            {
                manager.Stop();
            }
        }

        private void refreshLibraryListView()
        {
            if (manager == null || manager.Library == null)
            {
                return;
            }

            if (listViewLibrary.Columns.Count == 0) // once off
            {
                listViewLibrary.Columns.Add("Name", 200);
                listViewLibrary.Columns.Add("Size", 75, HorizontalAlignment.Right);
                listViewLibrary.View = View.Details;
            }

            ListViewItem lvi;
            FileSizeFormatProvider fmt = new FileSizeFormatProvider();
            foreach (AppInfo app in manager.Library.Apps.Values)
            {
                lvi = new ListViewItem(new string[] { app.Name, String.Format(fmt, "{0:fs}", app.Size) });
                lvi.Tag = app;
                listViewLibrary.Items.Add(lvi);
                
            }
            listViewLibrary.Sort();
        }

        private void refreshAvailableAppsListView()
        {
            if (manager == null || manager.Library == null)
            {
                return;
            }

            listViewAvailableApps.Clear();
            listViewAvailableApps.Columns.Add("Name", 200);
            listViewAvailableApps.Columns.Add("Size", 75, HorizontalAlignment.Right);
            listViewAvailableApps.View = View.Details;

            ListViewItem lvi;
            FileSizeFormatProvider fmt = new FileSizeFormatProvider();
            foreach (AppInfo app in manager.AvailableApps)
            {
                lvi = new ListViewItem(new string[] { app.Name, String.Format(fmt, "{0:fs}", app.Size) });
                lvi.Tag = app;
                listViewAvailableApps.Items.Add(lvi);
            }

            lblNoApps.Visible = listViewAvailableApps.Items.Count == 0;

            refreshLibraryListView(); // todo - set up a separate event handler
        }

        private void managerUpdatedSyncPeer(object sender, SyncPeerUpdatedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                SyncPeerUpdatedCallback d = new SyncPeerUpdatedCallback(managerUpdatedSyncPeer);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                refreshAvailableAppsListView();
            }
        }

        private void managerUpdatedStatus(object sender, SyncManagerUpdatedStatusEventArgs e)
        {
            if (this.InvokeRequired)
            {
                SyncManagerUpdatedStatusEventHandler d = new SyncManagerUpdatedStatusEventHandler(managerUpdatedStatus);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                statusStripMainLabel.Text = e.Status;
            }
        }

        private void managerUpdatedAvailableApps(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                AvailableAppsUpdatedEventHandler d = new AvailableAppsUpdatedEventHandler(managerUpdatedAvailableApps);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                refreshAvailableAppsListView();
            }
        }

        private void transferCreated(object sender, TransferEventArgs e)
        {
            // UI only care about transfers which we initiated -- let the agent handle the other side
            if (e.Transfer.IsSending == false)
            {
                e.Transfer.OnDataTransferred += transferDataTransferred;
                e.Transfer.OnTransferStateChanged += transferStateChanged;
            }
        }

        private void transferStateChanged(object sender, TransferStateChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                TransferStateChangedEventHandler d = new TransferStateChangedEventHandler(transferStateChanged);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                TransferInfo transfer = (TransferInfo)sender;
                if (e.NewState == TransferState.Failed)
                { // show message box with failure reason
                    string msg = "Unable to sync " + transfer.App.Name + ".";
                    if (e.Reason.Length > 0) { msg += " " + e.Reason + "."; }
                    MessageBox.Show(msg, "Transfer Failed");
                }
                else if (e.NewState == TransferState.Complete && !transfer.IsSending)
                {
                    progressBarTransfer.Maximum = 10;
                    progressBarTransfer.Value = 10;
                    
                    MessageBox.Show(String.Format("{0} has finished syncing!", transfer.App.Name), "Transfer Complete");
                    
                }
                
            }
        }

        private void transferDataTransferred(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                DataTransferredEventHandler d = new DataTransferredEventHandler(transferDataTransferred);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                TransferInfo transfer = (TransferInfo)sender;
                progressBarTransfer.Maximum = (int)(transfer.TotalBytes / 1024);
                progressBarTransfer.Value = (int)(transfer.BytesTransferred / 1024);

            }
        }

        private void transferRemoved(object sender, TransferEventArgs e)
        {
            if (this.InvokeRequired)
            {
                TransferRemovedEventHandler d = new TransferRemovedEventHandler(transferRemoved);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                //todo
            }
        }

        private void updateSpeedCounters(object sender, EventArgs e)
        {
            if (this.Disposing || this.IsDisposed)
                return;
           
            if (manager.Transfers.Count == 0)
                return;

            TransferInfo[] downloads = manager.Transfers.Values.Where((t) => { return t.State == TransferState.InProgress && t.IsSending == false; }).ToArray();
            long totalDownSpeed = 0;
            for (int i = 0; i < downloads.Length; i++)
            {
                totalDownSpeed += downloads[i].TransferSpeed;
            }

            TransferInfo[] uploads = manager.Transfers.Values.Where((t) => { return t.State == TransferState.InProgress && t.IsSending == true; }).ToArray();
            long totalUpSpeed = 0;
            for (int i = 0; i < uploads.Length; i++)
            {
                totalUpSpeed += uploads[i].TransferSpeed;
            }

            toolStripUpDownLabel.Text = String.Format(new FileSizeFormatProvider(), "Up: {0:fs}/s Dn: {1:fs}/s", totalUpSpeed, totalDownSpeed);
        }

        private void buttonRequestApp_Click(object sender, EventArgs e)
        {
            if (listViewAvailableApps.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (ListViewItem lvi in listViewAvailableApps.SelectedItems)
            {
                AppInfo app = (AppInfo)lvi.Tag;
                manager.RequestApp(app);
            }
            
        }

        private void exitSteamLanSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSettingsDialog();
        }

        private void showSettingsDialog()
        {
            FormSettings settingsForm = new FormSettings();
            DialogResult res = settingsForm.ShowDialog(this);
            string selectedPath = settingsForm.SelectedLibraryLocation;
            string currentLibPath = Properties.Settings.Default.LibraryPath;

            if (res == DialogResult.OK && selectedPath.ToUpper() != currentLibPath.ToUpper())
            {
                // setting to the config file
                Properties.Settings.Default.LibraryPath = selectedPath;
                Properties.Settings.Default.Save();

                restartSyncManager();
                
            }
            
            settingsForm.Dispose();

            
        }

        private void aboutSteamLanSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new FormAbout()).Show();
        }

        private void listViewAvailableApps_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            buttonRequestApp.Enabled = lv.SelectedItems.Count > 0;
        }
    }
}
