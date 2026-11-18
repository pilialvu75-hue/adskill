# ADS Kill — Privacy & Age · Microsoft Edge / Chrome prototype

This is a **working, opt-in browser extension prototype**, developed in a separate branch of the independent ADS Kill repository.

## What it does today

- Activate/deactivate protection **one website at a time** by pressing the browser extension icon. Browser permissions are only requested for a site when the user explicitly enables them.
- Locally detect common age-verification headings and paths in Italian, French, English and Spanish.
- Show a non-blocking privacy assistant when a potential age-check page is detected; encourage selecting an official proof of adult status rather than sharing a raw ID with the visited website.
- **Best-effort** prevent selection, drag-and-drop and HTML form submission of files on detected age-check pages while the local guard is on.
- Allow the user, after a confirmation, to consent to file uploads **for that tab only**, without saving that decision.
- Site browsing, standard videos, regular pages and the website's lawful age-verification workflow remain accessible. Nothing is automatically accepted/approved on the user's behalf.

## What it DOES NOT do

- It is **not** an age-verification service and does not fabricate age, IDs, tokens, cookies, location or credentials.
- Does **not** bypass legal access restrictions or claim to unlock any adult website.
- **Cannot guarantee that documents/photos are never sent**: websites may use JavaScript Fetch/XHR, service workers, cross-origin iframes, shadow DOM, camera/WebRTC APIs, native apps or other flows beyond its access. It cannot inspect encrypted HTTPS bodies. Do not upload sensitive content if you do not trust a verifier.
- Does not detect a person's real age, integrate with government wallets or issue cryptographic proof.
- No background service, VPN or website proxy. This extension is not yet integrated into the Android app or Windows HOSTS blocker.
- No analytics, domain history database, identity-file access, cookies, network API calls, remote code or paid dependency.

**Threat model:** This first version is a usability/privacy helper, not a verified DLP security boundary. Never advertise it as an absolute shield from identity/biometric upload.

## Install on ASUS Windows 11, Edge

1. Download the workflow artifact **ADS-Kill-Privacy-Age-Edge-v0.1.0** from GitHub Actions (or clone this branch).
2. Unzip the artifact. The folder to install is `browser_age_guard` and it must contain `manifest.json`, `popup.html`, `popup.js`, `age-signals.js` and `content.js`.
3. In Microsoft Edge enter `edge://extensions`.
4. Turn on **Developer mode**, then press **Load unpacked / Carica estensione decompressa**. Select the `browser_age_guard` folder.
5. Click the new ADS Kill extension icon while on a website you wish to protect; choose **Attiva su questo sito** and confirm that site's permission.
6. Navigate to a verification page and observe the non-blocking privacy card. No data is sent by ADS Kill. Use **Disattiva su questo sito** to undo all extension permissions.

For the first smoke test, use an ordinary/non-sensitive page or local controlled fixture — do **not** upload real ID documents or selfie images.

## Privacy engineering

The extension reads only limited on-page headings, page title and pathname *locally* in the opted-in site. It does not read inputs, names, file contents, file names, birthdates, photo pixels, usernames, wallet credentials or query strings. It stores no history. The one-time user decision to permit file selection is held only in the open tab's memory.

Because this uses an opt-in site permission, closing the extension UI does not stop browsing. Extension permissions can be revoked at any time in Edge.

## Development and validation

Run without third-party packages:

```sh
node --test browser_age_guard/tests/age-signals.test.cjs
node --check browser_age_guard/popup.js
node --check browser_age_guard/content.js
```

The underlying pure-Dart privacy age assessment lives separately in `privacy_age/` and remains unchanged.

## Next steps (not implemented)

- Integration of the existing Android/Windows applications after source/licensing reconciliation.
- Improve file and webcam guard with browser-level permission controls; honest limits even then.
- Recognize actual supported privacy-preserving EU/FR credential/wallet issuance options per provider, without retaining identifying information.
- Independent audit, site-specific compatibility tests, threat model and legal review before a paid launch.

Official background:
- https://digital-strategy.ec.europa.eu/en/policies/eu-age-verification
- https://www.cnil.fr/fr/verification-de-lage-en-ligne-trouver-lequilibre-entre-protection-des-mineurs-et-respect-de-la-vie
