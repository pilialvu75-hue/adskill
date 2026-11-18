import '../lib/age_privacy_preflight.dart';

const threshold = ProposedDisclosure(
  category: AgeDataCategory.ageAboveRequiredThreshold,
  recipient: DisclosureRecipient.website,
);

AgeCheckMetadata proposal({
  List<ProposedDisclosure> disclosures = const [threshold],
  bool? siteCanLink = false,
  bool? verifierKnowsSite = false,
  bool? retentionExplained = true,
  int age = 18,
}) =>
    AgeCheckMetadata(
      requiredAge: age,
      disclosures: disclosures,
      siteCanLinkProofToIdentity: siteCanLink,
      verifierLearnsWhichSite: verifierKnowsSite,
      retentionExplained: retentionExplained,
    );

void check(bool condition, String reason) {
  if (!condition) throw StateError('Privacy test failed: $reason');
}

void main() {
  const engine = AgePrivacyPreflight();
  final minimal = engine.inspect(proposal());
  check(minimal.level == AgePrivacyLevel.minimalDisclosureCandidate,
      'proof-only metadata should be only a candidate');
  check(!minimal.verifiedAge && !minimal.mayBypassSiteGate,
      'assessment must never unlock the age gate');

  final withIdentity = engine.inspect(proposal(disclosures: [
    threshold,
    const ProposedDisclosure(
      category: AgeDataCategory.identityDocument,
      recipient: DisclosureRecipient.website,
    ),
  ]));
  check(withIdentity.level == AgePrivacyLevel.highExposure,
      'site requesting document must be high-exposure');

  final withFace = engine.inspect(proposal(disclosures: [
    threshold,
    const ProposedDisclosure(
      category: AgeDataCategory.faceOrBiometricSample,
      recipient: DisclosureRecipient.website,
    ),
  ]));
  check(withFace.level == AgePrivacyLevel.highExposure,
      'site requesting biometric data must be high-exposure');

  final withVerifierDocument = engine.inspect(proposal(disclosures: [
    threshold,
    const ProposedDisclosure(
      category: AgeDataCategory.identityDocument,
      recipient: DisclosureRecipient.independentVerifier,
    ),
  ]));
  check(withVerifierDocument.level == AgePrivacyLevel.reviewRequired,
      'verifier document may be lawful but deserves review');

  final linkable = engine.inspect(proposal(siteCanLink: true));
  check(linkable.level == AgePrivacyLevel.highExposure,
      'site linkability must be high-exposure');
  final visitedSiteKnown = engine.inspect(proposal(verifierKnowsSite: true));
  check(visitedSiteKnown.level == AgePrivacyLevel.highExposure,
      'verifier knowing destination must be high-exposure');
  final unknownRetention = engine.inspect(
    proposal(retentionExplained: null),
  );
  check(unknownRetention.level == AgePrivacyLevel.reviewRequired,
      'unknown retention must not be marked minimal');
  final unknownLinkability = engine.inspect(proposal(siteCanLink: null));
  check(unknownLinkability.level == AgePrivacyLevel.reviewRequired,
      'unknown linkability must not be marked minimal');
  final noClaim = engine.inspect(proposal(disclosures: [
    const ProposedDisclosure(
      category: AgeDataCategory.legalName,
      recipient: DisclosureRecipient.website,
    ),
  ]));
  check(noClaim.level == AgePrivacyLevel.insufficientInformation,
      'missing age proof must not be accepted');
  final invalidAge = engine.inspect(proposal(age: 0));
  check(invalidAge.level == AgePrivacyLevel.insufficientInformation,
      'invalid threshold must fail closed');
  final blank = engine.inspect(proposal(disclosures: []));
  check(blank.level == AgePrivacyLevel.insufficientInformation,
      'empty metadata must fail closed');

  print('ADS Kill Privacy & Age: 11 privacy-only tests PASS');
}
