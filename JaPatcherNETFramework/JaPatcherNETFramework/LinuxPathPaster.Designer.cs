namespace JaPatcherNETFramework
{
    partial class LinuxPathPaster
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
            this.PasteBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PasteBox
            // 
            this.PasteBox.Location = new System.Drawing.Point(24, 25);
            this.PasteBox.Name = "PasteBox";
            this.PasteBox.Size = new System.Drawing.Size(382, 20);
            this.PasteBox.TabIndex = 0;
            this.PasteBox.TextChanged += new System.EventHandler(this.PasteBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(345, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Please paste the path to where Jalopy is installed, excluding Jalopy.exe.";
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(15, 51);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(391, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // LinuxPathPaster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 82);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PasteBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LinuxPathPaster";
            this.Text = "JaPatcher - Linux Path Paster";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox PasteBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OKButton;
    }
}