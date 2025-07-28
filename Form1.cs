using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSHHandlerApp
{
    public partial class Form1 : Form
    {
        // These controls are initialized in Form1.Designer.cs
        private ComboBox comboTerminal = null!;
        private ComboBox comboWtProfile = null!;
        private Label labelWtProfile = null!;
        private Button btnInstall = null!;
        private Button btnUninstall = null!;
        private LinkLabel linkUpdate = null!;

        private readonly string _configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "embold-ssh");

        public Form1(string[] args)
        {
            InitializeComponent();

            // Set up event handlers
            btnInstall.Click += BtnApply_Click;
            btnUninstall.Click += BtnRemoveHandler_Click;
            comboTerminal.SelectedIndexChanged += ComboTerminal_SelectedIndexChanged;

            // Initialize config directory
            Directory.CreateDirectory(_configDir);

            LoadConfig();
            CheckForUpdate();

            // Set initial profile visibility after loading config
            ComboTerminal_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(_configDir, "config.json");
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
                // Load the saved profile if it exists
                if (doc.RootElement.TryGetProperty("wtProfile", out var profileProp))
                {
                    string savedProfile = profileProp.GetString() ?? "";
                    // The SelectedIndexChanged handler will populate the profiles, then we can select the saved one.
                    PopulateWtProfiles();
                    comboWtProfile.SelectedItem = savedProfile;
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

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            // Apply settings - register protocol handler with current app location
            ApplySettings();
        }

        private void ApplySettings()
        {
            try
            {
                string? currentAppPath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(currentAppPath))
                {
                    MessageBox.Show("Could not determine the application path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Apply settings without copying files
                string terminalPath = comboTerminal.SelectedItem?.ToString() switch
                {
                    "Windows Terminal" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "wt.exe"),
                    "Command Prompt" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe"),
                    "Ubuntu" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "wsl.exe"),
                    _ => comboTerminal.SelectedItem?.ToString() ?? "wt.exe"
                };

                // Use the app's embedded icon for protocol handler (Windows will extract it automatically)
                string protocolIconPath = $"{currentAppPath},0";

                // Update config file
                var config = new
                {
                    command = terminalPath,
                    wtProfile = comboTerminal.SelectedItem?.ToString() == "Windows Terminal" ? comboWtProfile.SelectedItem?.ToString() : null
                };
                File.WriteAllText(Path.Combine(_configDir, "config.json"), JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

                // Update registry entries for protocol handler - use current app path
                // Use HKEY_CURRENT_USER to avoid requiring admin privileges
                string regCommand = $"\"{currentAppPath}\" \"%1\"";
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Embold.SSH"))
                {
                    key.SetValue("", "URL:SSH Protocol");
                    key.SetValue("URL Protocol", "");
                    using (var iconKey = key.CreateSubKey("DefaultIcon")) { iconKey.SetValue("", protocolIconPath); }
                    using (var shellKey = key.CreateSubKey(@"shell\open\command")) { shellKey.SetValue("", regCommand); }
                }

                MessageBox.Show("SSH handler settings applied successfully.\n\nThe protocol handler now points to this application at its current location.", "Settings Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying settings: {ex.Message}", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRemoveHandler_Click(object? sender, EventArgs e)
        {
            // Remove protocol handler registration
            PerformRegistryCleanup();
        }

        private void PerformRegistryCleanup()
        {
            try
            {
                bool removedAny = false;

                // Try to remove from HKEY_CURRENT_USER first
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Embold.SSH", false);
                    removedAny = true;
                }
                catch { /* Key doesn't exist in HKCU, continue */ }

                // Try to remove from HKEY_LOCAL_MACHINE (may require admin)
                try
                {
                    Registry.LocalMachine.DeleteSubKeyTree(@"Software\Classes\Embold.SSH", false);
                    removedAny = true;
                }
                catch { /* Key doesn't exist in HKLM or no permission, continue */ }

                if (removedAny)
                {
                    // Ask if user wants to also remove config files
                    var result = MessageBox.Show("SSH protocol handler removed successfully.\n\nDo you also want to remove the configuration files from %LocalAppData%\\embold-ssh?\n\n(Note: The app will close after cleanup to release file locks.)",
                        "Handler Removed", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Create a PowerShell script to clean up after the app exits
                        var cleanupScript = Path.GetTempFileName() + ".ps1";
                        var scriptContent = $@"
# Wait for the SSH Handler app to fully exit
Start-Sleep -Seconds 3

# Remove the config directory
try {{
    if (Test-Path '{_configDir}') {{
        Remove-Item '{_configDir}' -Recurse -Force
        Write-Host 'Configuration files removed successfully.'
    }}
}} catch {{
    Write-Host 'Could not remove configuration files. You may need to delete them manually from: {_configDir}'
}}

# Clean up this script
Remove-Item '{cleanupScript}' -Force -ErrorAction SilentlyContinue
";

                        try
                        {
                            File.WriteAllText(cleanupScript, scriptContent);

                            // Start PowerShell to run the cleanup script in the background
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = "powershell.exe",
                                Arguments = $"-WindowStyle Hidden -ExecutionPolicy Bypass -File \"{cleanupScript}\"",
                                UseShellExecute = true,
                                CreateNoWindow = true
                            };
                            Process.Start(startInfo);

                            MessageBox.Show("SSH protocol handler removed. The application will now close and clean up configuration files.",
                                "Cleanup Scheduled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"SSH protocol handler removed, but could not schedule config cleanup: {ex.Message}\n\nYou can manually delete: {_configDir}",
                                "Cleanup Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        Application.Exit();
                    }
                }
                else
                {
                    MessageBox.Show("No SSH protocol handler found to remove.",
                        "Handler Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing handler: {ex.Message}", "Removal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                // Always link to releases page, regardless of version status
                linkUpdate.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo("https://github.com/emboldagency/embold-ssh/releases") { UseShellExecute = true });

                if (Version.TryParse(tagName, out var latestVersion) && Version.TryParse(currentVersion, out var appVersion))
                {
                    if (latestVersion > appVersion)
                    {
                        linkUpdate.Text = $"Update available: v{latestVersion}";
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

        private void PopulateWtProfiles()
        {
            comboWtProfile.Items.Clear();
            var profiles = GetWtProfiles();
            if (profiles.Any())
            {
                comboWtProfile.Items.AddRange(profiles.ToArray());
                comboWtProfile.SelectedIndex = 0;
            }
        }

        private List<string> GetWtProfiles()
        {
            var profiles = new List<string>();
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages", "Microsoft.WindowsTerminal_8wekyb3d8bbwe", "LocalState", "settings.json");

            if (!File.Exists(settingsPath)) return profiles;

            try
            {
                var json = File.ReadAllText(settingsPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("profiles", out var profilesElement) && profilesElement.TryGetProperty("list", out var listElement))
                {
                    foreach (var profile in listElement.EnumerateArray())
                    {
                        if (profile.TryGetProperty("name", out var nameElement))
                        {
                            profiles.Add(nameElement.GetString() ?? "");
                        }
                    }
                }
            }
            catch { /* Ignore parsing errors */ }
            return profiles;
        }

        private void ComboTerminal_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isWtSelected = comboTerminal.SelectedItem?.ToString() == "Windows Terminal";
            labelWtProfile.Visible = isWtSelected;
            comboWtProfile.Visible = isWtSelected;

            if (isWtSelected)
            {
                PopulateWtProfiles();
            }

            if (comboTerminal.SelectedItem?.ToString() == "Custom...")
            {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        if (!comboTerminal.Items.Contains(ofd.FileName))
                            comboTerminal.Items.Insert(comboTerminal.Items.Count - 1, ofd.FileName);
                        comboTerminal.SelectedItem = ofd.FileName;
                    }
                    else
                    {
                        comboTerminal.SelectedIndex = 0;
                    }
                }
            }
        }
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
                string? wtProfile = null;

                if (File.Exists(configPath))
                {
                    var configJson = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(configJson);
                    if (doc.RootElement.TryGetProperty("command", out var commandProp))
                    {
                        terminalCommand = commandProp.GetString() ?? "wt.exe";
                    }
                    if (doc.RootElement.TryGetProperty("wtProfile", out var profileProp))
                    {
                        wtProfile = profileProp.GetString();
                    }
                }

                string sshCommand = sshPort != null ? $"ssh -p {sshPort} {sshHost}" : $"ssh {sshHost}";
                string arguments;

                if (terminalCommand.Contains("wt.exe") && !string.IsNullOrEmpty(wtProfile))
                {
                    arguments = $"-p \"{wtProfile}\" {sshCommand}";
                }
                else if (terminalCommand.Contains("cmd.exe"))
                {
                    arguments = $"/k {sshCommand}";
                }
                else
                {
                    arguments = sshCommand;
                }

                var startInfo = new ProcessStartInfo(terminalCommand)
                {
                    UseShellExecute = true,
                    Arguments = arguments
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
