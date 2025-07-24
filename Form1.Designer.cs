namespace SSHHandlerApp;

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
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(500, 250);
        this.Text = "SSH Protocol Handler Setup";

        // Terminal Executable
        // Only comboTerminal and comboIcon are used for selection now.

        // Install Button
        this.btnInstall = new System.Windows.Forms.Button();
        this.btnInstall.Text = "Install Handler";
        this.btnInstall.Location = new System.Drawing.Point(150, 120);
        this.btnInstall.Size = new System.Drawing.Size(120, 30);
        this.btnInstall.Click += new System.EventHandler(this.BtnInstall_Click);
        this.Controls.Add(this.btnInstall);

        // Uninstall Button
        this.btnUninstall = new System.Windows.Forms.Button();
        this.btnUninstall.Text = "Uninstall Handler";
        this.btnUninstall.Location = new System.Drawing.Point(280, 120);
        this.btnUninstall.Size = new System.Drawing.Size(120, 30);
        this.btnUninstall.Click += new System.EventHandler(this.BtnUninstall_Click);
        this.Controls.Add(this.btnUninstall);
    }

    #endregion
}
