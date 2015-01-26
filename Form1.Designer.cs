namespace SteamLanSync
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label3 = new System.Windows.Forms.Label();
            this.listViewLibrary = new System.Windows.Forms.ListView();
            this.listViewAvailableApps = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
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
            this.labelNoLibraryApps = new System.Windows.Forms.Label();
            this.menuSysTray.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "My Steam Library";
            // 
            // listViewLibrary
            // 
            this.listViewLibrary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listViewLibrary.FullRowSelect = true;
            this.listViewLibrary.Location = new System.Drawing.Point(12, 33);
            this.listViewLibrary.Name = "listViewLibrary";
            this.listViewLibrary.Size = new System.Drawing.Size(312, 302);
            this.listViewLibrary.TabIndex = 12;
            this.listViewLibrary.UseCompatibleStateImageBehavior = false;
            // 
            // listViewAvailableApps
            // 
            this.listViewAvailableApps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewAvailableApps.Location = new System.Drawing.Point(346, 33);
            this.listViewAvailableApps.Name = "listViewAvailableApps";
            this.listViewAvailableApps.Size = new System.Drawing.Size(301, 302);
            this.listViewAvailableApps.TabIndex = 13;
            this.listViewAvailableApps.UseCompatibleStateImageBehavior = false;
            this.listViewAvailableApps.SelectedIndexChanged += new System.EventHandler(this.listViewAvailableApps_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(343, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Available Apps";
            // 
            // buttonRequestApp
            // 
            this.buttonRequestApp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRequestApp.Enabled = false;
            this.buttonRequestApp.Location = new System.Drawing.Point(535, 341);
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
            this.progressBarTransfer.Location = new System.Drawing.Point(346, 380);
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
            this.statusStrip.Location = new System.Drawing.Point(0, 427);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(664, 22);
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
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(633, 17);
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
            this.lblNoApps.Location = new System.Drawing.Point(359, 46);
            this.lblNoApps.Name = "lblNoApps";
            this.lblNoApps.Size = new System.Drawing.Size(151, 15);
            this.lblNoApps.TabIndex = 20;
            this.lblNoApps.Text = "No apps available to install";
            // 
            // labelNoLibraryApps
            // 
            this.labelNoLibraryApps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelNoLibraryApps.AutoSize = true;
            this.labelNoLibraryApps.BackColor = System.Drawing.SystemColors.Window;
            this.labelNoLibraryApps.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNoLibraryApps.Location = new System.Drawing.Point(24, 46);
            this.labelNoLibraryApps.Name = "labelNoLibraryApps";
            this.labelNoLibraryApps.Size = new System.Drawing.Size(130, 15);
            this.labelNoLibraryApps.TabIndex = 21;
            this.labelNoLibraryApps.Text = "No apps in your Library";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 449);
            this.Controls.Add(this.labelNoLibraryApps);
            this.Controls.Add(this.lblNoApps);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.progressBarTransfer);
            this.Controls.Add(this.buttonRequestApp);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listViewAvailableApps);
            this.Controls.Add(this.listViewLibrary);
            this.Controls.Add(this.label3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "SteamLanSync";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuSysTray.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView listViewLibrary;
        private System.Windows.Forms.ListView listViewAvailableApps;
        private System.Windows.Forms.Label label4;
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
        private System.Windows.Forms.Label labelNoLibraryApps;
    }
}

