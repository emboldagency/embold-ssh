namespace SSHHandlerApp
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
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 200);
            this.Text = "emBold SSH Protocol Handler";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Labels
            var labelTerminal = new System.Windows.Forms.Label { Text = "Terminal:", Location = new System.Drawing.Point(20, 23), AutoSize = true };
            var labelIcon = new System.Windows.Forms.Label { Text = "Icon:", Location = new System.Drawing.Point(20, 63), AutoSize = true };
            this.Controls.Add(labelTerminal);
            this.Controls.Add(labelIcon);

            // Terminal ComboBox
            this.comboTerminal = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(150, 20), Size = new System.Drawing.Size(250, 23), DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            this.comboTerminal.Items.AddRange(new object[] { "Windows Terminal", "Command Prompt", "Ubuntu", "Custom..." });
            this.comboTerminal.SelectedItem = "Windows Terminal";
            this.Controls.Add(this.comboTerminal);

            // Icon ComboBox
            this.comboIcon = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(150, 60), Size = new System.Drawing.Size(180, 23), DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList, DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed };
            this.Controls.Add(this.comboIcon);

            // Icon PictureBox
            this.pictureIcon = new System.Windows.Forms.PictureBox { Location = new System.Drawing.Point(340, 60), Size = new System.Drawing.Size(32, 32), SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom, BorderStyle = System.Windows.Forms.BorderStyle.None };
            this.Controls.Add(this.pictureIcon);

            // Install/Uninstall Buttons
            this.btnInstall = new System.Windows.Forms.Button { Text = "Apply", Location = new System.Drawing.Point(150, 120), Size = new System.Drawing.Size(120, 30) };
            this.Controls.Add(this.btnInstall);
            this.btnUninstall = new System.Windows.Forms.Button { Text = "Uninstall", Location = new System.Drawing.Point(280, 120), Size = new System.Drawing.Size(120, 30) };
            this.Controls.Add(this.btnUninstall);

            // Version and Update Link
            var versionLabel = new System.Windows.Forms.Label { Text = $"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}", AutoSize = true };
            versionLabel.Location = new System.Drawing.Point(10, this.ClientSize.Height - versionLabel.Height - 10);
            this.Controls.Add(versionLabel);

            this.linkUpdate = new System.Windows.Forms.LinkLabel { Text = "Checking for updates...", AutoSize = true, LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline };
            this.linkUpdate.Location = new System.Drawing.Point(this.ClientSize.Width - this.linkUpdate.Width - 110, this.ClientSize.Height - this.linkUpdate.Height - 10);
            this.linkUpdate.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.Controls.Add(this.linkUpdate);
        }

        #endregion
    }
}
