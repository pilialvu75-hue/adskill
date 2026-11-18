/// ADS Kill Privacy & Age, offline disclosure preflight.
///
/// This is a risk review of declared data flows, NOT age verification.
/// It does not accept or retain names, identity documents, birth dates, URLs,
/// cryptographic proofs, cookies or tokens. It makes no network requests.
/// It cannot unlock a site or override a site's verification requirements.
library;

/// Which *category* of data would be disclosed, never its raw value.
enum AgeDataCategory {
  ageAboveRequiredThreshold,
  exactBirthDate,
  legalName,
  identityDocument,
  faceOrBiometricSample,
  postalAddress,
  browsingHistory,
  otherPersonalData,
}

/// A verifier may require ID to issue a proof without sharing ID with a site.
enum DisclosureRecipient { website, independentVerifier }

class ProposedDisclosure {
  const ProposedDisclosure({
    required this.category,
    required this.recipient,
  });

  final AgeDataCategory category;
  final DisclosureRecipient recipient;
}

/// Metadata-only description of a proposed age-check mechanism.
///
/// All nullable fields are explicitly UNKNOWN when null; callers must NOT
/// interpret unknown as false or as a privacy guarantee. Statements supplied
/// by a provider need independent validation before commercial claims.
class AgeCheckMetadata {
  const AgeCheckMetadata({
    required this.requiredAge,
    required this.disclosures,
    this.siteCanLinkProofToIdentity,
    this.verifierLearnsWhichSite,
    this.retentionExplained,
  });

  final int requiredAge;
  final List<ProposedDisclosure> disclosures;
  final bool? siteCanLinkProofToIdentity;
  final bool? verifierLearnsWhichSite;
  final bool? retentionExplained;
}

enum AgePrivacyLevel {
  insufficientInformation,
  highExposure,
  reviewRequired,
  minimalDisclosureCandidate,
}

class AgePrivacyReport {
  const AgePrivacyReport({
    required this.level,
    required this.reasons,
  });

  final AgePrivacyLevel level;
  final List<String> reasons;

  /// This report is not evidence of successful verification or compliance.
  bool get verifiedAge => false;
  bool get mayBypassSiteGate => false;
}

class AgePrivacyPreflight {
  const AgePrivacyPreflight();

  AgePrivacyReport inspect(AgeCheckMetadata metadata) {
    if (metadata.requiredAge < 1 ||
        metadata.requiredAge > 125 ||
        metadata.disclosures.isEmpty) {
      return const AgePrivacyReport(
        level: AgePrivacyLevel.insufficientInformation,
        reasons: <String>['Missing/invalid age threshold or disclosure list.'],
      );
    }

    final toSite = metadata.disclosures
        .where((item) => item.recipient == DisclosureRecipient.website)
        .map((item) => item.category)
        .toSet();

    if (!toSite.contains(AgeDataCategory.ageAboveRequiredThreshold)) {
      return const AgePrivacyReport(
        level: AgePrivacyLevel.insufficientInformation,
        reasons: <String>['No age-threshold claim disclosed to the site.'],
      );
    }

    final reasons = <String>[];
    final siteExcess = toSite.difference(
      const <AgeDataCategory>{AgeDataCategory.ageAboveRequiredThreshold},
    );
    if (siteExcess.isNotEmpty) {
      reasons.add('The site is asking for data beyond an age-threshold claim.');
    }
    if (metadata.siteCanLinkProofToIdentity == true) {
      reasons.add('The site could link the proof to a person's identity.');
    }
    if (metadata.verifierLearnsWhichSite == true) {
      reasons.add('The verifier could learn which site was visited.');
    }
    if (reasons.isNotEmpty) {
      return AgePrivacyReport(
        level: AgePrivacyLevel.highExposure,
        reasons: List<String>.unmodifiable(reasons),
      );
    }

    if (metadata.siteCanLinkProofToIdentity == null ||
        metadata.verifierLearnsWhichSite == null ||
        metadata.retentionExplained != true) {
      return const AgePrivacyReport(
        level: AgePrivacyLevel.reviewRequired,
        reasons: <String>[
          'Linkability, verifier visibility or retention is unconfirmed.',
        ],
      );
    }

    final toVerifier = metadata.disclosures
        .where(
          (item) => item.recipient == DisclosureRecipient.independentVerifier,
        )
        .map((item) => item.category)
        .toSet();
    if (toVerifier.any(
      (item) => item != AgeDataCategory.ageAboveRequiredThreshold,
    )) {
      return const AgePrivacyReport(
        level: AgePrivacyLevel.reviewRequired,
        reasons: <String>[
          'The independent verifier may process identifying data: review its '
              'retention and disclosure policies.',
        ],
      );
    }

    return const AgePrivacyReport(
      level: AgePrivacyLevel.minimalDisclosureCandidate,
      reasons: <String>[
        'Declared data flow appears limited to an age-threshold claim.',
        'This is not a verified, certified or legally sufficient age proof.',
      ],
    );
  }
}
