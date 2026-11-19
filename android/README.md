# ADS Kill Android — standalone 0.1.9-beta1

This is the independent Android project. Code originated in `pilialvu75-hue/Ai-orchestrator-riserva/ads-kill/android` at commit `1c01776e0e081cf0a780d57c29be6054eb5075f5`; it now builds and publishes **only** from `pilialvu75-hue/adskill`. Package ID stays `dev.aivexus.adskill` to avoid gratuitous compatibility breaks.

## Features / caveats

- Android 10+, DNS-only filtering through local `VpnService`; manual switch and emergency stop; link to Android VPN settings; manifest explicitly disallows Always-on VPN in this beta.
- Smart Reward DNS heuristics, remembered per-app choices, 60-second global DNS allowance, experimental black overlay and media-volume mute, IT/FR/EN/ES, themes.
- Installed version/build and debug-vs-release status visible in app.
- Optional beta update check once per day when app opens (signed builds only), plus manual check. No silent downloads/installations; user confirms updates with Android's installer.
- **DNS requests cannot identify a rewarded advertisement, video completion or the requesting app reliably.** Black screen/mute can interfere with earned rewards. YouTube in-stream ads are not generally blocked by DNS.
- Samsung S24 FE VPN stop fix from beta 0.1.8 migrated; *real-device validation is still required*.

## Build

```bash
cd android
gradle --no-daemon assembleDebug
```

Requires Java 17, Android SDK 35 and Gradle 8.10.2 (CI installs dependencies). The [Android standalone workflow](../.github/workflows/ads-kill-android.yml) also builds artifacts and publishes a prerelease.

## Signing and updates

**An APK signed with a new certificate cannot update an existing APK installed under another certificate.** Old CI debug APKs used ephemeral signatures. Consequently there is **no safe one-click upgrade** from the original experimental debug builds: users must stop the VPN and perform a one-time uninstall before installing the first stable-signed ADS Kill APK, losing local Ads Kill settings. This does not delete data from other apps.

The new repo must have four GitHub Actions secrets configured: `ADS_KILL_KEYSTORE_BASE64`, `ADS_KILL_STORE_PASSWORD`, `ADS_KILL_KEY_ALIAS`, `ADS_KILL_KEY_PASSWORD`. Without all of them, CI produces a DEBUG test APK and clearly labels the prerelease as non-upgradable. It never fabricates a signed artifact.

With the permanent key configured, the workflow builds `AdsKill-Android-v0.1.9-beta1-signed.apk` with the independent repo's signer. Future versions must use **the same key** with an increasing `versionCode` and new release tags `adskill-android-vX.Y.Z-betaN`.

The updater accepts only a matching signed release asset from the independent GitHub repo, checks GitHub SHA-256, package name, higher Android versionCode and matching APK signing certificate; Android installer also verifies the signature. Full setup: [Signing and update runbook](../docs/SIGNING_AND_UPDATES.md).

Historical Android development notes from AIVEXUS: [Android history](../docs/ANDROID_HISTORY.md). Windows client remains a separate project under [windows](../windows).
