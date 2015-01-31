namespace SteamLanSync
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.listViewAvailableApps = new System.Windows.Forms.ListView();
            this.labelAvailableApps = new System.Windows.Forms.Label();
            this.buttonRequestApp = new System.Windows.Forms.Button();
            this.progressBarTransfer = new System.Windows.Forms.ProgressBar();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuSysTray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutSteamLanSyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitSteamLanSyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusStripMainLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripUpDownLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblNoApps = new System.Windows.Forms.Label();
            this.labelTransferName = new System.Windows.Forms.Label();
            this.labelTransferSpeed = new System.Windows.Forms.Label();
            this.labelTransferTime = new System.Windows.Forms.Label();
            this.labelMyLibrary = new System.Windows.Forms.Label();
            this.listViewLibrary = new System.Windows.Forms.ListView();
            this.labelNoLibraryApps = new System.Windows.Forms.Label();
            this.menuSysTray.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewAvailableApps
            // 
            this.listViewAvailableApps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listViewAvailableApps.Location = new System.Drawing.Point(378, 33);
            this.listViewAvailableApps.Name = "listViewAvailableApps";
            this.listViewAvailableApps.Size = new System.Drawing.Size(301, 297);
            this.listViewAvailableApps.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewAvailableApps.TabIndex = 13;
            this.listViewAvailableApps.UseCompatibleStateImageBehavior = false;
            this.listViewAvailableApps.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewAvailableApps_ColumnClick);
            this.listViewAvailableApps.SelectedIndexChanged += new System.EventHandler(this.listViewAvailableApps_SelectedIndexChanged);
            // 
            // labelAvailableApps
            // 
            this.labelAvailableApps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAvailableApps.AutoSize = true;
            this.labelAvailableApps.Location = new System.Drawing.Point(375, 14);
            this.labelAvailableApps.Name = "labelAvailableApps";
            this.labelAvailableApps.Size = new System.Drawing.Size(77, 13);
            this.labelAvailableApps.TabIndex = 14;
            this.labelAvailableApps.Text = "Available Apps";
            // 
            // buttonRequestApp
            // 
            this.buttonRequestApp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRequestApp.Enabled = false;
            this.buttonRequestApp.Location = new System.Drawing.Point(567, 336);
            this.buttonRequestApp.Name = "buttonRequestApp";
            this.buttonRequestApp.Size = new System.Drawing.Size(112, 23);
            this.buttonRequestApp.TabIndex = 15;
            this.buttonRequestApp.Text = "Request App";
            this.buttonRequestApp.UseVisualStyleBackColor = true;
            this.buttonRequestApp.Click += new System.EventHandler(this.buttonRequestApp_Click);
            // 
            // progressBarTransfer
            // 
            this.progressBarTransfer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarTransfer.Location = new System.Drawing.Point(378, 387);
            this.progressBarTransfer.Name = "progressBarTransfer";
            this.progressBarTransfer.Size = new System.Drawing.Size(301, 23);
            this.progressBarTransfer.TabIndex = 16;
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.menuSysTray;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "SteamLanSync";
            this.notifyIcon.Visible = true;
            // 
            // menuSysTray
            // 
            this.menuSysTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.toolStripSeparator1,
            this.aboutSteamLanSyncToolStripMenuItem,
            this.exitSteamLanSyncToolStripMenuItem});
            this.menuSysTray.Name = "menuSysTray";
            this.menuSysTray.Size = new System.Drawing.Size(188, 76);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(184, 6);
            // 
            // aboutSteamLanSyncToolStripMenuItem
            // 
            this.aboutSteamLanSyncToolStripMenuItem.Name = "aboutSteamLanSyncToolStripMenuItem";
            this.aboutSteamLanSyncToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.aboutSteamLanSyncToolStripMenuItem.Text = "About SteamLanSync";
            this.aboutSteamLanSyncToolStripMenuItem.Click += new System.EventHandler(this.aboutSteamLanSyncToolStripMenuItem_Click);
            // 
            // exitSteamLanSyncToolStripMenuItem
            // 
            this.exitSteamLanSyncToolStripMenuItem.Name = "exitSteamLanSyncToolStripMenuItem";
            this.exitSteamLanSyncToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.exitSteamLanSyncToolStripMenuItem.Text = "Exit";
            this.exitSteamLanSyncToolStripMenuItem.Click += new System.EventHandler(this.exitSteamLanSyncToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusStripMainLabel,
            this.toolStripStatusLabel1,
            this.toolStripUpDownLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 447);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(696, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 19;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusStripMainLabel
            // 
            this.statusStripMainLabel.Name = "statusStripMainLabel";
            this.statusStripMainLabel.Size = new System.Drawing.Size(16, 17);
            this.statusStripMainLabel.Text = "...";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(665, 17);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStripUpDownLabel
            // 
            this.toolStripUpDownLabel.Name = "toolStripUpDownLabel";
            this.toolStripUpDownLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // lblNoApps
            // 
            this.lblNoApps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNoApps.AutoSize = true;
            this.lblNoApps.BackColor = System.Drawing.SystemColors.Window;
            this.lblNoApps.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoApps.Location = new System.Drawing.Point(391, 46);
            this.lblNoApps.Name = "lblNoApps";
            this.lblNoApps.Size = new System.Drawing.Size(151, 15);
            this.lblNoApps.TabIndex = 20;
            this.lblNoApps.Text = "No apps available to install";
            // 
            // labelTransferName
            // 
            this.labelTransferName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTransferName.AutoSize = true;
            this.labelTransferName.Location = new System.Drawing.Point(375, 371);
            this.labelTransferName.Name = "labelTransferName";
            this.labelTransferName.Size = new System.Drawing.Size(16, 13);
            this.labelTransferName.TabIndex = 22;
            this.labelTransferName.Text = "   ";
            // 
            // labelTransferSpeed
            // 
            this.labelTransferSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTransferSpeed.Location = new System.Drawing.Point(578, 413);
            this.labelTransferSpeed.Name = "labelTransferSpeed";
            this.labelTransferSpeed.Size = new System.Drawing.Size(101, 23);
            this.labelTransferSpeed.TabIndex = 23;
            this.labelTransferSpeed.Text = "    ";
            this.labelTransferSpeed.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelTransferTime
            // 
            this.labelTransferTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTransferTime.AutoSize = true;
            this.labelTransferTime.Location = new System.Drawing.Point(375, 413);
            this.labelTransferTime.Name = "labelTransferTime";
            this.labelTransferTime.Size = new System.Drawing.Size(19, 13);
            this.labelTransferTime.TabIndex = 24;
            this.labelTransferTime.Text = "    ";
            // 
            // labelMyLibrary
            // 
            this.labelMyLibrary.AutoSize = true;
            this.labelMyLibrary.Location = new System.Drawing.Point(12, 14);
            this.labelMyLibrary.Name = "labelMyLibrary";
            this.labelMyLibrary.Size = new System.Drawing.Size(88, 13);
            this.labelMyLibrary.TabIndex = 11;
            this.labelMyLibrary.Text = "My Steam Library";
            // 
            // listViewLibrary
            // 
            this.listViewLibrary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listViewLibrary.FullRowSelect = true;
            this.listViewLibrary.Location = new System.Drawing.Point(12, 33);
            this.listViewLibrary.Name = "listViewLibrary";
            this.listViewLibrary.Size = new System.Drawing.Size(312, 297);
            this.listViewLibrary.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewLibrary.TabIndex = 12;
            this.listViewLibrary.UseCompatibleStateImageBehavior = false;
            // 
            // labelNoLibraryApps
            // 
            this.labelNoLibraryApps.AutoSize = true;
            this.labelNoLibraryApps.BackColor = System.Drawing.SystemColors.Window;
            this.labelNoLibraryApps.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNoLibraryApps.Location = new System.Drawing.Point(24, 46);
            this.labelNoLibraryApps.Name = "labelNoLibraryApps";
            this.labelNoLibraryApps.Size = new System.Drawing.Size(130, 15);
            this.labelNoLibraryApps.TabIndex = 21;
            this.labelNoLibraryApps.Text = "No apps in your Library";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 469);
            this.Controls.Add(this.labelTransferTime);
            this.Controls.Add(this.labelTransferSpeed);
            this.Controls.Add(this.labelTransferName);
            this.Controls.Add(this.labelNoLibraryApps);
            this.Controls.Add(this.lblNoApps);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.progressBarTransfer);
            this.Controls.Add(this.buttonRequestApp);
            this.Controls.Add(this.labelAvailableApps);
            this.Controls.Add(this.listViewAvailableApps);
            this.Controls.Add(this.listViewLibrary);
            this.Controls.Add(this.labelMyLibrary);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "FormMain";
            this.Text = "SteamLanSync";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.menuSysTray.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewAvailableApps;
        private System.Windows.Forms.Label labelAvailableApps;
        private System.Windows.Forms.Button buttonRequestApp;
        private System.Windows.Forms.ProgressBar progressBarTransfer;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip menuSysTray;
        private System.Windows.Forms.ToolStripMenuItem exitSteamLanSyncToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusStripMainLabel;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem aboutSteamLanSyncToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripUpDownLabel;
        private System.Windows.Forms.Label lblNoApps;
        private System.Windows.Forms.Label labelTransferName;
        private System.Windows.Forms.Label labelTransferSpeed;
        private System.Windows.Forms.Label labelTransferTime;
        private System.Windows.Forms.Label labelMyLibrary;
        private System.Windows.Forms.ListView listViewLibrary;
        private System.Windows.Forms.Label labelNoLibraryApps;
    }
}

