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
            GroupBox groupBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            Label installedText;
            Label gameFolderText;
            GroupBox ModsGroupBox;
            installButton = new Button();
            uninstallButton = new Button();
            locateFolderButton = new Button();
            installedTextValue = new Label();
            folderTextValue = new Label();
            gameFolderModsText = new Label();
            documentsModsText = new Label();
            gameFolderModsButton = new RadioButton();
            documentsButton = new RadioButton();
            tabControl1 = new TabControl();
            modsTab = new TabPage();
            locateFolderDialog = new OpenFileDialog();
            groupBox1 = new GroupBox();
            installedText = new Label();
            gameFolderText = new Label();
            ModsGroupBox = new GroupBox();
            groupBox1.SuspendLayout();
            ModsGroupBox.SuspendLayout();
            tabControl1.SuspendLayout();
            modsTab.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(installButton);
            groupBox1.Controls.Add(uninstallButton);
            groupBox1.Controls.Add(locateFolderButton);
            groupBox1.Controls.Add(installedTextValue);
            groupBox1.Controls.Add(folderTextValue);
            groupBox1.Controls.Add(installedText);
            groupBox1.Controls.Add(gameFolderText);
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Name = "groupBox1";
            groupBox1.TabStop = false;
            // 
            // installButton
            // 
            resources.ApplyResources(installButton, "installButton");
            installButton.Name = "installButton";
            installButton.UseVisualStyleBackColor = true;
            installButton.Click += installButton_Click;
            // 
            // uninstallButton
            // 
            resources.ApplyResources(uninstallButton, "uninstallButton");
            uninstallButton.Name = "uninstallButton";
            uninstallButton.UseVisualStyleBackColor = true;
            uninstallButton.Click += uninstallButton_Click;
            // 
            // locateFolderButton
            // 
            resources.ApplyResources(locateFolderButton, "locateFolderButton");
            locateFolderButton.Name = "locateFolderButton";
            locateFolderButton.UseVisualStyleBackColor = true;
            locateFolderButton.Click += locateFolderButton_Click;
            // 
            // installedTextValue
            // 
            resources.ApplyResources(installedTextValue, "installedTextValue");
            installedTextValue.Name = "installedTextValue";
            installedTextValue.Click += label1_Click;
            // 
            // folderTextValue
            // 
            resources.ApplyResources(folderTextValue, "folderTextValue");
            folderTextValue.Name = "folderTextValue";
            folderTextValue.Click += folderTextValue_Click;
            // 
            // installedText
            // 
            resources.ApplyResources(installedText, "installedText");
            installedText.Name = "installedText";
            installedText.Click += label1_Click;
            // 
            // gameFolderText
            // 
            resources.ApplyResources(gameFolderText, "gameFolderText");
            gameFolderText.Name = "gameFolderText";
            gameFolderText.Click += gameFolderText_Click;
            // 
            // ModsGroupBox
            // 
            ModsGroupBox.Controls.Add(gameFolderModsText);
            ModsGroupBox.Controls.Add(documentsModsText);
            ModsGroupBox.Controls.Add(gameFolderModsButton);
            ModsGroupBox.Controls.Add(documentsButton);
            resources.ApplyResources(ModsGroupBox, "ModsGroupBox");
            ModsGroupBox.Name = "ModsGroupBox";
            ModsGroupBox.TabStop = false;
            // 
            // gameFolderModsText
            // 
            resources.ApplyResources(gameFolderModsText, "gameFolderModsText");
            gameFolderModsText.Name = "gameFolderModsText";
            // 
            // documentsModsText
            // 
            resources.ApplyResources(documentsModsText, "documentsModsText");
            documentsModsText.Name = "documentsModsText";
            documentsModsText.Click += documentsModsText_Click;
            // 
            // gameFolderModsButton
            // 
            resources.ApplyResources(gameFolderModsButton, "gameFolderModsButton");
            gameFolderModsButton.Name = "gameFolderModsButton";
            gameFolderModsButton.UseVisualStyleBackColor = true;
            gameFolderModsButton.CheckedChanged += gameFolderModsButton_CheckedChanged;
            // 
            // documentsButton
            // 
            resources.ApplyResources(documentsButton, "documentsButton");
            documentsButton.Checked = true;
            documentsButton.Name = "documentsButton";
            documentsButton.TabStop = true;
            documentsButton.UseVisualStyleBackColor = true;
            documentsButton.CheckedChanged += documentsButton_CheckedChanged;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(modsTab);
            resources.ApplyResources(tabControl1, "tabControl1");
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            // 
            // modsTab
            // 
            modsTab.Controls.Add(ModsGroupBox);
            resources.ApplyResources(modsTab, "modsTab");
            modsTab.Name = "modsTab";
            modsTab.UseVisualStyleBackColor = true;
            modsTab.Click += modsTab_Click;
            // 
            // locateFolderDialog
            // 
            locateFolderDialog.FileName = "Jalopy.exe";
            resources.ApplyResources(locateFolderDialog, "locateFolderDialog");
            locateFolderDialog.FileOk += locateFolderDialog_FileOk;
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabControl1);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ModsGroupBox.ResumeLayout(false);
            ModsGroupBox.PerformLayout();
            tabControl1.ResumeLayout(false);
            modsTab.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Label installedTextValue;
        private Label folderTextValue;
        private Button locateFolderButton;
        private Button uninstallButton;
        private Button installButton;
        private TabControl tabControl1;
        private TabPage modsTab;
        private OpenFileDialog locateFolderDialog;
        private RadioButton gameFolderModsButton;
        private RadioButton documentsButton;
        private Label documentsModsText;
        private Label gameFolderModsText;
    }
}