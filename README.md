# **emBold SSH Handler for Windows**

**emBold SSH Handler** lets you open `ssh://` links in your favorite terminal (Windows Terminal, Command Prompt, Ubuntu, or custom) on Windows.

## **Download & Installation**

1. **Download** and extract the latest `embold-ssh.zip` from the [GitHub Releases page](https://github.com/emboldagency/embold-ssh/releases).  
2. Place the extracted `SSHHandlerApp.exe` in a permanent location (e.g., `%LocalAppData%\emBoldSSH` or `C:\Program Files\emBoldSSH` ).  
3. **Run** `SSHHandlerApp.exe`.  
4. Select your preferred terminal and icon, then click **Install / Update**.  
5. You can now click `ssh://user@host` links, and they will open in your chosen terminal.

## **Windows Defender Notice**

Because this app registers a custom protocol handler, Windows Defender or other antivirus software may show a warning upon first run.

* If blocked by SmartScreen, click "More info" \-\> "Run anyway".  
* If you trust this application, you can add its folder to your antivirus exclusion list to prevent future warnings.

## **Uninstall**

1. Run `SSHHandlerApp.exe`.  
2. Click **Uninstall**. This will remove the registry entries.  
3. You can then safely delete the application file.

## **Building and Publishing (for Developers)**

This project is set up with GitHub Actions to automatically build and create a new release when a new version tag (e.g., `v1.2.3`) is pushed to the repository.

For local development and testing, you can use the following `dotnet` CLI commands from the root of the project directory.

### **1\. Build the Application**

Builds the project and its dependencies.

```
dotnet build "SSH Handler.sln"
```

### **2\. Run for Testing**

Runs the application directly from the source for quick testing and debugging.

```
dotnet run --project SSHHandlerApp.csproj
```

### **3\. Publish for Release**

To create the final standalone executable for distribution, use the `publish` command. This bundles the app and the .NET runtime into a single `.exe`.

```
dotnet publish SSHHandlerApp.csproj -c Release -r win-x64 --self-contained=true /p:PublishSingleFile=true
```

This command will output the final `SSHHandlerApp.exe` file into the `.\bin\Release\net8.0-windows\win-x64\publish\` directory.

### **4\. Create the Release Zip**

After publishing, you should create a `.zip` file containing the executable.

1. Navigate to the output directory: `.\bin\Release\net8.0-windows\win-x64\publish\`  
2. Right-click on `SSHHandlerApp.exe` and choose `Send to -> Compressed (zipped) folder`.  
3. Name the file `embold-ssh.zip`. This is the file that should be attached to a new GitHub release.