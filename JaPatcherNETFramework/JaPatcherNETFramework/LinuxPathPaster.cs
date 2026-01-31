using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JaPatcherNETFramework
{
    public partial class LinuxPathPaster : Form
    {
        internal JaPatcherLogic Logic;

        public LinuxPathPaster()
        {
            InitializeComponent();

            JaPatcherLogic.LoadIcon(this);
        }

        private void PasteBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            var path = LinuxUtils.ToWinePath(PasteBox.Text);

            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Please enter a valid path.");
                return;
            }

            if(!File.Exists(Path.Combine(path, "Jalopy.exe")))
            {
                MessageBox.Show("The path you entered does not appear to contain Jalopy.exe. Please check the path and try again.");
                return;
            }

            Logic.Window.CheckPatchedStatus(path);
            this.Close();
        }
    }
}
