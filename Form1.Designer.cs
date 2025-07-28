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
            this.ClientSize = new System.Drawing.Size(480, 240); // Increased height for custom header
            this.Text = "emBold SSH Protocol Handler";
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.BackColor = System.Drawing.Color.FromArgb(240, 240, 240); // Light gray background
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Custom header panel for better logo visibility
            var headerPanel = new System.Windows.Forms.Panel
            {
                Size = new System.Drawing.Size(480, 50),
                Location = new System.Drawing.Point(0, 0),
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48), // Dark background for logo visibility
                Dock = System.Windows.Forms.DockStyle.Top
            };

            // Header icon (loaded from embedded resource)
            var headerIcon = new System.Windows.Forms.PictureBox
            {
                Size = new System.Drawing.Size(32, 32),
                Location = new System.Drawing.Point(15, 9),
                SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
            };
            
            try
            {
                using var iconStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SSHHandlerApp.DefaultIcons.embold.ico");
                if (iconStream != null)
                {
                    headerIcon.Image = new System.Drawing.Icon(iconStream).ToBitmap();
                }
            }
            catch
            {
                // If icon loading fails, continue without header icon
            }
            headerPanel.Controls.Add(headerIcon);

            // Header title label
            var headerLabel = new System.Windows.Forms.Label
            {
                Text = "emBold SSH Protocol Handler",
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular),
                Location = new System.Drawing.Point(55, 15), // Moved right to make room for icon
                AutoSize = true
            };
            headerPanel.Controls.Add(headerLabel);
            this.Controls.Add(headerPanel);

            // Labels (moved down to account for header)
            var labelTerminal = new System.Windows.Forms.Label { Text = "Terminal:", Location = new System.Drawing.Point(30, 78), AutoSize = true };
            this.labelWtProfile = new System.Windows.Forms.Label { Text = "WT Profile:", Location = new System.Drawing.Point(30, 118), AutoSize = true, Visible = false };
            this.Controls.Add(labelTerminal);
            this.Controls.Add(this.labelWtProfile);

            // Terminal ComboBox
            this.comboTerminal = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(130, 75), Size = new System.Drawing.Size(280, 23), DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            this.comboTerminal.Items.AddRange(new object[] { "Windows Terminal", "Command Prompt", "Ubuntu", "Custom..." });
            this.comboTerminal.SelectedItem = "Windows Terminal";
            this.Controls.Add(this.comboTerminal);

            // Windows Terminal Profile ComboBox
            this.comboWtProfile = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(130, 115), Size = new System.Drawing.Size(280, 23), DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList, Visible = false };
            this.Controls.Add(this.comboWtProfile);

            // Apply Button (Primary Action)
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnInstall.Text = "Apply";
            this.btnInstall.Location = new System.Drawing.Point(130, 165); // Moved down for header
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
            this.btnUninstall.Location = new System.Drawing.Point(260, 165); // Moved down for header
            this.btnUninstall.Size = new System.Drawing.Size(120, 35);
            this.btnUninstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUninstall.FlatAppearance.BorderSize = 1;
            this.btnUninstall.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.btnUninstall.BackColor = System.Drawing.Color.White;
            this.btnUninstall.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            this.Controls.Add(this.btnUninstall);

            // Version and Update Link
            var versionLabel = new System.Windows.Forms.Label { Text = $"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}", AutoSize = true };
            versionLabel.Location = new System.Drawing.Point(20, 210);
            versionLabel.ForeColor = System.Drawing.Color.Gray;
            this.Controls.Add(versionLabel);

            this.linkUpdate = new System.Windows.Forms.LinkLabel { Text = "Checking for updates...", AutoSize = true, LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline };
            this.linkUpdate.Location = new System.Drawing.Point(280, 210);
            this.linkUpdate.LinkColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.Controls.Add(this.linkUpdate);
        }

        #endregion
    }
}
