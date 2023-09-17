namespace JalopyModLoader
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Label installedText;
            Label gameFolderText;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            groupBox1 = new GroupBox();
            updateButton = new Button();
            launchButton = new Button();
            installButton = new Button();
            locateFolderButton = new Button();
            installedTextValue = new Label();
            folderTextValue = new Label();
            locateFolderDialog = new OpenFileDialog();
            groupBox2 = new GroupBox();
            customFolderModsText = new Label();
            customModsLocationButton = new RadioButton();
            gameFolderModsText = new Label();
            documentsModsText = new Label();
            gameFolderModsButton = new RadioButton();
            documentsButton = new RadioButton();
            customModsDialog = new FolderBrowserDialog();
            installedText = new Label();
            gameFolderText = new Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // installedText
            // 
            installedText.AutoSize = true;
            installedText.Location = new Point(6, 45);
            installedText.Name = "installedText";
            installedText.Size = new Size(54, 15);
            installedText.TabIndex = 1;
            installedText.Text = "Installed:";
            // 
            // gameFolderText
            // 
            gameFolderText.AutoSize = true;
            gameFolderText.Location = new Point(6, 19);
            gameFolderText.Name = "gameFolderText";
            gameFolderText.Size = new Size(79, 15);
            gameFolderText.TabIndex = 0;
            gameFolderText.Text = "Jalopy Folder:";
            // 
            // groupBox1
            // 
            groupBox1.BackColor = SystemColors.Control;
            groupBox1.Controls.Add(updateButton);
            groupBox1.Controls.Add(launchButton);
            groupBox1.Controls.Add(installButton);
            groupBox1.Controls.Add(locateFolderButton);
            groupBox1.Controls.Add(installedTextValue);
            groupBox1.Controls.Add(folderTextValue);
            groupBox1.Controls.Add(installedText);
            groupBox1.Controls.Add(gameFolderText);
            groupBox1.ForeColor = SystemColors.ControlText;
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(541, 96);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Information";
            // 
            // updateButton
            // 
            updateButton.ForeColor = SystemColors.ControlText;
            updateButton.ImeMode = ImeMode.NoControl;
            updateButton.Location = new Point(387, 67);
            updateButton.Name = "updateButton";
            updateButton.Size = new Size(71, 23);
            updateButton.TabIndex = 8;
            updateButton.Text = "Update";
            updateButton.UseVisualStyleBackColor = true;
            updateButton.Visible = false;
            updateButton.Click += updateButton_Click;
            // 
            // launchButton
            // 
            launchButton.ForeColor = SystemColors.ControlText;
            launchButton.ImeMode = ImeMode.NoControl;
            launchButton.Location = new Point(138, 67);
            launchButton.Name = "launchButton";
            launchButton.Size = new Size(71, 23);
            launchButton.TabIndex = 7;
            launchButton.Text = "Launch";
            launchButton.UseVisualStyleBackColor = true;
            launchButton.Visible = false;
            launchButton.Click += launchButton_Click;
            // 
            // installButton
            // 
            installButton.ForeColor = SystemColors.ControlText;
            installButton.ImeMode = ImeMode.NoControl;
            installButton.Location = new Point(464, 67);
            installButton.Name = "installButton";
            installButton.Size = new Size(71, 23);
            installButton.TabIndex = 6;
            installButton.Text = "Install";
            installButton.UseVisualStyleBackColor = true;
            installButton.Visible = false;
            installButton.Click += installButton_Click;
            // 
            // locateFolderButton
            // 
            locateFolderButton.Location = new Point(4, 67);
            locateFolderButton.Name = "locateFolderButton";
            locateFolderButton.Size = new Size(128, 23);
            locateFolderButton.TabIndex = 4;
            locateFolderButton.Text = "Locate Jalopy Folder";
            locateFolderButton.UseVisualStyleBackColor = true;
            locateFolderButton.Click += locateFolderButton_Click;
            // 
            // installedTextValue
            // 
            installedTextValue.AutoSize = true;
            installedTextValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            installedTextValue.ForeColor = SystemColors.ControlText;
            installedTextValue.Location = new Point(57, 45);
            installedTextValue.Name = "installedTextValue";
            installedTextValue.Size = new Size(80, 15);
            installedTextValue.TabIndex = 3;
            installedTextValue.Text = "Not Selected";
            // 
            // folderTextValue
            // 
            folderTextValue.AutoSize = true;
            folderTextValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            folderTextValue.Location = new Point(81, 19);
            folderTextValue.Name = "folderTextValue";
            folderTextValue.Size = new Size(80, 15);
            folderTextValue.TabIndex = 2;
            folderTextValue.Text = "Not Selected";
            // 
            // locateFolderDialog
            // 
            locateFolderDialog.FileName = "Jalopy.exe";
            locateFolderDialog.Filter = "Jalopy executable|*Jalopy.exe|All files|*.*";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(customFolderModsText);
            groupBox2.Controls.Add(customModsLocationButton);
            groupBox2.Controls.Add(gameFolderModsText);
            groupBox2.Controls.Add(documentsModsText);
            groupBox2.Controls.Add(gameFolderModsButton);
            groupBox2.Controls.Add(documentsButton);
            groupBox2.Location = new Point(12, 130);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(541, 162);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Select Mod Folder";
            groupBox2.Visible = false;
            // 
            // customFolderModsText
            // 
            customFolderModsText.AutoSize = true;
            customFolderModsText.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            customFolderModsText.ImeMode = ImeMode.NoControl;
            customFolderModsText.Location = new Point(34, 135);
            customFolderModsText.Name = "customFolderModsText";
            customFolderModsText.Size = new Size(175, 15);
            customFolderModsText.TabIndex = 5;
            customFolderModsText.Text = "Locate the Jalopy folder first...";
            // 
            // customModsLocationButton
            // 
            customModsLocationButton.AutoSize = true;
            customModsLocationButton.Enabled = false;
            customModsLocationButton.ImeMode = ImeMode.NoControl;
            customModsLocationButton.Location = new Point(6, 114);
            customModsLocationButton.Name = "customModsLocationButton";
            customModsLocationButton.Size = new Size(67, 19);
            customModsLocationButton.TabIndex = 4;
            customModsLocationButton.Text = "Custom";
            customModsLocationButton.UseVisualStyleBackColor = true;
            customModsLocationButton.CheckedChanged += customModsLocationButton_CheckedChanged;
            // 
            // gameFolderModsText
            // 
            gameFolderModsText.AutoSize = true;
            gameFolderModsText.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            gameFolderModsText.ImeMode = ImeMode.NoControl;
            gameFolderModsText.Location = new Point(26, 90);
            gameFolderModsText.Name = "gameFolderModsText";
            gameFolderModsText.Size = new Size(175, 15);
            gameFolderModsText.TabIndex = 3;
            gameFolderModsText.Text = "Locate the Jalopy folder first...";
            // 
            // documentsModsText
            // 
            documentsModsText.AutoSize = true;
            documentsModsText.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            documentsModsText.ImeMode = ImeMode.NoControl;
            documentsModsText.Location = new Point(26, 44);
            documentsModsText.Name = "documentsModsText";
            documentsModsText.Size = new Size(175, 15);
            documentsModsText.TabIndex = 2;
            documentsModsText.Text = "Locate the Jalopy folder first...";
            // 
            // gameFolderModsButton
            // 
            gameFolderModsButton.AutoSize = true;
            gameFolderModsButton.Enabled = false;
            gameFolderModsButton.ImeMode = ImeMode.NoControl;
            gameFolderModsButton.Location = new Point(6, 68);
            gameFolderModsButton.Name = "gameFolderModsButton";
            gameFolderModsButton.Size = new Size(128, 19);
            gameFolderModsButton.TabIndex = 1;
            gameFolderModsButton.Text = "Jalopy Game Folder";
            gameFolderModsButton.UseVisualStyleBackColor = true;
            gameFolderModsButton.CheckedChanged += gameFolderModsButton_CheckedChanged;
            // 
            // documentsButton
            // 
            documentsButton.AutoSize = true;
            documentsButton.Checked = true;
            documentsButton.Enabled = false;
            documentsButton.ForeColor = SystemColors.ControlText;
            documentsButton.ImeMode = ImeMode.NoControl;
            documentsButton.Location = new Point(6, 22);
            documentsButton.Name = "documentsButton";
            documentsButton.Size = new Size(86, 19);
            documentsButton.TabIndex = 0;
            documentsButton.TabStop = true;
            documentsButton.Text = "Documents";
            documentsButton.UseVisualStyleBackColor = true;
            documentsButton.CheckedChanged += documentsButton_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(565, 300);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            ForeColor = SystemColors.ControlText;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "JaPatcher";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label installedTextValue;
        private Label folderTextValue;
        private Button locateFolderButton;
        private Button installButton;
        private OpenFileDialog locateFolderDialog;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label gameFolderModsText;
        private Label documentsModsText;
        private RadioButton gameFolderModsButton;
        private RadioButton documentsButton;
        private Button launchButton;
        private Button updateButton;
        private Label customFolderModsText;
        private RadioButton customModsLocationButton;
        private FolderBrowserDialog customModsDialog;
    }
}