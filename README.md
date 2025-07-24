# emBold SSH Handler

**emBold SSH Handler** lets you open `ssh://` links in your favorite terminal (Windows Terminal, Command Prompt, Ubuntu, or custom) on Windows.

---

## Download & Installation

1. **Download** and extract [embold-ssh.zip](/downloads/embold-ssh/embold-ssh.zip).)` to a folder (e.g., `C:\Programs\emBoldSSH`).
2. **Run** `emBoldSSHHandler.exe`.  
   > **Note:** The app will automatically request administrator privileges.
3. Select your preferred terminal and icon, then click **Install Handler**.
4. You can now click `ssh://user@host` links in browsers or apps, and they will open in your chosen terminal.

---

## Windows Defender Notice

> Because this app uses scripting and protocol handlers, Windows Defender may flag it as suspicious.

- If blocked, open **Windows Security → Virus & threat protection → Protection history** and allow the app.
- Add the app's folder to **Defender Exclusions** for smoother use:
  1. Go to **Windows Security → Virus & threat protection → Manage settings → Exclusions**.
  2. Add your app's folder as an exclusion.
- If you trust this app, you can [submit it to Microsoft as a false positive](https://www.microsoft.com/en-us/wdsi/filesubmission).

---

## Uninstall

1. Run `emBoldSSHHandler.exe`.
2. Click **Uninstall Handler**.
3. Delete the app folder if you wish.

---

## Troubleshooting

- **Handler install fails:** Make sure you allow the app to run as administrator when prompted.
- **Windows Defender blocks/quarantines:** Allow the app in Protection History and add an exclusion.
- **CMD doesn't SSH:** The app supports `cmd.exe` with proper arguments. If issues persist, ensure `ssh.exe` is in your PATH.
- **Other issues:** Check `%LOCALAPPDATA%\embold-ssh\handler.log` for logs.

---

## Security

- This app is unsigned and may be flagged by antivirus software. Only use if you trust the source.
- For enterprise or public distribution, consider code signing.

---

## Generating the HTML README

This project uses [Pandoc](https://pandoc.org/) to generate a user-friendly HTML version of this README from the Markdown source.

### How to install Pandoc

**Windows (with winget):**
```sh
winget install --source winget --exact --id JohnMacFarlane.Pandoc
```

**Windows (manual):**
1. Download the installer from [Pandoc Releases](https://github.com/jgm/pandoc/releases/latest).
2. Run the installer and follow the prompts.

**macOS (with Homebrew):**
```sh
brew install pandoc
```

**Linux (Debian/Ubuntu):**
```sh
sudo apt-get install pandoc
```

Or see the [Pandoc installation guide](https://pandoc.org/installing.html) for more options.

### How to generate README.html

After installing Pandoc, run this command in your project folder:

```sh
pandoc README.md -o README.html
```

This will create an HTML version of the README for

---

## Publishing the HTML README and Download Page

### 1. Build the HTML README into your site folder

We use [Pandoc](https://pandoc.org/) to convert the Markdown README to HTML and place it in the `docs/` folder (or `site/` if you prefer):

```sh
pandoc README.md -o docs/index.html --standalone --css readme-style.css
```

> Make sure your CSS file is also in the `docs/` folder as `readme-style.css`.

### 2. Deploy to your web server

Use `rsync` to upload the entire `docs/` folder to your server:

```sh
rsync -av --exclude='readme-style.css' ./docs/ root@staging.ssh.embold.net:/home/embold-su/webapps/embold/downloads/embold-ssh/
```

- This will sync all files (except the CSS file, since pandoc will embed it in the html) in `docs/` (including `index.html` and any other assets) to your downloads directory on the server.
