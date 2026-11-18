# ADS Kill — commercial Privacy & Age scope

Decision record, 18 November 2026. No commercial publication or paid services are approved.

## Claim we may aspire to make
"Helps adults understand and choose privacy-preserving age verification, without sending full identity documents to the site when a compliant proof-only option is available."

**Cannot yet claim:** "anonymous access guaranteed", "unblocks age-gated sites", "age successfully verified", "approved by CNIL", "double anonymity certified", or "compatible with every site".

## Why different from an age-check bypass
In France, covered adult sites must use a robust age verification mechanism. ADS Kill cannot legally or technically replace a site's server-side verification by filtering a popup, hiding an overlay, changing location, manipulating cookies, supplying fake ID tokens, or asserting an age. A privacy-preserving proof is real cryptographic assurance with minimal disclosure, issued and verified through a supported, trusted provider.

## Data minimization
The first module processes only *categories* of requested data. It does not store names, birth dates, scans, identity numbers, visited domain names, proofs, cookies or tracking IDs. Before any real implementation, verify SDK/network behaviors and define a privacy notice and deletion behavior.

## Dependencies for real launch
- Android ADS Kill source import / licensing + regression suite on S24 FE
- Browser flow analysis on supported websites: documented official provider integration only
- Real proof-of-age protocol and issuance+verification, security audit
- Consent and privacy UX, no device age inference
- Platform rules (Google Play ad-interference and VpnService policies are nontrivial)
- Legal publisher and economic decision

## 0 € launch strategy candidate
No direct ad monetization based on sensitive browsing. The privacy module may support differentiation of ADS Kill's legitimate security utility. Monetization hypothesis: a voluntary support tier for advanced privacy education/control, *not* payment to bypass legal age checks. Conversion demand has not been tested.
