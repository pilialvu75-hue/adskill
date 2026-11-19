# One-time local signing setup for the independent ADS Kill Android app.
# The key NEVER leaves this PC except encrypted into GitHub Actions secrets.
# Requires: JDK keytool, GitHub CLI (gh), logged-in GitHub account with repo admin access.
# PowerShell 5.1+ on Windows 11. Do NOT share this .p12 or its password in chat.
param(
    [string]$Repository = 'pilialvu75-hue/adskill',
    [string]$KeystorePath = (Join-Path $env:LOCALAPPDATA 'AdsKill\Signing\ads-kill-release.p12')
)
$ErrorActionPreference = 'Stop'
$alias = 'ads-kill-release'

function Require-Command($name) {
    if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
        throw "Comando $name mancante. Installa prima Java JDK e GitHub CLI (gh)."
    }
}
function Set-SecretStdin([string]$name, [string]$secret) {
    # Use standard input WITHOUT a trailing newline and WITHOUT placing the
    # value on the command line or in a plaintext environment file.
    $info = New-Object System.Diagnostics.ProcessStartInfo
    $info.FileName = 'gh'
    $info.Arguments = "secret set $name --repo $Repository"
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.RedirectStandardInput = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $process = [System.Diagnostics.Process]::Start($info)
    try {
        $process.StandardInput.Write($secret)
        $process.StandardInput.Close()
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            throw "Errore creando secret $name. Controlla autorizzazioni GitHub."
        }
        Write-Host "$name impostato nel repository." -ForegroundColor Green
    } finally {
        $process.Dispose()
    }
}

if ($Repository -ne 'pilialvu75-hue/adskill') {
    throw 'Repository non autorizzato per la firma ADS Kill.'
}
Require-Command 'gh'
Require-Command 'keytool'
& gh auth status *> $null
if ($LASTEXITCODE -ne 0) {
    throw 'Accedi prima con: gh auth login'
}

$folder = Split-Path -Parent $KeystorePath
if (-not (Test-Path -LiteralPath $folder)) {
    New-Item -ItemType Directory -Path $folder -Force | Out-Null
}
if (-not (Test-Path -LiteralPath $KeystorePath)) {
    Write-Host 'Creazione della chiave permanente: keytool richiedera una password.' -ForegroundColor Cyan
    Write-Host 'Conserva la password e fai una copia offline della chiave. NON rigenerarla agli aggiornamenti successivi.'
    & keytool -genkeypair -keystore $KeystorePath -storetype PKCS12 `
        -alias $alias -keyalg RSA -keysize 3072 -validity 10000 `
        -dname 'CN=ADS Kill Android, O=ADS Kill, C=FR'
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $KeystorePath)) {
        throw 'Creazione della chiave non riuscita. Nessun secret caricato.'
    }
} else {
    Write-Host 'Riutilizzo della chiave permanente gia presente: nessuna rotazione.' -ForegroundColor Cyan
}
$securePassword = Read-Host 'Inserisci la password della chiave per GitHub Actions' -AsSecureString
$pointer = [IntPtr]::Zero
$password = $null
try {
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    if ([string]::IsNullOrWhiteSpace($password)) { throw 'Password vuota.' }
    $keystoreBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($KeystorePath))
    Set-SecretStdin 'ADS_KILL_KEYSTORE_BASE64' $keystoreBase64
    Set-SecretStdin 'ADS_KILL_STORE_PASSWORD' $password
    Set-SecretStdin 'ADS_KILL_KEY_ALIAS' $alias
    Set-SecretStdin 'ADS_KILL_KEY_PASSWORD' $password
    Write-Host 'Firma configurata: rilancia il workflow ADS Kill - Android standalone beta nel repository.' -ForegroundColor Green
    Write-Host ('Chiave locale: ' + $KeystorePath)
    Write-Host 'Conserva un backup offline della .p12 e la password. Non pubblicarli.'
} finally {
    $password = $null
    $securePassword.Dispose()
    if ($pointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
}
