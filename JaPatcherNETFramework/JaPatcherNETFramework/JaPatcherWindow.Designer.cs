namespace JaPatcherNETFramework
{
    partial class JaPatcherWindow
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkForUpdatesButton = new System.Windows.Forms.Button();
            this.installButton = new System.Windows.Forms.Button();
            this.locateButton = new System.Windows.Forms.Button();
            this.StatusText = new System.Windows.Forms.Label();
            this.JalopyLocationText = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.SelectCustomPathButton = new System.Windows.Forms.Button();
            this.customModsLocationText = new System.Windows.Forms.Label();
            this.customButton = new System.Windows.Forms.RadioButton();
            this.jalopyModsLocationText = new System.Windows.Forms.Label();
            this.gameFolderButton = new System.Windows.Forms.RadioButton();
            this.documentsModsLocationText = new System.Windows.Forms.Label();
            this.documentsButton = new System.Windows.Forms.RadioButton();
            this.jaPatcherVersionText = new System.Windows.Forms.Label();
            this.needHelpButton = new System.Windows.Forms.LinkLabel();
            this.JaLoaderVersion = new System.Windows.Forms.Label();
            this.locateFolderDialog = new System.Windows.Forms.OpenFileDialog();
            this.customModsDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkForUpdatesButton);
            this.groupBox1.Controls.Add(this.installButton);
            this.groupBox1.Controls.Add(this.locateButton);
            this.groupBox1.Controls.Add(this.StatusText);
            this.groupBox1.Controls.Add(this.JalopyLocationText);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(496, 79);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Information";
            // 
            // checkForUpdatesButton
            // 
            this.checkForUpdatesButton.Location = new System.Drawing.Point(270, 50);
            this.checkForUpdatesButton.Name = "checkForUpdatesButton";
            this.checkForUpdatesButton.Size = new System.Drawing.Size(120, 23);
            this.checkForUpdatesButton.TabIndex = 6;
            this.checkForUpdatesButton.Text = "Check for updates";
            this.checkForUpdatesButton.UseVisualStyleBackColor = true;
            this.checkForUpdatesButton.Click += new System.EventHandler(this.checkForUpdatesButton_Click);
            // 
            // installButton
            // 
            this.installButton.Enabled = false;
            this.installButton.Location = new System.Drawing.Point(396, 51);
            this.installButton.Name = "installButton";
            this.installButton.Size = new System.Drawing.Size(89, 23);
            this.installButton.TabIndex = 5;
            this.installButton.Text = "Install";
            this.installButton.UseVisualStyleBackColor = true;
            this.installButton.Click += new System.EventHandler(this.installButton_Click);
            // 
            // locateButton
            // 
            this.locateButton.Location = new System.Drawing.Point(12, 51);
            this.locateButton.Name = "locateButton";
            this.locateButton.Size = new System.Drawing.Size(87, 23);
            this.locateButton.TabIndex = 4;
            this.locateButton.Text = "Locate Jalopy";
            this.locateButton.UseVisualStyleBackColor = true;
            this.locateButton.Click += new System.EventHandler(this.locateButton_Click);
            // 
            // StatusText
            // 
            this.StatusText.AutoSize = true;
            this.StatusText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StatusText.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusText.Location = new System.Drawing.Point(50, 35);
            this.StatusText.Name = "StatusText";
            this.StatusText.Size = new System.Drawing.Size(73, 13);
            this.StatusText.TabIndex = 3;
            this.StatusText.Text = "Not Selected";
            // 
            // JalopyLocationText
            // 
            this.JalopyLocationText.AutoSize = true;
            this.JalopyLocationText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.JalopyLocationText.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.JalopyLocationText.Location = new System.Drawing.Point(96, 16);
            this.JalopyLocationText.Name = "JalopyLocationText";
            this.JalopyLocationText.Size = new System.Drawing.Size(73, 13);
            this.JalopyLocationText.TabIndex = 2;
            this.JalopyLocationText.Text = "Not Selected";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Status:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Jalopy location:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.SelectCustomPathButton);
            this.groupBox2.Controls.Add(this.customModsLocationText);
            this.groupBox2.Controls.Add(this.customButton);
            this.groupBox2.Controls.Add(this.jalopyModsLocationText);
            this.groupBox2.Controls.Add(this.gameFolderButton);
            this.groupBox2.Controls.Add(this.documentsModsLocationText);
            this.groupBox2.Controls.Add(this.documentsButton);
            this.groupBox2.Location = new System.Drawing.Point(6, 97);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(496, 135);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Mod folder";
            // 
            // SelectCustomPathButton
            // 
            this.SelectCustomPathButton.Enabled = false;
            this.SelectCustomPathButton.Location = new System.Drawing.Point(73, 88);
            this.SelectCustomPathButton.Name = "SelectCustomPathButton";
            this.SelectCustomPathButton.Size = new System.Drawing.Size(81, 23);
            this.SelectCustomPathButton.TabIndex = 12;
            this.SelectCustomPathButton.Text = "Select path";
            this.SelectCustomPathButton.UseVisualStyleBackColor = true;
            this.SelectCustomPathButton.Click += new System.EventHandler(this.SelectCustomPathButton_Click);
            // 
            // customModsLocationText
            // 
            this.customModsLocationText.AutoSize = true;
            this.customModsLocationText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.customModsLocationText.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.customModsLocationText.Location = new System.Drawing.Point(37, 111);
            this.customModsLocationText.Name = "customModsLocationText";
            this.customModsLocationText.Size = new System.Drawing.Size(157, 13);
            this.customModsLocationText.TabIndex = 11;
            this.customModsLocationText.Text = "Waiting for Jalopy location...";
            // 
            // customButton
            // 
            this.customButton.AutoSize = true;
            this.customButton.Enabled = false;
            this.customButton.Location = new System.Drawing.Point(12, 91);
            this.customButton.Name = "customButton";
            this.customButton.Size = new System.Drawing.Size(60, 17);
            this.customButton.TabIndex = 10;
            this.customButton.Text = "Custom";
            this.customButton.UseVisualStyleBackColor = true;
            this.customButton.CheckedChanged += new System.EventHandler(this.customButton_CheckedChanged);
            // 
            // jalopyModsLocationText
            // 
            this.jalopyModsLocationText.AutoSize = true;
            this.jalopyModsLocationText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.jalopyModsLocationText.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.jalopyModsLocationText.Location = new System.Drawing.Point(37, 75);
            this.jalopyModsLocationText.Name = "jalopyModsLocationText";
            this.jalopyModsLocationText.Size = new System.Drawing.Size(157, 13);
            this.jalopyModsLocationText.TabIndex = 9;
            this.jalopyModsLocationText.Text = "Waiting for Jalopy location...";
            // 
            // gameFolderButton
            // 
            this.gameFolderButton.AutoSize = true;
            this.gameFolderButton.Enabled = false;
            this.gameFolderButton.Location = new System.Drawing.Point(12, 55);
            this.gameFolderButton.Name = "gameFolderButton";
            this.gameFolderButton.Size = new System.Drawing.Size(118, 17);
            this.gameFolderButton.TabIndex = 8;
            this.gameFolderButton.Text = "Jalopy Game Folder";
            this.gameFolderButton.UseVisualStyleBackColor = true;
            this.gameFolderButton.CheckedChanged += new System.EventHandler(this.gameFolderButton_CheckedChanged);
            // 
            // documentsModsLocationText
            // 
            this.documentsModsLocationText.AutoSize = true;
            this.documentsModsLocationText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.documentsModsLocationText.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.documentsModsLocationText.Location = new System.Drawing.Point(37, 39);
            this.documentsModsLocationText.Name = "documentsModsLocationText";
            this.documentsModsLocationText.Size = new System.Drawing.Size(157, 13);
            this.documentsModsLocationText.TabIndex = 7;
            this.documentsModsLocationText.Text = "Waiting for Jalopy location...";
            // 
            // documentsButton
            // 
            this.documentsButton.AutoSize = true;
            this.documentsButton.Checked = true;
            this.documentsButton.Enabled = false;
            this.documentsButton.Location = new System.Drawing.Point(12, 19);
            this.documentsButton.Name = "documentsButton";
            this.documentsButton.Size = new System.Drawing.Size(79, 17);
            this.documentsButton.TabIndex = 0;
            this.documentsButton.TabStop = true;
            this.documentsButton.Text = "Documents";
            this.documentsButton.UseVisualStyleBackColor = true;
            this.documentsButton.CheckedChanged += new System.EventHandler(this.documentsButton_CheckedChanged);
            // 
            // jaPatcherVersionText
            // 
            this.jaPatcherVersionText.AutoSize = true;
            this.jaPatcherVersionText.Location = new System.Drawing.Point(3, 235);
            this.jaPatcherVersionText.Name = "jaPatcherVersionText";
            this.jaPatcherVersionText.Size = new System.Drawing.Size(22, 13);
            this.jaPatcherVersionText.TabIndex = 2;
            this.jaPatcherVersionText.Text = "1.0";
            // 
            // needHelpButton
            // 
            this.needHelpButton.AutoSize = true;
            this.needHelpButton.Location = new System.Drawing.Point(440, 235);
            this.needHelpButton.Name = "needHelpButton";
            this.needHelpButton.Size = new System.Drawing.Size(62, 13);
            this.needHelpButton.TabIndex = 3;
            this.needHelpButton.TabStop = true;
            this.needHelpButton.Text = "Need help?";
            this.needHelpButton.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.needHelpButton_LinkClicked);
            // 
            // JaLoaderVersion
            // 
            this.JaLoaderVersion.AutoSize = true;
            this.JaLoaderVersion.Location = new System.Drawing.Point(31, 235);
            this.JaLoaderVersion.Name = "JaLoaderVersion";
            this.JaLoaderVersion.Size = new System.Drawing.Size(78, 13);
            this.JaLoaderVersion.TabIndex = 4;
            this.JaLoaderVersion.Text = "JaLoader 4.0.5";
            // 
            // locateFolderDialog
            // 
            this.locateFolderDialog.FileName = "Jalopy.exe";
            this.locateFolderDialog.Filter = "Jalopy executable|Jalopy.exe";
            // 
            // JaPatcherWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 253);
            this.Controls.Add(this.JaLoaderVersion);
            this.Controls.Add(this.needHelpButton);
            this.Controls.Add(this.jaPatcherVersionText);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JaPatcherWindow";
            this.Text = "JaPatcher";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label JalopyLocationText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button checkForUpdatesButton;
        private System.Windows.Forms.Button installButton;
        private System.Windows.Forms.Button locateButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label documentsModsLocationText;
        private System.Windows.Forms.RadioButton documentsButton;
        private System.Windows.Forms.Label customModsLocationText;
        private System.Windows.Forms.RadioButton customButton;
        private System.Windows.Forms.Label jalopyModsLocationText;
        private System.Windows.Forms.RadioButton gameFolderButton;
        private System.Windows.Forms.Label StatusText;
        private System.Windows.Forms.Label jaPatcherVersionText;
        private System.Windows.Forms.LinkLabel needHelpButton;
        private System.Windows.Forms.Label JaLoaderVersion;
        private System.Windows.Forms.OpenFileDialog locateFolderDialog;
        private System.Windows.Forms.FolderBrowserDialog customModsDialog;
        private System.Windows.Forms.Button SelectCustomPathButton;
    }
}

