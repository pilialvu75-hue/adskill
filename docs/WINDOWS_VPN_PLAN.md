# ADS Kill: true local Windows VPN and browser filtering plan

## Target

Protect the entire Windows 11 laptop without rented servers. The product should let the user disable protection completely from the UI and independently from Windows settings, even when an ad is in progress. Do not block legitimate content categories.

### Architecture options

1. **Browser network interception (implemented as 0.2 prototype):** Chromium declarativeNetRequest + minimal cosmetic ad selectors. Most effective for web page banners/popunders, best safety. Cannot directly filter arbitrary native apps.
2. **Local DNS filter (next native Windows milestone):** loopback DNS resolver service with supervised fail-safe and reversible OS DNS setup. Blocks known ad hostnames across compatible apps. Not a VPN, not a complete HTTPS solution. Provide per-app exclusions only after source attribution is measurable.
3. **Full local VPN (engineering stage):** local TUN adapter using a signed driver (e.g. Wintun, subject to license audit), complete userspace IPv4/IPv6 transport, safe routing and DNS, robust error recovery, and a foreground app to control it. Prefer integration with a reliable existing open-source VPN stack over writing unsafe custom kernel code. Administrator elevation and driver installation require explicit device-owner consent.
4. **Advanced web filtering:** browser-specific filters where HTTPS content is inaccessible to the VPN. No covert TLS decryption, certificate substitution, or fake interception of private traffic.

### Release gates

- Baseline: record system DNS, adapters, proxy settings and routing; do not alter them until recovery is implemented.
- VPN must start and stop 20 times in tests without losing network, retaining DNS leaks, or forcing reboot.
- IPv4 and IPv6 work; Windows restart recovers to a user-understandable state.
- If filtering process dies, the laptop must recover connectivity in an explicitly documented fail-open mode by default.
- All privileges and driver changes explained to user; use standard Windows signed components; never tell the user to disable Defender/Smart App Control.
- In-network and DNS blocking is measured separately from YouTube browser advertisements.
- No recording or exfiltration of browsing history, visited adult sites, identity information or private traffic.
- Build and test system-wide network components in an isolated environment before publishing downloadable installer.
- No spend approved; no paid infrastructure.

## What a VPN can and cannot do

A local VPN can redirect device packets through a local virtual interface and apply policies to DNS/IP/connection metadata. But encrypted HTTPS hides page elements, and domain-only filtering cannot isolate ads served from the same domains as primary videos. Therefore browser filtering remains necessary even after the Windows VPN exists.

## Current state

0.2 browser blocking prototype is testable. **Full Windows VPN not implemented, installed, tested or published.** Legacy 0.1 HOSTS beta does not contain a tunnel driver.
