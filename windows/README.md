# ADS Kill Windows 0.2.0 Beta 1

Portable standalone Windows 11 x64 app migrated from the old `Ai-orchestrator-riserva` repository. Does not depend on AIVEXUS to build or run. This is still **the old 30-domain HOSTS filter**, not the announced full VPN. The separate `browser_adblock/` branch contains a different Chromium request/cosmetic filter, not bundled into this executable yet.

## What's new

- At application launch, check **public GitHub releases under pilialvu75-hue/adskill** automatically for a newer Windows release.
- Buttons **Controlla aggiornamenti** and **Scarica nuova beta**. A download happens *only* after the user confirms it.
- Only accept a correctly named `adskill-windows-vX.Y.Z[-betaN]` release and matching `AdsKill-Windows-vX.Y.Z[-betaN]-win-x64.zip` asset from the expected GitHub repository.
- Require SHA-256 `digest` advertised in GitHub's release asset API; verify the downloaded archive before finalizing it under `%LOCALAPPDATA%\AdsKill\updates`. On a failed checksum, delete the partial file.
- Open the download folder. **The beta does not automatically install or execute downloaded unsigned EXE files**. You must close Ads Kill, extract the ZIP, replace the portable app folder and reopen it.
- The updater does not collect telemetry or browsing data; it contacts GitHub's public releases API on startup (GitHub may see the network IP as with any web request).
- HOSTS file marker, backup, UAC requirements and protection toggle behavior unchanged.

## How to test

1. Download the ZIP from the `adskill-windows-v0.2.0-beta1` release and extract it.
2. Exit the old Ads Kill Windows app, and then run this version's `AdsKill.Windows.exe`. If required, accept standard UAC, but **do not disable Windows Defender or Smart App Control**.
3. At launch the app queries GitHub for new beta releases; if none is newer it should report none, then click **Controlla aggiornamenti** to test manually. The first beta tests the *checking infrastructure*; future beta releases test actual download and version comparison.
4. Test **Attiva blocco** then **Disattiva blocco**. HOSTS should be returned to its prior contents except for external changes.
5. Use the `browser_adblock` extension separately if testing website banners, since this app still filters only 30 exact DNS names.

**Neither this app nor the browser prototype bypasses sites' legal age verification.** The experimental Privacy & Age branch is parked; focus is ad blocking.

## Limitations and recovery

- No VPN, HTTPS interception, automatic silent installation or built-in YouTube pre/mid-roll removal.
- Binary unsigned; no warranty of being installable when Windows Smart App Control blocks it. Do not weaken OS security.
- If the app cannot disable HOSTS rules, run Notepad as administrator, open `C:\Windows\System32\drivers\etc\hosts`, delete only entries between `# ADS-KILL BEGIN v1` and `# ADS-KILL END v1`, and flush DNS with `ipconfig /flushdns`. Backups at `C:\ProgramData\AdsKill\backups`.
- Windows 8.1 is not supported (.NET 10).

## Build

```powershell
dotnet run --project windows/AdsKill.Windows.Tests/AdsKill.Windows.Tests.csproj -c Release
dotnet publish windows/AdsKill.Windows/AdsKill.Windows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
```
