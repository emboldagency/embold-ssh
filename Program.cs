using System;
using System.Threading;
using System.Windows.Forms;

namespace SSHHandlerApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Check if the app was launched with an ssh:// URL argument.
            // If so, handle it silently and exit without showing any UI.
            if (args.Length > 0 && args[0].StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
            {
                UrlHandler.HandleSshUrl(args[0]);
                return; // Exit immediately after handling.
            }

            // Create a mutex to prevent multiple instances
            bool createdNew;
            using var mutex = new Mutex(true, "EmboldSSHHandler_SingleInstance", out createdNew);
            
            if (!createdNew)
            {
                // Another instance is already running
                MessageBox.Show("emBold SSH Handler is already running.", "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // If no URL argument, run the main settings form.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(args));
        }
    }
}
