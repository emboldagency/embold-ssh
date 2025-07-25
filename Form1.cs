using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSHHandlerApp
{
    public partial class Form1 : Form
    {
        // These controls are initialized in Form1.Designer.cs
        private ComboBox comboTerminal = null!;
        private ComboBox comboIcon = null!;
        private Button btnInstall = null!;
        private Button btnUninstall = null!;
        private PictureBox pictureIcon = null!;
        private LinkLabel linkUpdate = null!;

        private readonly string _installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "embold-ssh");
        private readonly string _installedAppPath;

        public Form1(string[] args)
        {
            _installedAppPath = Path.Combine(_installDir, "SSHHandlerApp.exe");

            InitializeComponent();

            // Set up event handlers
            btnInstall.Click += BtnInstall_Click;
            btnUninstall.Click += BtnUninstall_Click;
            comboTerminal.SelectedIndexChanged += ComboTerminal_SelectedIndexChanged;
            comboIcon.SelectedIndexChanged += ComboIcon_SelectedIndexChanged;
            comboIcon.DrawItem += ComboIcon_DrawItem;

            // Initialize Icons and UI state
            ExtractDefaultIcons();
            string iconsDir = Path.Combine(_installDir, "icons");
            if (Directory.Exists(iconsDir))
            {
                foreach (var iconFile in Directory.GetFiles(iconsDir, "*.ico"))
                {
                    comboIcon.Items.Add(new ComboBoxItem { Display = Path.GetFileName(iconFile), Value = iconFile });
                }
            }
            comboIcon.Items.Add(new ComboBoxItem { Display = "Custom...", Value = "Custom..." });

            // Set initial icon based on the default terminal
            ComboTerminal_SelectedIndexChanged(this, EventArgs.Empty);

            LoadConfig();
            CheckForUpdate();

            // Handle command-line arguments for post-elevation tasks
            if (args.Length > 0)
            {
                if (args.Any(a => a.Equals("--install", StringComparison.OrdinalIgnoreCase)))
                {
                    PerformInstall();
                    Application.Exit();
                }
                else if (args.Any(a => a.Equals("--uninstall", StringComparison.OrdinalIgnoreCase)))
                {
                    PerformUninstall();
                }
            }
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(_installDir, "config.json");
            if (!File.Exists(configPath)) return;

            try
            {
                var configJson = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(configJson);
                if (doc.RootElement.TryGetProperty("command", out var commandProp))
                {
                    string savedCommand = commandProp.GetString() ?? "wt.exe";
                    comboTerminal.SelectedItem = GetTerminalDisplayName(savedCommand);
                }
            }
            catch { /* Ignore config load errors */ }
        }

        private string GetTerminalDisplayName(string commandPath)
        {
            if (commandPath.EndsWith("wt.exe", StringComparison.OrdinalIgnoreCase)) return "Windows Terminal";
            if (commandPath.EndsWith("cmd.exe", StringComparison.OrdinalIgnoreCase)) return "Command Prompt";
            if (commandPath.EndsWith("wsl.exe", StringComparison.OrdinalIgnoreCase)) return "Ubuntu";
            return commandPath;
        }

        private void BtnInstall_Click(object? sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                RelaunchAsAdmin("--install");
                return;
            }
            PerformInstall();
        }

        private void PerformInstall()
        {
            try
            {
                // Use Process.GetCurrentProcess().MainModule.FileName to reliably get the .exe path
                string? currentAppPath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(currentAppPath))
                {
                    MessageBox.Show("Could not determine the application path.", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Prevent installation when running from a DLL (e.g., via 'dotnet run')
                if (Path.GetExtension(currentAppPath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "Installation must be run from the published SSHHandlerApp.exe, not from the development environment (dotnet run).\n\nPlease build the application for release and run the .exe directly.",
                        "Installation Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Directory.CreateDirectory(_installDir);
                File.Copy(currentAppPath, _installedAppPath, true);

                string terminalPath = comboTerminal.SelectedItem?.ToString() switch
                {
                    "Windows Terminal" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "wt.exe"),
                    "Command Prompt" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe"),
                    "Ubuntu" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "wsl.exe"),
                    _ => comboTerminal.SelectedItem?.ToString() ?? "wt.exe"
                };

                var selectedItem = comboIcon.SelectedItem as ComboBoxItem;
                string? iconSource = selectedItem?.Value;
                string iconPath = Path.Combine(_installDir, "terminal.ico");
                if (!string.IsNullOrWhiteSpace(iconSource) && File.Exists(iconSource))
                {
                    File.Copy(iconSource, iconPath, true);
                }

                var config = new { command = terminalPath, icon = "terminal.ico" };
                File.WriteAllText(Path.Combine(_installDir, "config.json"), JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

                string regCommand = $"\"{_installedAppPath}\" \"%1\"";

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Embold.SSH"))
                {
                    key.SetValue("", "URL:SSH Protocol");
                    key.SetValue("URL Protocol", "");
                    using (var iconKey = key.CreateSubKey("DefaultIcon")) { iconKey.SetValue("", iconPath); }
                    using (var appKey = key.CreateSubKey("Application"))
                    {
                        appKey.SetValue("ApplicationName", "Embold SSH Handler");
                        appKey.SetValue("ApplicationDescription", "Open ssh:// URLs in your preferred terminal");
                        appKey.SetValue("ApplicationCompany", "Embold");
                        appKey.SetValue("ApplicationIcon", iconPath);
                    }
                    using (var capKey = key.CreateSubKey(@"Capabilities\UrlAssociations")) { capKey.SetValue("ssh", "Embold.SSH"); }
                    using (var shellKey = key.CreateSubKey(@"shell\open\command")) { shellKey.SetValue("", regCommand); }
                }

                MessageBox.Show("SSH handler settings applied successfully for the current user.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during install: {ex.Message}\n\nMake sure you are running as an administrator.", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUninstall_Click(object? sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                RelaunchAsAdmin("--uninstall");
                return;
            }
            PerformUninstall();
        }

        private void PerformUninstall()
        {
            try
            {
                // First, remove registry entries. This is safe to do.
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Embold.SSH", false);

                // Create a temporary batch file to delete the application files after this process exits.
                string tempBatchFile = Path.Combine(Path.GetTempPath(), "embold-ssh-uninstall.bat");
                string batchContent = $@"
@echo off
echo Waiting for SSH Handler to close...
timeout /t 2 /nobreak > nul
echo Deleting installation files...
rmdir /s /q ""{_installDir}""
echo Cleanup complete. Deleting self...
del ""%~f0""
";
                File.WriteAllText(tempBatchFile, batchContent);

                // Launch the batch file in a new, hidden window.
                var startInfo = new ProcessStartInfo(tempBatchFile)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(startInfo);

                MessageBox.Show("SSH handler uninstalled successfully. Cleanup will complete in the background.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Close the application so the batch file can delete it.
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during uninstall: {ex.Message}", "Uninstall Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void CheckForUpdate()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EmboldSshHandler", "1.1"));
                var response = await client.GetStringAsync("https://api.github.com/repos/emboldagency/embold-ssh/releases/latest");

                using var doc = JsonDocument.Parse(response);
                var tagName = doc.RootElement.GetProperty("tag_name").GetString()?.TrimStart('v');
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);

                if (Version.TryParse(tagName, out var latestVersion) && Version.TryParse(currentVersion, out var appVersion))
                {
                    if (latestVersion > appVersion)
                    {
                        linkUpdate.Text = $"Update available: v{latestVersion}";
                        linkUpdate.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo(doc.RootElement.GetProperty("html_url").GetString() ?? "") { UseShellExecute = true });
                    }
                    else
                    {
                        linkUpdate.Text = "You are up to date.";
                    }
                }
            }
            catch
            {
                linkUpdate.Text = "Update check failed.";
            }
        }

        public static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void RelaunchAsAdmin(string argument)
        {
            var host = Process.GetCurrentProcess().MainModule?.FileName;
            if (host == null)
            {
                MessageBox.Show("Could not determine the application's host process.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fileName;
            string arguments;

            if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            {
                fileName = host;
                arguments = $"run -- {argument}";
            }
            else
            {
                fileName = host;
                arguments = argument;
            }

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = fileName,
                Verb = "runas",
                Arguments = arguments
            };

            try
            {
                Process.Start(startInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to relaunch with admin privileges: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ComboBoxItem { public string Display { get; set; } = string.Empty; public string Value { get; set; } = string.Empty; public override string ToString() => Display; }
        private void ComboIcon_DrawItem(object? sender, DrawItemEventArgs e) { if (e.Index < 0) return; var cb = sender as ComboBox; var item = cb?.Items[e.Index] as ComboBoxItem; string text = item?.Display ?? cb?.Items[e.Index]?.ToString() ?? string.Empty; e.DrawBackground(); using (var brush = new SolidBrush(e.ForeColor)) { e.Graphics.DrawString(text, e.Font ?? SystemFonts.DefaultFont, brush, e.Bounds); } e.DrawFocusRectangle(); }
        private void ComboTerminal_SelectedIndexChanged(object? sender, EventArgs e) { if (comboTerminal.SelectedItem?.ToString() == "Custom...") { using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*" }) { if (ofd.ShowDialog() == DialogResult.OK) { if (!comboTerminal.Items.Contains(ofd.FileName)) comboTerminal.Items.Insert(comboTerminal.Items.Count - 1, ofd.FileName); comboTerminal.SelectedItem = ofd.FileName; } else { comboTerminal.SelectedIndex = 0; } } } string? selectedTerminal = comboTerminal.SelectedItem?.ToString(); string? matchIcon = "embold.ico"; if (selectedTerminal == "Windows Terminal") matchIcon = "wt.ico"; else if (selectedTerminal == "Command Prompt") matchIcon = "cmd.ico"; else if (selectedTerminal == "Ubuntu") matchIcon = "ubuntu.ico"; if (!string.IsNullOrEmpty(matchIcon)) { for (int i = 0; i < comboIcon.Items.Count; i++) { if (comboIcon.Items[i] is ComboBoxItem cbi && cbi.Display.Equals(matchIcon, StringComparison.OrdinalIgnoreCase)) { comboIcon.SelectedIndex = i; break; } } } }
        private void ExtractDefaultIcons() { string[] iconNames = { "embold.ico", "wt.ico", "cmd.ico", "ubuntu.ico" }; string iconsDir = Path.Combine(_installDir, "icons"); Directory.CreateDirectory(iconsDir); foreach (var iconName in iconNames) { string outPath = Path.Combine(iconsDir, iconName); if (!File.Exists(outPath)) { string resourceName = $"SSHHandlerApp.DefaultIcons.{iconName}"; using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) { if (stream != null) { using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write)) { stream.CopyTo(fs); } } } } } }
        private void ComboIcon_SelectedIndexChanged(object? sender, EventArgs e) { var selectedItem = comboIcon.SelectedItem as ComboBoxItem; string? selectedIcon = selectedItem?.Value; if (selectedIcon == "Custom...") { using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Icon files (*.ico)|*.ico|All files (*.*)|*.*" }) { if (ofd.ShowDialog() == DialogResult.OK) { var newItem = new ComboBoxItem { Display = Path.GetFileName(ofd.FileName), Value = ofd.FileName }; bool exists = false; foreach (var item in comboIcon.Items) { if (item is ComboBoxItem cbi && cbi.Value == ofd.FileName) { exists = true; break; } } if (!exists) comboIcon.Items.Insert(comboIcon.Items.Count - 1, newItem); comboIcon.SelectedItem = newItem; selectedIcon = ofd.FileName; } else { if (comboIcon.Items.Count > 0) comboIcon.SelectedIndex = 0; selectedIcon = (comboIcon.Items[0] as ComboBoxItem)?.Value; } } } if (!string.IsNullOrWhiteSpace(selectedIcon) && File.Exists(selectedIcon)) { try { if (pictureIcon.Image != null) { var oldImg = pictureIcon.Image; pictureIcon.Image = null; oldImg.Dispose(); } using (var icon = new Icon(selectedIcon, 32, 32)) { pictureIcon.Image = icon.ToBitmap(); } } catch { if (pictureIcon.Image != null) { var oldImg = pictureIcon.Image; pictureIcon.Image = null; oldImg.Dispose(); } } } else { if (pictureIcon.Image != null) { var oldImg = pictureIcon.Image; pictureIcon.Image = null; oldImg.Dispose(); } } }
    }

    public static class UrlHandler
    {
        public static void HandleSshUrl(string sshUrl)
        {
            try
            {
                string parsedUrl = sshUrl.Trim().Replace("ssh://", "").TrimEnd('/');
                string[] urlParts = parsedUrl.Split(':');
                string sshHost = urlParts[0];
                string? sshPort = urlParts.Length > 1 ? urlParts[1] : null;

                string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "embold-ssh", "config.json");
                string terminalCommand = "wt.exe";
                if (File.Exists(configPath))
                {
                    var configJson = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(configJson);
                    if (doc.RootElement.TryGetProperty("command", out var commandProp))
                    {
                        terminalCommand = commandProp.GetString() ?? "wt.exe";
                    }
                }

                string sshCommand = sshPort != null ? $"ssh -p {sshPort} {sshHost}" : $"ssh {sshHost}";

                var startInfo = new ProcessStartInfo(terminalCommand)
                {
                    UseShellExecute = true,
                    Arguments = terminalCommand.Contains("cmd.exe") ? $"/k {sshCommand}" : sshCommand
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "embold-ssh");
                Directory.CreateDirectory(logDir);
                File.AppendAllText(Path.Combine(logDir, "error.log"), $"{DateTime.Now}: {ex.Message}\n");
            }
        }
    }
}
