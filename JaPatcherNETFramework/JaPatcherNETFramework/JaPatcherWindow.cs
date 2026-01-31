using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JaPatcherNETFramework
{
    public partial class JaPatcherWindow : Form
    {
        internal JaPatcherLogic logic = new JaPatcherLogic();

        public JaPatcherWindow()
        {
            InitializeComponent();

            JaPatcherLogic.LoadIcon(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logic.Window = this;
            logic.CurrentDirectory = Directory.GetCurrentDirectory();
            needHelpButton.Links[0].LinkData = logic.HelpLink;
            jaPatcherVersionText.Text = logic.Version;

            logic.StartupChecks();
            if (logic.IsLinux)
            {
                locateFolderDialog.Dispose();

                if (string.IsNullOrEmpty(logic.GamePath))
                {
                    MessageBox.Show("It looks like you're running JaPatcher using Wine on Linux. Instead of locating the folder via the file browser, please paste the path to where Jalopy is installed, excluding Jalopy.exe.", "JaPatcher - Linux Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    CreateLinuxPathPaster();
                }
            }
            JaLoaderVersion.Text = $"JaLoader version: {logic.GetJaLoaderVersionFromDLL(false)}";
        }

        internal void CreateLinuxPathPaster()
        {
            LinuxPathPaster pathPaster = new LinuxPathPaster
            {
                Logic = logic
            };
            pathPaster.ShowDialog();
        }

        internal void WarnAboutMissingFiles(string missingFile, string whatFolder)
        {
            MessageBox.Show($"The file {missingFile}, from the {whatFolder} folder, is missing. Please extract all of the contents from the archive!", "JaPatcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }

        internal void UpdateStatusTextAndInstallButton()
        {
            string newText = "";

            switch (logic.PatchedStatus)
            {
                case PatchedStatus.Patched:
                    newText = "Installed";
                    StatusText.ForeColor = Color.Green;
                    installButton.Text = "Uninstall";
                    break;

                case PatchedStatus.NotPatched:
                    newText = "Not Installed";
                    StatusText.ForeColor = Color.Red;
                    installButton.Text = "Install";
                    break;

                case PatchedStatus.PatchedIncomplete:
                    newText = "Installed, missing files";
                    StatusText.ForeColor = Color.Orange;
                    installButton.Text = "Fix installation";
                    break;

                case PatchedStatus.PatchedOutdated:
                    newText = "Installed, outdated version";
                    StatusText.ForeColor = Color.Orange;
                    installButton.Text = "Update";
                    break;
            }

            if(logic.PatchedStatus != PatchedStatus.NotPatched)
                newText += $" (Version {logic.GetJaLoaderVersionFromDLL(true)})";

            StatusText.Text = newText;
        }

        private void needHelpButton_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            logic.OpenLink();
        }

        private void locateButton_Click(object sender, EventArgs e)
        {
            if (logic.IsLinux)
            {
                CreateLinuxPathPaster();
                return;
            }

            if(locateFolderDialog.ShowDialog() == DialogResult.OK)
            {
                CheckPatchedStatus(locateFolderDialog.FileName);
            }
        }

        internal void CheckPatchedStatus(string path)
        {
            if (logic.ReturnPatchedStatus(path))
            {
                UpdateStatusTextAndInstallButton();
            }

            EnableButtonsAfterLocation();
            UpdateModLocationButtonsText();
        }

        internal void UpdateModLocationButtonsText()
        {
            documentsModsLocationText.Text = logic.DocumentsModsLocation;
            jalopyModsLocationText.Text = logic.GameFolderModsLocation;
            customModsLocationText.Text = string.IsNullOrEmpty(logic.CustomModsLocation) ? "No folder selected" : logic.CustomModsLocation;
        }

        internal void EnableButtonsAfterLocation()
        {
            installButton.Enabled = true;
            documentsButton.Enabled = gameFolderButton.Enabled = customButton.Enabled = true;
            JalopyLocationText.Text = logic.GamePath;
        }

        private void documentsButton_CheckedChanged(object sender, EventArgs e)
        {
            logic.SelectDocumentsFolder();
        }

        private void gameFolderButton_CheckedChanged(object sender, EventArgs e)
        {
            logic.SelectGameFolderModsFolder();
        }

        private void customButton_CheckedChanged(object sender, EventArgs e)
        {
            if(customButton.Checked)
                SelectCustomPathButton.Enabled = true;
            else
                SelectCustomPathButton.Enabled = false;

            if(!string.IsNullOrEmpty(logic.CustomModsLocation))
                logic.SelectCustomModsFolder(customModsDialog.SelectedPath);
        }

        private void SelectCustomPathButton_Click(object sender, EventArgs e)
        {
            if (customModsDialog.ShowDialog() == DialogResult.OK)
            {
                logic.SelectCustomModsFolder(customModsDialog.SelectedPath);
                customModsLocationText.Text = customModsDialog.SelectedPath;
            }
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            switch(logic.PatchedStatus)
            {
                case PatchedStatus.Patched:
                    if(MessageBox.Show("Are you sure you want to uninstall JaLoader?", "JaPatcher", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        logic.UninstallJaLoader();
                    break;
                case PatchedStatus.PatchedIncomplete:
                case PatchedStatus.PatchedOutdated:
                case PatchedStatus.NotPatched:
                    logic.InstallOrFixJaLoader();
                    break;
            }
        }

        internal void checkForUpdatesButton_Click(object sender, EventArgs e)
        {
            logic.CheckForUpdates();
        }
    }
}
