# ADS Kill — Edge / Chrome browser shield 0.2 beta

## Why this exists

The current ADS Kill Windows 0.1.0 app only writes **30 exact HOSTS entries**. User tests on ASUS Windows 11 showed numerous ordinary website banners and advertisements surviving. A full-tunnel VPN would intercept more network traffic but **would not automatically remove ads delivered by the same website through HTTPS** or reliably remove YouTube in-stream ads.

This browser extension is a **separate immediately testable ad-blocking layer**. Unlike HOSTS, it can match request paths and hide well-known ad containers in the browser DOM.

## What it implements now

- Offline Manifest V3 `declarativeNetRequest`: 132 manually curated ad network domains, including several commonly associated with third-party display and pop-under advertising.
- 16 conservative **third-party-only** URL path patterns for common ad scripts, prebid libraries and ad-serving resources.
- Cosmetic hiding of specific known ad selectors, without broad page reconstruction or interfering with age checks.
- **Enable/disable switch**, both browser network rulesets and cosmetic hiding. Reload the page to compare.
- **No accounts, subscriptions, remote telemetry, browser history collection, server proxy or Windows admin elevation**. No purchased filters.
- Does not block adult sites as a category. It targets third-party advertising, not page content.
- Independent of AIVEXUS and of the Windows HOSTS beta. You can use both, but test them independently to attribute results.

## Honest limitations

- **Not itself a VPN**. Works only in the browser with the extension installed (Edge / Chrome).
- Blocks only the included network patterns and visible selectors; many first-party and dynamically obfuscated ads can survive.
- **YouTube pre-roll/mid-roll ads may continue**, as video and advertisements can share infrastructure. Blocking `youtube.com` or `googlevideo.com` would break playback and is deliberately avoided.
- The generic cosmetic selectors can sometimes hide a legitimate component. The one-click disable restores it, and reloading the page removes stale ad placeholders.
- Popups redirected to ad networks in the domain set may be blocked. This does not guarantee stopping all popup windows.
- Edge prompts for access to website content because the cosmetic script is loaded on web pages. The script only adds a CSS style and reads the extension's on/off preference. It does not collect or transmit browsing data.
- No license from third-party EasyList/AdGuard filter lists is assumed: the current rules are authored here. A future commercial release needs formal licensing and security review.

## Install on ASUS Windows 11

1. Download the ZIP artifact `ADS-Kill-Edge-Browser-Shield-0.2.0` from GitHub Actions, or retrieve this branch.
2. Unzip the archive to a normal folder. The folder to select is `browser_adblock`, with `manifest.json` inside.
3. Open **Microsoft Edge**, navigate to `edge://extensions`, enable **Developer mode**, click **Load unpacked / Carica estensione decompressa**, select the `browser_adblock` folder.
4. Open any ordinary site with banners and compare with Ads Kill enabled and disabled, **reloading between comparisons**.
5. To isolate the browser test, first click **Disattiva blocco** in the legacy ADS Kill Windows app. It may be closed after that.
6. You may remove the extension in Edge at any time. No HOSTS file or Windows DNS setting is touched.

Do not disable Windows Defender, Smart App Control, TLS certificate checks or age restrictions to run the test.

## Future *true* local VPN (not included in 0.2)

A full system-wide VPN requires a signed virtual adapter (for example Wintun) or a platform-approved packet filtering stack, administrator permission, IPv4/IPv6 handling, fail-open/fail-safe recovery, trusted DNS upstream handling, and a verified kill switch. **It is not an ad blocker by itself.** Redirecting all Windows traffic through a tunnel that has no reliably implemented routing is unsafe and could break connectivity.

Follow a staged plan in `docs/WINDOWS_VPN_PLAN.md`. Do not ship a pretend VPN, and do not install a filter driver without explicit consent.

### Dev checks

```sh
node browser_adblock/tests/validate.cjs
node --check browser_adblock/popup.js
node --check browser_adblock/cosmetic.js
```
