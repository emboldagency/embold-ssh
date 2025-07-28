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
            try
            {
                // Check if the app was launched with an ssh:// URL argument.
                // If so, handle it silently and exit without showing any UI.
                if (args.Length > 0 && args[0].StartsWith("ssh://", StringComparison.OrdinalIgnoreCase))
                {
                    UrlHandler.HandleSshUrl(args[0]);
                    return; // Exit immediately after handling.
                }

                // If no URL argument, run the main settings form.
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1(args));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
