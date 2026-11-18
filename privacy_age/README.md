# ADS Kill — Privacy & Age (technical preview)

**State: independent Dart preflight, NOT integrated into the Android ADS Kill application.** The Android app source has not yet been imported into this newly initialized repository. This feature cannot currently protect live website traffic or complete age verification.

## Purpose
Assess a *description* of an age-verification flow on-device, without collecting a site address, date of birth, ID document, selfie, name, age assertion, browsing history or device identifier.

- Prefer systems that provide only a **proof that the user meets the required age threshold** to a site.
- Explain that submitting a photo ID directly to a website is a different privacy exposure from identity checked by an independent issuer using a privacy-preserving token.
- Surface unknown recipient, linkability or retention details as unverified.
- Never infer adulthood from the app, never simulate cryptographic proofs, and never disable, skip, falsify or override a website's legal verification gate.
- Do not route traffic, access DNS/HTTPS, intercept personal data or contact any external service.

This is advisory **metadata-only** code. The enum labels and boolean descriptors are supplied by a future integration adapter. The module **does not verify** what a provider promises. Even its best rating is explicitly a *candidate*, not an independent audit or certification.

## Policy behavior
- **insufficientInformation** — invalid/absent age threshold claim or incomplete set of declared disclosures.
- **highExposure** — the site requests any extra data (documents, legal name, biometrics etc.), or either side is linkable to identity/visited site.
- **reviewRequired** — details are unknown or a third-party verifier handles sensitive data.
- **minimalDisclosureCandidate** — the submitted metadata describes disclosure of just an age-threshold claim, with known linkability answers and retention information; this is **not verified by ADS Kill**.

No decision ever unlocks a website. User consent is required in any future real integration.

## Offline check

With Dart SDK >=3.3:

```sh
cd privacy_age
dart pub get
dart analyze --fatal-infos
dart run tool/self_test.dart
```

No runtime packages or network access are used by the preflight.

## Future actual integration (blocked on Android sources)
1. Import / identify the existing ADS Kill Android implementation and licenses **without replacing the working device app**.
2. Implement an explicit opt-in local privacy-screen/UI that displays what is known and unknown. No hidden telemetry, profile, site history, fingerprinting or raw identity storage.
3. Explore **official, independently verifiable** wallet / proof-of-age integrations (EU age verification blueprint, approved age-attribute issuers, France's double-anonymity requirements). Cryptographic verification and trusted issuer status are **not** simulated by this component.
4. Maintain the site's legitimate age gate; if compliant proof is unavailable, explain the limitation rather than bypass.
5. Review security, browser/API constraints, provider privacy claims, GDPR, CNIL/Arcom and market policies before commercialization.

References:
- https://digital-strategy.ec.europa.eu/en/policies/eu-age-verification
- https://digital-strategy.ec.europa.eu/en/factpages/blueprint-age-verification-solution-help-protect-minors-online
- https://www.legifrance.gouv.fr/jorf/id/JORFTEXT000050385836
- https://www.cnil.fr/fr/
- https://support.google.com/googleplay/android-developer/answer/18265346
