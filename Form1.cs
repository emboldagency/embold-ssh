using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SSHHandlerApp
{

    public partial class Form1 : Form
    {
        // UI controls
    private ComboBox comboTerminal = null!;
    private ComboBox comboIcon = null!;
    private Button btnInstall = null!;
    private Button btnUninstall = null!;
    private PictureBox pictureIcon = null!;

        public Form1()
        {
            // Check if launched with SSH URL argument
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1].StartsWith("ssh://"))
            {
                HandleSSHUrl(args[1]);
                return; // Exit after handling URL
            }

            InitializeComponent();        // Populate terminal options
        comboTerminal = new ComboBox();
        comboTerminal.Location = new System.Drawing.Point(150, 20);
        // Add version label at the bottom right
        var versionLabel = new Label();
        versionLabel.Text = $"Version: {Application.ProductVersion}";
        versionLabel.AutoSize = true;
        versionLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        versionLabel.Location = new System.Drawing.Point(this.ClientSize.Width - 160, this.ClientSize.Height - 30);
        versionLabel.TextAlign = System.Drawing.ContentAlignment.BottomRight;
        versionLabel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.Controls.Add(versionLabel);
        // Ensure label stays at the bottom right on resize
        this.Resize += (s, e) => {
            versionLabel.Location = new System.Drawing.Point(this.ClientSize.Width - versionLabel.Width - 10, this.ClientSize.Height - versionLabel.Height - 10);
        };
        // Set initial position
        versionLabel.Location = new System.Drawing.Point(this.ClientSize.Width - versionLabel.Width - 10, this.ClientSize.Height - versionLabel.Height - 10);
        comboTerminal.Size = new System.Drawing.Size(250, 23);
        comboTerminal.DropDownStyle = ComboBoxStyle.DropDownList;
        comboTerminal.Items.Add("Windows Terminal");
        comboTerminal.Items.Add("Command Prompt");
        comboTerminal.Items.Add("Ubuntu");
        comboTerminal.Items.Add("Custom...");
        comboTerminal.SelectedIndexChanged += ComboTerminal_SelectedIndexChanged;
        this.Controls.Add(comboTerminal);

        // Populate icon options
        comboIcon = new ComboBox();
        comboIcon.Location = new System.Drawing.Point(150, 60);
        // Ensure default icons are available in the user's icons directory
        ExtractDefaultIcons();
        comboIcon.Size = new System.Drawing.Size(180, 23); // Reduced width for icon names
        comboIcon.DropDownStyle = ComboBoxStyle.DropDownList;
        string iconsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "embold-ssh", "icons");
        if (Directory.Exists(iconsDir))
        {
            foreach (var iconFile in Directory.GetFiles(iconsDir, "*.ico"))
            {
                comboIcon.Items.Add(new ComboBoxItem { Display = Path.GetFileName(iconFile), Value = iconFile });
            }
        }
        comboIcon.Items.Add(new ComboBoxItem { Display = "Custom...", Value = "Custom..." });
        comboIcon.SelectedIndexChanged += ComboIcon_SelectedIndexChanged;
        comboIcon.DrawMode = DrawMode.OwnerDrawFixed;
        comboIcon.DrawItem += ComboIcon_DrawItem;
        this.Controls.Add(comboIcon);

        // Picture box for icon preview
        pictureIcon = new PictureBox();
        pictureIcon.Location = new System.Drawing.Point(410, 60);
        pictureIcon.Size = new System.Drawing.Size(32, 32);
        pictureIcon.SizeMode = PictureBoxSizeMode.Zoom;
        this.Controls.Add(pictureIcon);

        // Set defaults
        if (comboTerminal.Items.Contains("Windows Terminal"))
            comboTerminal.SelectedItem = "Windows Terminal";

        // Set initial icon to match preselected terminal
        string? initialMatchIcon = null;
        if (comboTerminal.SelectedItem?.ToString() == "Windows Terminal")
            initialMatchIcon = "wt.ico";
        else if (comboTerminal.SelectedItem?.ToString() == "Command Prompt")
            initialMatchIcon = "cmd.ico";
        else if (comboTerminal.SelectedItem?.ToString() == "Ubuntu")
            initialMatchIcon = "ubuntu.ico";

        bool foundInitial = false;
        if (!string.IsNullOrEmpty(initialMatchIcon))
        {
            for (int i = 0; i < comboIcon.Items.Count; i++)
            {
                if (comboIcon.Items[i] is ComboBoxItem cbi && cbi.Display.Equals(initialMatchIcon, StringComparison.OrdinalIgnoreCase))
                {
                    comboIcon.SelectedIndex = i;
                    foundInitial = true;
                    break;
                }
            }
        }
        if (!foundInitial && comboIcon.Items.Count > 0)
            comboIcon.SelectedIndex = 0;

        // Show default icon
        if (comboIcon.Items.Count > 0)
            ComboIcon_SelectedIndexChanged(this, EventArgs.Empty);
    }

    // Helper class for ComboBox items
    private class ComboBoxItem
    {
        public string Display { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public override string ToString() => Display;
    }

    // Custom draw to show only filename
    private void ComboIcon_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        var cb = sender as ComboBox;
        var item = cb?.Items[e.Index] as ComboBoxItem;
        string text = item?.Display ?? cb?.Items[e.Index]?.ToString() ?? string.Empty;
        e.DrawBackground();
        using (var brush = new System.Drawing.SolidBrush(e.ForeColor))
        {
            e.Graphics.DrawString(text, e.Font ?? SystemFonts.DefaultFont, brush, e.Bounds);
        }
        e.DrawFocusRectangle();
    }

    private void ComboTerminal_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (comboTerminal.SelectedItem?.ToString() == "Custom...")
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (!comboTerminal.Items.Contains(ofd.FileName))
                        comboTerminal.Items.Insert(comboTerminal.Items.Count - 1, ofd.FileName);
                    comboTerminal.SelectedItem = ofd.FileName;
                }
                else
                {
                    // Revert to previous selection if cancelled
                    comboTerminal.SelectedIndex = 0;
                }
            }
        }

        // Auto-select matching icon for known terminals
        string? selectedTerminal = comboTerminal.SelectedItem?.ToString();
        string? matchIcon = null;
        if (selectedTerminal == "Windows Terminal")
            matchIcon = "wt.ico";
        else if (selectedTerminal == "Command Prompt")
            matchIcon = "cmd.ico";
        else if (selectedTerminal == "Ubuntu")
            matchIcon = "ubuntu.ico";

        if (!string.IsNullOrEmpty(matchIcon))
        {
            for (int i = 0; i < comboIcon.Items.Count; i++)
            {
                if (comboIcon.Items[i] is ComboBoxItem cbi && cbi.Display.Equals(matchIcon, StringComparison.OrdinalIgnoreCase))
                {
                    comboIcon.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Extracts embedded default icons to the user's icons directory if not already present.
    /// </summary>
    private void ExtractDefaultIcons()
    {
        string[] iconNames = { "wt.ico", "cmd.ico", "ubuntu.ico" };
        string iconsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "embold-ssh", "icons");
        Directory.CreateDirectory(iconsDir);

        foreach (var iconName in iconNames)
        {
            string outPath = Path.Combine(iconsDir, iconName);
            if (!File.Exists(outPath))
            {
                // Resource name: [DefaultNamespace].[Folder].[FileName]
                // e.g., SSHHandlerApp.DefaultIcons.wt.ico
                string resourceName = $"SSHHandlerApp.DefaultIcons.{iconName}";
                using (var stream = typeof(Form1).Assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fs);
                        }
                    }
                }
            }
        }
    }

private void ComboIcon_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var selectedItem = comboIcon.SelectedItem as ComboBoxItem;
        string? selectedIcon = selectedItem?.Value;
        if (selectedIcon == "Custom...")
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Icon files (*.ico)|*.ico|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var newItem = new ComboBoxItem { Display = Path.GetFileName(ofd.FileName), Value = ofd.FileName };
                    bool exists = false;
                    foreach (var item in comboIcon.Items)
                    {
                        if (item is ComboBoxItem cbi && cbi.Value == ofd.FileName)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                        comboIcon.Items.Insert(comboIcon.Items.Count - 1, newItem);
                    comboIcon.SelectedItem = newItem;
                    selectedIcon = ofd.FileName;
                }
                else
                {
                    // Revert to previous selection if cancelled
                    comboIcon.SelectedIndex = 0;
                    selectedIcon = (comboIcon.Items[0] as ComboBoxItem)?.Value;
                }
            }
        }
        // Display the selected icon using Icon class for better .ico support
        if (!string.IsNullOrWhiteSpace(selectedIcon) && File.Exists(selectedIcon))
        {
            try
            {
                // Dispose previous image if any
                if (pictureIcon.Image != null)
                {
                    var oldImg = pictureIcon.Image;
                    pictureIcon.Image = null;
                    oldImg.Dispose();
                }
                using (var icon = new System.Drawing.Icon(selectedIcon, 32, 32))
                {
                    pictureIcon.Image = icon.ToBitmap();
                }
            }
            catch (Exception ex)
            {
                if (pictureIcon.Image != null)
                {
                    var oldImg = pictureIcon.Image;
                    pictureIcon.Image = null;
                    oldImg.Dispose();
                }
                MessageBox.Show($"Failed to load icon: {selectedIcon}\n{ex.Message}", "Icon Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            if (pictureIcon.Image != null)
            {
                var oldImg = pictureIcon.Image;
                pictureIcon.Image = null;
                oldImg.Dispose();
            }
        }
    }



    private void BtnInstall_Click(object sender, EventArgs e)
    {
        try
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string emboldDir = Path.Combine(localAppData, "embold-ssh");
            Directory.CreateDirectory(emboldDir);

            // Determine terminal path
            string terminalPath = comboTerminal.SelectedItem?.ToString() switch
            {
                "Windows Terminal" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "wt.exe"),
                "Command Prompt" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cmd.exe"),
                "Ubuntu" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "wsl.exe"),
                _ => comboTerminal.SelectedItem?.ToString() ?? "wt.exe"
            };

            // Copy icon
            var selectedItem = comboIcon.SelectedItem as ComboBoxItem;
            string? iconSource = selectedItem?.Value;
            if (!string.IsNullOrWhiteSpace(iconSource) && File.Exists(iconSource))
            {
                File.Copy(iconSource, Path.Combine(emboldDir, "terminal.ico"), true);
            }

            // Save config.json
            string configJson = "{\n  \"command\": \"" + terminalPath.Replace("\\", "\\\\") + "\",\n  \"icon\": \"terminal.ico\"\n}";
            File.WriteAllText(Path.Combine(emboldDir, "config.json"), configJson);

            // Write registry - direct to C# app instead of scripts
            // Use MainModule.FileName for single-file publish compatibility
            string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string regCommand = $"\"{appPath}\" \"%1\"";
            string iconPath = Path.Combine(emboldDir, "terminal.ico");

            using (var key = Registry.ClassesRoot.CreateSubKey("Embold.SSH"))
            {
                key.SetValue("", "URL:SSH Protocol");
                key.SetValue("URL Protocol", "");
                using (var iconKey = key.CreateSubKey("DefaultIcon"))
                {
                    iconKey.SetValue("", iconPath);
                }
                using (var appKey = key.CreateSubKey("Application"))
                {
                    appKey.SetValue("ApplicationName", "Embold SSH Handler");
                    appKey.SetValue("ApplicationDescription", "Open ssh:// URLs in your preferred terminal");
                    appKey.SetValue("ApplicationCompany", "Embold");
                    appKey.SetValue("ApplicationIcon", iconPath);
                }
                using (var capKey = key.CreateSubKey(@"Capabilities\UrlAssociations"))
                {
                    capKey.SetValue("ssh", "Embold.SSH");
                }
                using (var shellKey = key.CreateSubKey(@"shell\open\command"))
                {
                    shellKey.SetValue("", regCommand);
                }
            }
            using (var regApps = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\RegisteredApplications", true))
            {
                if (regApps != null)
                {
                    regApps.SetValue("Embold SSH Handler", "Software\\Classes\\Embold.SSH\\Capabilities");
                }
            }

            MessageBox.Show("SSH handler installed successfully.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during install: {ex.Message}");
        }
    }

        private void BtnUninstall_Click(object sender, EventArgs e)
    {
        try
        {
            // Remove registry
            Registry.ClassesRoot.DeleteSubKeyTree("Embold.SSH", false);
            using (var regApps = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\RegisteredApplications", true))
            {
                if (regApps != null)
                {
                    regApps.DeleteValue("Embold SSH Handler", false);
                }
            }

            // Remove files
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string emboldDir = Path.Combine(localAppData, "embold-ssh");
            if (Directory.Exists(emboldDir))
            {
                Directory.Delete(emboldDir, true);
            }

            MessageBox.Show("SSH handler uninstalled and cleaned up.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during uninstall: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle SSH URL when app is launched from protocol handler
    /// </summary>
    private void HandleSSHUrl(string sshUrl)
    {
        try
        {
            // Parse SSH URL (ssh://user@host:port)
            string parsedUrl = sshUrl.Replace("ssh://", "").Replace("/", "");
            string[] urlParts = parsedUrl.Split(':');
            string sshHost = urlParts[0];
            string? sshPort = urlParts.Length > 1 ? urlParts[1] : null;

            // Load terminal configuration
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "embold-ssh", "config.json");
            string terminalCommand = "wt.exe"; // default
            if (File.Exists(configPath))
            {
                try
                {
                    var configJson = File.ReadAllText(configPath);
                    using (var doc = System.Text.Json.JsonDocument.Parse(configJson))
                    {
                        if (doc.RootElement.TryGetProperty("command", out var commandProp))
                        {
                            terminalCommand = commandProp.GetString() ?? "wt.exe";
                        }
                    }
                }
                catch
                {
                    // Use default if config parsing fails
                }
            }

            // Build SSH command
            string sshCommand = sshPort != null ? $"ssh -p {sshPort} {sshHost}" : $"ssh {sshHost}";

            // Launch terminal with SSH command
            if (terminalCommand.Contains("cmd.exe"))
            {
                Process.Start(terminalCommand, $"/k {sshCommand}");
            }
            else
            {
                Process.Start(terminalCommand, sshCommand);
            }
        }
        catch (Exception ex)
        {
            // Log error or show message
            MessageBox.Show($"Error handling SSH URL: {ex.Message}", "SSH Handler Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Close the app after handling URL
            Application.Exit();
        }
    }
}
}
