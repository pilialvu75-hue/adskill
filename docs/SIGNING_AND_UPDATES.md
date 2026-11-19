# ADS Kill — permanently signed Android updates

## Current state

- The independent repository is `https://github.com/pilialvu75-hue/adskill`.
- The first migrated Android version is `0.1.9-beta1`, `versionCode=11`, `applicationId=dev.aivexus.adskill`.
- GitHub Actions **cannot publish a persistent signing key automatically** without compromising the secret. A repo owner must provision it once. Do not put keystores, passwords or base64 keys in issues, pull requests, chat or source files.
- When the secrets are absent, only a debug test artifact/release is published. **This is not an update of an installed debug build.**

## One-time owner setup on Windows 11

1. Install a Java JDK with `keytool` (JDK 17 recommended) and the official GitHub CLI `gh` if they are not already present.
2. In PowerShell, run `gh auth login` and authenticate with the GitHub account that manages `pilialvu75-hue/adskill`.
3. Clone or download this standalone repository locally. From its root run `./scripts/ads-kill-signing-setup.ps1` in PowerShell (if script execution is restricted, use an approved method without weakening system-wide security). The script first checks GitHub access and tools, then creates a dedicated PKCS12 key interactively **only if it does not already exist**.
4. Store the key password securely and back up the `.p12` file to a separate offline location. Do not lose or rotate it: installed apps require the same signing certificate for in-place updates.
5. The script uploads **four** values directly to the repository's encrypted GitHub Actions secrets over authenticated GitHub CLI: `ADS_KILL_KEYSTORE_BASE64`, `ADS_KILL_STORE_PASSWORD`, `ADS_KILL_KEY_ALIAS`, `ADS_KILL_KEY_PASSWORD`. It never commits or publishes the private key.
6. In `adskill` open **Actions → ADS Kill - Android standalone beta → Run workflow** on `main`. Confirm the step **Build stable-signed Android APK** succeeds and the release lists `AdsKill-Android-v0.1.9-beta1-signed.apk`. If CI fails, inspect errors without publishing secret values.
7. **One-time transition on the S24 FE and Xiaomi:** disable Ads Kill VPN (use Android Settings → VPN if necessary), record settings, uninstall the old experimental debug-signed Ads Kill, install the first stable-signed APK. Verify its version in the Home/Settings screens and approve VPN setup if required.
8. After that, do not uninstall ADS Kill for each update. New signed releases must increment `versionCode`, use the same package ID and same signing key. The app checks for beta updates once per day on launch (switchable off in Settings) or on demand. It verifies hash, package, version and signing certificate; **Android still requires the user's consent to install**.

## Security and failure modes

- No automatic APK installation. The app never turns off device security or bypasses Android signature verification.
- The release asset's SHA-256 `digest` comes from GitHub's release API; the downloaded file is independently hashed.
- The signed APK certificate is verified against the installed app before opening the installer. Releases with no signed asset, an invalid SHA, a wrong package/certificate, or lower version are rejected.
- Never regenerate the signing key during an update. If you lose the `.p12` but GitHub secrets still exist, do not modify those secrets; protect that state and create an offline backup of the original key if available.
- If VPN stays connected, Android Settings → Connections → More connection settings → VPN → Ads Kill → Disconnect/Forget. Do not disable Android security protections.
- Samsung Galaxy S24 FE and Xiaomi real-device tests remain essential. CI compilation alone does not certify VPN behavior.
- The separate Windows client currently downloads checksum-verified unsigned ZIPs with user approval; Android signing setup does **not** sign the Windows EXE.
