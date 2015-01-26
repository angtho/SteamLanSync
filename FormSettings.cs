using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamLanSync
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
            Font = SystemFonts.MessageBoxFont;
        }

        public string SelectedLibraryLocation
        {
            get {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            
            DialogResult res = browserSelectLibraryLocation.ShowDialog();
            if (res == DialogResult.OK)
                textBox1.Text = browserSelectLibraryLocation.SelectedPath;
        }
    }
}
