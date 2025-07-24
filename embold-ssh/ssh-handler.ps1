param (
    [string]$Url,
    [switch]$VerboseLog
)

# Function to handle logging
function Write-Log {
    param (
        [string]$Message
    )
    if ($VerboseLog) {
        $logPath = Join-Path $env:LocalAppData "embold-ssh\handler.log"
        Add-Content -Path $logPath -Value $Message
    }
}

# Log the input URL
Write-Log "Input URL: $Url"

# Parse the SSH URL
$parsedUrl = $Url -replace 'ssh://', '' -replace '/', ''
$urlParts = $parsedUrl -split ':'
$sshHost = $urlParts[0]
$sshPort = if ($urlParts.Length -gt 1) { $urlParts[1] } else { $null }

# Log the parsed values
Write-Log "Host: $sshHost, Port: $sshPort"

# Load configuration
$configPath = Join-Path $env:LocalAppData "embold-ssh\config.json"
if (Test-Path $configPath) {
    $config = Get-Content $configPath | ConvertFrom-Json
    $terminalCommand = $config.command
} else {
    # Default terminal command
    $terminalCommand = "wt.exe"
}

# Log the terminal command
Write-Log "Terminal Command: $terminalCommand"

# Build the SSH command
if ($sshPort) {
    $sshCommand = "ssh -p $sshPort $sshHost"
} else {
    $sshCommand = "ssh $sshHost"
}

# Log the SSH command
Write-Log "SSH Command: $sshCommand"

# Execute the terminal command, special handling for cmd.exe
if ($terminalCommand -like "*cmd.exe") {
    Start-Process -FilePath $terminalCommand -ArgumentList "/k $sshCommand"
} else {
    Start-Process -FilePath $terminalCommand -ArgumentList $sshCommand
}
