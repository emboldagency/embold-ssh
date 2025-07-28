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

            // Form-level styling
            this.ClientSize = new System.Drawing.Size(480, 200); // Reduced height since no icon selection
            this.Text = "emBold SSH Protocol Handler";
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.BackColor = System.Drawing.Color.White; // Consistent white background
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Labels
            var labelTerminal = new System.Windows.Forms.Label { Text = "Terminal:", Location = new System.Drawing.Point(30, 38), AutoSize = true };
            this.labelWtProfile = new System.Windows.Forms.Label { Text = "WT Profile:", Location = new System.Drawing.Point(30, 78), AutoSize = true, Visible = false };
            this.Controls.Add(labelTerminal);
            this.Controls.Add(this.labelWtProfile);

            // Terminal ComboBox
            this.comboTerminal = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(130, 35), Size = new System.Drawing.Size(280, 23), DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            this.comboTerminal.Items.AddRange(new object[] { "Windows Terminal", "Command Prompt", "Ubuntu", "Custom..." });
            this.comboTerminal.SelectedItem = "Windows Terminal";
            this.Controls.Add(this.comboTerminal);

            // Windows Terminal Profile ComboBox
            this.comboWtProfile = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(130, 75), Size = new System.Drawing.Size(280, 23), DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList, Visible = false };
            this.Controls.Add(this.comboWtProfile);

            // Apply Button (Primary Action)
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnInstall.Text = "Apply";
            this.btnInstall.Location = new System.Drawing.Point(130, 125); // Moved up since no icon controls
            this.btnInstall.Size = new System.Drawing.Size(120, 35);
            this.btnInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstall.FlatAppearance.BorderSize = 0;
            this.btnInstall.BackColor = System.Drawing.Color.FromArgb(0, 122, 204); // Accent blue
            this.btnInstall.ForeColor = System.Drawing.Color.White;
            this.btnInstall.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.Controls.Add(this.btnInstall);

            // Uninstall Button (Secondary Action)
            this.btnUninstall = new System.Windows.Forms.Button();
            this.btnUninstall.Text = "Clear";
            this.btnUninstall.Location = new System.Drawing.Point(260, 125); // Moved up since no icon controls
            this.btnUninstall.Size = new System.Drawing.Size(120, 35);
            this.btnUninstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUninstall.FlatAppearance.BorderSize = 1;
            this.btnUninstall.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.btnUninstall.BackColor = System.Drawing.Color.White;
            this.btnUninstall.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            this.Controls.Add(this.btnUninstall);

            // Version and Update Link
            var versionLabel = new System.Windows.Forms.Label { Text = $"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}", AutoSize = true };
            versionLabel.Location = new System.Drawing.Point(20, this.ClientSize.Height - versionLabel.Height - 15);
            versionLabel.ForeColor = System.Drawing.Color.Gray;
            this.Controls.Add(versionLabel);

            this.linkUpdate = new System.Windows.Forms.LinkLabel { Text = "Checking for updates...", AutoSize = true, LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline };
            this.linkUpdate.Location = new System.Drawing.Point(this.ClientSize.Width - this.linkUpdate.Width - 110, this.ClientSize.Height - this.linkUpdate.Height - 15);
            this.linkUpdate.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.linkUpdate.LinkColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.Controls.Add(this.linkUpdate);
        }

        #endregion
    }
}
